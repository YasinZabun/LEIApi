using Data.Enums;
using Data.Model;
using DataAccess;
using DataAccess.AccessInterfaces;
using DataAccess.Repositories;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Transactions;
using ZstdSharp.Unsafe;

namespace Business.Workflow
{
    public class LeiRecordBusiness
    {
        #region Members
        /// <summary>
        /// MongoDB data access class that coming from ioc container.
        /// </summary>
        private LeiRecordMongoAccess repository;
        #endregion

        #region Constructure
        public LeiRecordBusiness(LeiRecordMongoAccess repository)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        #endregion

        #region Methods
        
        #region Public

        /// <summary>
        /// Starts adding process then return result.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public string AddCsv(IFormFile file)
        {
            Stream stream = new MemoryStream();
            file.CopyTo(stream);//The file variable will collect by Garbage Collector so that my task will crash. Because at that time i am using file variable in task.
            stream.Position = 0;
            GC.SuppressFinalize(file);//Then i am collecting file variable.
            Task.Run(() => AddCsvToDb(stream));
            return "Your data is adding to database.";
        }

        /// <summary>
        /// Adds a lei record.
        /// </summary>
        /// <param name="leiRecordModel"></param>
        /// <returns></returns>
        public LeiRecordModel AddLeiRecord(LeiRecordModel leiRecordModel)
        {
            return repository.AddAsync(leiRecordModel).Result;
        }

        /// <summary>
        /// Gets all lei records then returns them.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LeiRecordModel> GetAll()
        {
            return repository.Get(_ => true);
        }

        /// <summary>
        /// Gets lei record by id or leiid 
        /// </summary>
        /// <param name="id">Object Id</param>
        /// <param name="getByLei">Specifies which field the id parameter belongs to. If true Id is belongs to Id. If false Id is belongs to leiid</param>
        /// <returns></returns>
        public LeiRecordModel? Get(string id, bool getByLei = false)
        {
            return getByLei ? repository.Get(x => x.Lei == id).FirstOrDefault() : repository.GetByIdAsync(id).Result;
        }

        /// <summary>
        /// Deletes lei record by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(string id)
        {
            return repository.DeleteAsync(id).Result == null ? false : true;
        }

        /// <summary>
        /// Updates a lei record according to the new values to be changed.
        /// </summary>
        /// <param name="leiRecordModel"></param>
        /// <returns></returns>
        public LeiRecordModel? Update(LeiRecordModel leiRecordModel)
        {
            return repository.UpdateAsync(leiRecordModel).Result;
        }
        
        #endregion

        #region Private

        /// <summary>
        /// Adds csv file to MongoDB.
        /// </summary>
        /// <param name="file"></param>
        private void AddCsvToDb(Stream file)
        {
            using (file)
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    sr.ReadLine();
                    string[]? leiRecordsAsString = sr.ReadToEnd().Split("\n");
                    Parallel.ForEach(leiRecordsAsString, leiRecordAsString =>
                    {
                        string[]? columns = leiRecordAsString.Split(",");
                        string? responseContent = SendLeiRequest(columns[7]);
                        string[]? bicField = responseContent != null ? Regex.Matches(Regex.Match(responseContent, "bic\":\\[(.*?)\\]").Groups[1].Value, "\"(.*?)\"").Select(str1 => str1.Groups[1].Value).ToArray() : null;
                        bool responseContentStatus = responseContent != null ? true : false;
                        LeiRecordModel leiRecordModel = new()
                        {
                            TransactionUti = columns[0],
                            Isin = columns[1],
                            Notional = Convert.ToDouble(columns[2]),
                            NotionalCurrency = columns[3],
                            TransactionType = columns[4],
                            TransactionDatetime = Convert.ToDateTime(columns[5]),
                            Rate = Convert.ToDouble(columns[6]),
                            Lei = Regex.Replace(columns[7], @"\r", ""),
                            Bic = bicField,
                            TransactionCost = responseContentStatus ? (Regex.Match(responseContent, "country\":\"GB").Success ? CardanoCalculation(CountryEnum.GB, columns[2], columns[6]) : (Regex.Match(responseContent, "country\":\"NL").Success ? CardanoCalculation(CountryEnum.NL, columns[2], columns[6]) : 0)) : 0,
                            LegalName = responseContentStatus ? Regex.Match(responseContent, "legalName[\":{a-z]*(.*?)\"").Groups[1].Value : String.Empty,
                        };
                        repository.AddAsync(leiRecordModel).Wait();
                    });
                }
            }
        }

        /// <summary>
        /// Calculates transaction cost based on notional and rate variable.
        /// </summary>
        /// <param name="country">Calculation logic seperator</param>
        /// <param name="notional">notional variable</param>
        /// <param name="rate">rate variable</param>
        /// <returns></returns>
        private double CardanoCalculation(CountryEnum country, string notional, string rate)
        {
            switch (country)
            {
                case CountryEnum.GB:
                    return Convert.ToDouble(notional) * Convert.ToDouble(rate) - Convert.ToDouble(notional);
                case CountryEnum.NL:
                    return Math.Abs(Convert.ToDouble(notional) * (1 / Convert.ToDouble(rate)) - Convert.ToDouble(notional));
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Sends request to globalapi(leiRecord) then get response content and returns.
        /// </summary>
        /// <param name="leiId">lei object id</param>
        /// <returns></returns>
        private string? SendLeiRequest(string leiId)
        {
            var client = new RestClient("https://api.gleif.org/api/v1/lei-records/");
            var request = new RestRequest(leiId);
            request.AddHeader("Accept", "application/vnd.api+json");
            RestResponse response = client.Execute(request);
            if (response != null && response.Content != null)
            {
                return response.Content;
            }
            return null;
        }

        #endregion
        
        #endregion
    }
}

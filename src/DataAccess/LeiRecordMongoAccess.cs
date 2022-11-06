using Data.Model;
using DataAccess.AccessInterfaces;
using DataAccess.Repositories;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Utils.Settings;

namespace DataAccess
{
    public class LeiRecordMongoAccess:MongoRepository<LeiRecordModel>,ILeiRecordMongoAccess
    {
        public LeiRecordMongoAccess(IOptions<MongoDbSettings> options):base(options)    
        {

        }
    }
}

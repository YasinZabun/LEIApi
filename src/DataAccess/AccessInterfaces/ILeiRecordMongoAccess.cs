using Data.Model;
using DataAccess.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.AccessInterfaces
{
    public interface ILeiRecordMongoAccess : IRepository<LeiRecordModel, string>
    {

    }
}

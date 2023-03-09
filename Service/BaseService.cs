using System;
using System.Collections.Generic;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oduna.Lib.Service
{
    public class BaseService
    {
        protected string dbConnStr = string.Empty;
        public BaseService()
        {
            // UOF Oracle Connection String
            this.dbConnStr = ConfigurationManager.ConnectionStrings["CDB"].ConnectionString;
        }

        public OracleConnection GetConnection()
        {
            return new OracleConnection(this.dbConnStr);
        }

        public string GetConnectionString()
        {
            return dbConnStr;
        }
    }
}

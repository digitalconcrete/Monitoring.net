using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace MonitorLib
{
    [DataContract(Namespace = "")]
    public abstract class Base
    {
        protected ILog log;
        private static List<Type> types = new List<Type>();
        public Base()
        {
            Deserialized();
        }

        [OnDeserialized]
        private void Deserialized(StreamingContext cxt=default(StreamingContext))
        {
            log = LogManager.GetLogger(GetType());
            if (!types.Contains(GetType()))
            {
                types.Add(this.GetType());
            }
        }
        public static void MergeTypes(Type[] _types)
        {
            Array.ForEach(_types,t=> { if(!types.Contains(t)) types.Add(t);});
        }
        public static Type[] GetTypes()
        {
            return types.ToArray();
        }
        public DbConnection GetConnection(string connStr, string providerName=null)
        {
            var csb = new DbConnectionStringBuilder { ConnectionString = connStr };
            if (providerName == null)
            {
                if (csb.ContainsKey("provider"))
                {
                    providerName = csb["provider"].ToString();
                }
                else
                {
                    var css = ConfigurationManager
                        .ConnectionStrings
                        .Cast<ConnectionStringSettings>()
                        .FirstOrDefault(x => x.ConnectionString == connStr);
                    if (css != null) providerName = css.ProviderName;
                }
            }
            if (providerName == null)
                return null;
            var providerExists = DbProviderFactories
                .GetFactoryClasses()
                .Rows.Cast<DataRow>()
                .Any(r => r[2].Equals(providerName));
            if (!providerExists)
                return null;
            var factory = DbProviderFactories.GetFactory(providerName);
            var dbConnection = factory.CreateConnection();

            dbConnection.ConnectionString = connStr;
            return dbConnection;
        }
    }
}

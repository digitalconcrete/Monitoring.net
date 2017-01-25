using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLib.Triggers
{
    [DataContract(Namespace = "")]
    public class QueryTrigger : TriggerBase
    {
        [DataMember]
        public string ConnectionString;
        [DataMember]
        public string ProviderName = "System.Data.SqlClient";
        [DataMember]
        public string Query;

        public override int Check()
        {
            using (var con = GetConnection(ConnectionString,ProviderName))
            {
                con.Open();
                var cmd = con.CreateCommand();
                cmd.CommandText = Query;
                var res=cmd.ExecuteScalar();
                if(res==null || res==DBNull.Value)
                    return 0;
                var value=Convert.ToInt32(res);
                return value;
            }
            
        }
    }
}

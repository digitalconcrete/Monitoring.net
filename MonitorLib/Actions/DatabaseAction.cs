using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLib.Actions
{
    [DataContract(Namespace = "")]
    public class DatabaseAction : ActionBase
    {
        [DataMember]
        public string QueryToExecute;
        public override bool Execute(int value)
        {
            log.Debug($"{QueryToExecute} : {value}");
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MonitorLib.Triggers;


namespace MonitorLib.Actions
{
    [DataContract(Namespace= "")]
    public abstract class ActionBase : Base
    {
        [DataMember]
        public bool Enabled = true;
        public abstract bool Execute(int value);
        public TriggerBase Trigger;
    }
}

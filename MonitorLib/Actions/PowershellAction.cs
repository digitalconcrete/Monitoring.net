using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLib.Actions
{
    [DataContract(Namespace="")]
    public class PowershellAction : ActionBase
    {
        [DataMember] public string CommandText;

        public override bool Execute(int value)
        {
            using (var powershell = PowerShell.Create())
            {
                powershell.AddScript(CommandText);
                
                powershell.AddParameter("thresholdValue", value);
                powershell.Invoke();
            }
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLib.Triggers
{
    [DataContract(Namespace="")]
    public class UsageTrigger : TriggerBase
    {
        public override int Check()
        {
            var searcher = new ManagementObjectSearcher("select * from Win32_PerfFormattedData_PerfOS_Processor where name = '_Total'");
            var lastUsage = 0;
            foreach (var obj in searcher.Get())
            {
                var usage = obj["PercentProcessorTime"];
                var name = obj["Name"];

                log.DebugFormat(name + " : " + usage);
                lastUsage = Convert.ToInt32(usage);
            }
            return lastUsage;
        }
    }
}

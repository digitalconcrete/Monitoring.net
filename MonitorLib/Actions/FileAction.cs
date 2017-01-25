using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLib.Actions
{
    [DataContract(Namespace = "")]
    public class FileAction : ActionBase
    {

        public override bool Execute(int value)
        {
            Console.WriteLine(value);
            return true;
        }
    }
}

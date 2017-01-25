using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MonitorLib.Actions
{
    [DataContract(Namespace = "")]
    public class Email : ActionBase
    {
        [DataMember]
        public string SmtpServer;

        [DataMember] public int SmtpPort = 25;
        [DataMember]
        public string User;
        [DataMember]
        public string Pass;
        [DataMember]
        public string From;
        [DataMember]
        public string To;
        public override bool Execute(int value)
        {
            if (!Enabled)
                return false;
            log.Debug($"Email trigger, got value: {value}");
            var msg = new MailMessage(From, To, $"Triggered event from {Dns.GetHostName()}", $"An event has been triggered based on a {Trigger.GetType().Name} named {Trigger.TriggerName} the threshold ({Trigger.ThresholdValue}) has been exceeded {Trigger.SequentialThresholdViolations} time(s).");
            using (var client = new SmtpClient(SmtpServer, SmtpPort))
            {
                if(!string.IsNullOrEmpty(User))
                    client.Credentials=new NetworkCredential(User,Pass);
                if (SmtpPort != 25)
                    client.EnableSsl = true;
                client.Send(msg);
            }
            return true;
        }

    }
}

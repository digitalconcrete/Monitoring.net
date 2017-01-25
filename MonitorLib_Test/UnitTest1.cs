using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MonitorLib.Actions;
using MonitorLib.Configuration;
using MonitorLib.Scheduler;
using MonitorLib.Triggers;

namespace MonitorLib_Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var nc = new NotificationConfig();
            nc.Triggers.Add(new QueryTrigger()
            {
                ConnectionString = "Data Source=localhost;Initial Catalog=PPS_Service;User ID=ppsService;Password=r1v3r4398",
                Query="select count(*) from grader_page (nolock) where status_id = 99",
                ThresholdValue = 90,
                TriggerAfterNEvents = 5,
                CronEntry = "* * * * *",
                TriggerName = "PPS Queue Monitor",
                Action =new Email()
                {
                    SmtpServer = "smtp.hmhco.com",
                    From="monitorService@hmhco.com",
                    To= "HMH-PT-DataDirectorMonitoring@hmhco.com, ron.trolard@hmhco.com",
                    Enabled =true
                }
            });
            nc.Triggers.Add(new QueryTrigger()
            {
                ConnectionString = "Data Source=localhost;Initial Catalog=PPS_Service;User ID=ppsService;Password=r1v3r4398",
                Query = "select count(*) from grader_page (nolock) where status_id = 99",
                ThresholdValue = -1,
                TriggerAfterNEvents = 0,
                CronEntry = "* * * * * *",
                TriggerName = "PPS Queue Stats Accumulator",
                Action = new LogAccumulator()
                {
                    SmtpServer = "smtp.hmhco.com",
                    From = "monitorService@hmhco.com",
                    To = "ron.trolard@hmhco.com",
                    EmailInterval = 720,
                    Enabled = true
                }
            });
            nc.Triggers.Add(new UsageTrigger()
            {
                ThresholdValue = 90,
                TriggerAfterNEvents = 5,
                Action = new Email()
                {
                    SmtpServer = "smtp.hmhco.com",
                    From = "monitorService@hmhco.com",
                    To = "ron.trolard@hmhco.com",
                    Enabled = true
                }

            });
            nc.SerializeToFile("config.xml");

            var sr = ScheduleRunner.Initialize("config.xml");
            Thread.Sleep(TimeSpan.FromMinutes(2));
            /*foreach (var t in nc.Triggers)
            {
                Console.WriteLine(t.NextRunDate());
                Console.WriteLine(t.Execute());
            }*/
            sr.Stop();
        }

        [TestMethod]
        public void Deserialize()
        {
            var nc = NotificationConfig.DeserializeFromFile("config.xml");
            Console.WriteLine(nc);
        }
    }
}

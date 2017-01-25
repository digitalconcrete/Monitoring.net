using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using log4net;
using MonitorLib.Scheduler;

namespace Monitoring
{
    public partial class MonitorService : ServiceBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MonitorService));
        public MonitorService()
        {
            InitializeComponent();
        }

        private ScheduleRunner sr;
        protected override void OnStart(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            log.Debug($"Starting service from {Assembly.GetExecutingAssembly().Location }");
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            
            sr = ScheduleRunner.Initialize(path + "\\config.xml");
        }

        protected override void OnStop()
        {
            log.Debug("Stopping service");
            sr.Stop();
        }
    }
}

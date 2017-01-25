using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using MonitorLib.Configuration;

namespace MonitorLib.Scheduler
{
    public class ScheduleRunner
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ScheduleRunner));
        private Thread _runnerThread;
        private static ScheduleRunner _instance;
        private bool running = true;
        private NotificationConfig configuration = null;
        public static ScheduleRunner Initialize(string configPath, bool start=true)
        {
            log.Debug("Schedule runner starting!");
            if (_instance == null)
                _instance = new ScheduleRunner();
            try
            {
                _instance.configuration = NotificationConfig.DeserializeFromFile(configPath);
            }
            catch (Exception e)
            {
                log.Error("Error starting",e);
            }
            if (start)
                _instance.Start();
            return _instance;
        }

        private ScheduleRunner()
        {
            log4net.Config.XmlConfigurator.Configure();
        }
        public void Start()
        {
            if (_runnerThread == null)
            {
                _runnerThread = new Thread(Run)
                {
                    Name = "ScheduleRunner",
                    IsBackground = true
                };
            }
            if ((_runnerThread.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
                _runnerThread.Start();
        }

        public void Stop()
        {
            running = false;
            try
            {
                _runnerThread.Interrupt();
            } catch { }
            _runnerThread = null;
        }
        private void Run()
        {
            while (running)
            {
                try
                {
                    Thread.Sleep(TimeSpan.FromSeconds(15));
                    if (configuration == null)
                        continue;
                    foreach (var trigger in configuration.Triggers)
                    {
                        log.Debug($"trigger: {trigger.TriggerName} will run at or after {trigger.NextRunDate()}");
                        if(DateTime.Now > trigger.NextRunDate())
                            trigger.Execute();
                    }
                }
                catch (Exception e)
                {
                    
                    log.Error(e.Message,e);
                }
            }
        }
    }
}

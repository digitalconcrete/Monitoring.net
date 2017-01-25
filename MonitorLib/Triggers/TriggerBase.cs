using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MonitorLib.Actions;
using NCrontab;

namespace MonitorLib.Triggers
{
    [DataContract(Namespace = "")]
    public abstract class TriggerBase : Base
    {
        [DataMember]
        public ActionBase Action;
        [DataMember]
        public TriggerBase ChildTrigger;
        [DataMember] public int ThresholdValue;
        [DataMember] public string TriggerName;
        [DataMember] public int TriggerAfterNEvents;
        private string _cron="* * * * *";
        [DataMember]
        public string CronEntry
        {
            get { return _cron; }
            set
            {
                _cron = value;

                _schedule = CrontabSchedule.Parse(_cron, new CrontabSchedule.ParseOptions() {IncludingSeconds = CountSpaces(_cron)==5});
            }
        }

        protected int CountSpaces(string s)
        {
            return s.Count(char.IsWhiteSpace);
        }
        public abstract int Check();

        public bool Execute()
        {
            var lastValue = Check();
            _nextRunDate = _schedule.GetNextOccurrence(DateTime.Now);
            log.DebugFormat("Last Value: {0}", lastValue);
            if (lastValue > ThresholdValue)
            {
                SequentialThresholdViolations++;
                if(Action!=null)
                    Action.Trigger = this;
                if(TriggerAfterNEvents == 0 || SequentialThresholdViolations >= TriggerAfterNEvents)
                    return Action?.Execute(lastValue) ?? false;
            }
            else 
                SequentialThresholdViolations = 0;
            return true;
        }

        internal int SequentialThresholdViolations = 0;
        private CrontabSchedule _schedule;
        private DateTime _nextRunDate=DateTime.MinValue;
        
        
        public DateTime NextRunDate()
        {
            if(_schedule==null)
                _schedule = CrontabSchedule.Parse(_cron);
            if (_nextRunDate==DateTime.MinValue)
                _nextRunDate=_schedule.GetNextOccurrence(DateTime.Now);
            return _nextRunDate;
        }
    }
}

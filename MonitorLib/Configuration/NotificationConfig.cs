using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MonitorLib.Actions;
using MonitorLib.Scheduler;
using MonitorLib.Triggers;

namespace MonitorLib.Configuration
{
    [DataContract(Namespace = "")]
    public class NotificationConfig
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(NotificationConfig));
        private static DataContractSerializer _serializer;

        private static DataContractSerializer Serializer
        {
            get
            {
                if (_serializer != null)
                    return _serializer;
                var abTypes = typeof(ActionBase).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ActionBase))).ToArray();
                var tbTypes = typeof(TriggerBase).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(TriggerBase))).ToArray();
                Base.MergeTypes(abTypes);
                Base.MergeTypes(tbTypes);
                _serializer = new DataContractSerializer(typeof(NotificationConfig), Base.GetTypes());
                return _serializer;
            }
        }
        [DataMember]
        public readonly List<TriggerBase> Triggers=new List<TriggerBase>();

        public void SerializeToFile(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                Serializer.WriteObject(fs, this);
            }
        }

        public static NotificationConfig DeserializeFromFile(string path) 
        {
            NotificationConfig deserialized = null;
            var configFile = new FileInfo(path);
            log.Debug($"Deserializing from {configFile.FullName} and it { (configFile.Exists ? "exists" : "doesn't exist") }");
            if (!File.Exists(path))
            {
                deserialized = new NotificationConfig();
                deserialized.SerializeToFile(path);
            }
            try
            {
                log.Debug($"Opening config file");
                using (var fs = new FileStream(path, FileMode.Open,FileAccess.Read))
                {
                    deserialized = Serializer.ReadObject(fs) as NotificationConfig;
                }
                foreach (var c in deserialized.Triggers)
                {
                    log.Debug($"Loaded trigger {c.TriggerName} type: {c.GetType().Name}");
                }
            }
            catch (Exception e)
            {
                log.Error("Deserialization error", e);
            }
            log.Debug("Deserialization complete");
            return deserialized;

        }

        public string SerializeToXml()
        {
            byte[] data;
            using (var ms = new MemoryStream())
            {
                Serializer.WriteObject(ms, this);
                data = ms.ToArray();
            }
            return Encoding.UTF8.GetString(data);
        }

        public static NotificationConfig Deserialize(byte[] xml) 
        {
            NotificationConfig deserialized;
            using (var ms = new MemoryStream(xml))
            {
                deserialized = Serializer.ReadObject(ms) as NotificationConfig;
            }
            return deserialized;
        }

    }
}

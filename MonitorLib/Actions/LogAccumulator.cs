using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MonitorLib.Triggers;

namespace MonitorLib.Actions
{
    [DataContract(Namespace = "")]
    public class LogAccumulator : ActionBase
    {
        [DataMember]
        public string SmtpServer;

        [DataMember]
        public int SmtpPort = 25;
        [DataMember]
        public string User;
        [DataMember]
        public string Pass;
        [DataMember]
        public string From;
        [DataMember]
        public string To;

        private int _minValue;
        private int _maxValue;
        
        private int _currentCounter = 0;
        private Dictionary<DateTime, int> _detailedInfo=new Dictionary<DateTime, int>();
        private Dictionary<DateTime, int> _processorUsage = new Dictionary<DateTime, int>();
        [DataMember] public int EmailInterval = 720;
        private DateTime lastSend = DateTime.MinValue;
        private UsageTrigger _cpuTrigger=new UsageTrigger();
        [OnDeserialized()]
        private void Deserialized(StreamingContext cxt = default(StreamingContext))
        {
            if(_detailedInfo==null)
                _detailedInfo = new Dictionary<DateTime, int>();
            if(_processorUsage==null)
                _processorUsage=new Dictionary<DateTime, int>();
            if(_cpuTrigger==null)
                _cpuTrigger=new UsageTrigger();
        }
        public override bool Execute(int value)
        {
            var now = DateTime.Now;
            _detailedInfo.Add(now, value);
            var cpuVal = _cpuTrigger.Check();
            _processorUsage.Add(now, cpuVal);
            if (lastSend == DateTime.MinValue)
                lastSend = DateTime.Now;
            if (value < _minValue)
                _minValue = value;
            if (value > _maxValue)
                _maxValue = value;
            _currentCounter++;
            if (_currentCounter <= EmailInterval)
                return true;
            if(_maxValue > 0)
                Send();
            _currentCounter = 0;
            lastSend = DateTime.Now;
            _detailedInfo.Clear();
            _processorUsage.Clear();
            _maxValue = 0;
            _minValue = 0;
            
            return true;
        }

        public string DetailedView()
        {
            var sb = new StringBuilder();
            var sortedKeys = _detailedInfo.Keys.ToList();
            sortedKeys.Sort();
            foreach (var e in sortedKeys)
            {
                sb.Append($"Value at {e}: {_detailedInfo[e]}\r\n");
            }
            return sb.ToString();
        }

        public string DetailedGraph()
        {
            var sb = new StringBuilder();
            const int width = 1200;
            const int height = 400;
            sb.Append($"<svg width=\"{width}\" height=\"{height}\">");
            var sortedKeys = _detailedInfo.Keys.ToList();
            sortedKeys.Sort();
            var cx = 0f;
            var pointsPerMarker = (float)width/_detailedInfo.Count;
            var points = new StringBuilder();
            var cpuPoints = new StringBuilder();
            if (_maxValue == 0)
                return "";
            float lastTextOut = 0;
            foreach (var e in sortedKeys)
            {
                var val = height - ((_detailedInfo[e] / (float)_maxValue) * height);
                points.Append($" {cx},{Math.Round(val)} ");
                if ((cx - lastTextOut) > 25)
                {
                    sb.Append($"<text x=\"{cx}\" y=\"0\" fill=\"blue\" transform=\"rotate(90 {cx},0)\">{e}</text>");
                    lastTextOut = cx;
                }
                // add cpu info
                val = height - ((_processorUsage[e] / 100f) * height);
                cpuPoints.Append($" {cx},{Math.Round(val)} ");
                cx += pointsPerMarker;
            }
            sb.Append($"<polyline points=\"{cpuPoints}\" style = \"fill:none;stroke:black;stroke-width:1\" />" +
                      $"<polyline points=\"{points}\" style=\"fill: none; stroke: green; stroke-width:1\" />");
                     sb.Append("</svg>");
            return sb.ToString();

        }
        private void Send()
        {
            if (!Enabled)
                return;
            var body = $@"<!DOCTYPE html>
<html>
<body>Log from {Trigger.GetType().Name} named {Trigger.TriggerName}.
<br/>Accumulated since: {lastSend}
<br/>Min Value: {_minValue}
<br/>Max Value: {_maxValue}
<br/>
{DetailedGraph()} 
</body>
</html>";
            log.Debug(body);
            var msg = new MailMessage(From, To, $"Log message from {Dns.GetHostName()}", $"Log from {Trigger.GetType().Name} named {Trigger.TriggerName}.\r\nAccumulated since: {lastSend}\r\nMin Value: { _minValue}\r\nMax Value: { _maxValue}");
            msg.Attachments.Add(new Attachment(new MemoryStream(Encoding.ASCII.GetBytes(body)), new ContentType("text/html")));
            //msg.IsBodyHtml = true;

            using (var client = new SmtpClient(SmtpServer, SmtpPort))
            {
                if (!string.IsNullOrEmpty(User))
                    client.Credentials = new NetworkCredential(User, Pass);
                if (SmtpPort != 25)
                    client.EnableSsl = true;
                client.Send(msg);
            }

        }
    }
}

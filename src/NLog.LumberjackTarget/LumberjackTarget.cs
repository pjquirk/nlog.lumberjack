﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NLog.Common;
using Newtonsoft.Json;
using NLog.Config;


namespace NLog.Targets.Lumberjack
{
    [Target("Lumberjack")]
    public class LumberjackTarget : TargetWithLayout
    {
        private LumberjackProtocol _protocol = new LumberjackProtocol();
        private Transport _transport;

        #region parameters
        [RequiredParameter]
        public string Host { get; set; }
        public int Port { get; set; } = 5000;
        [RequiredParameter]
        public string Thumbprint { get; set; }
        #endregion

        protected override void InitializeTarget()
        {
            if (_transport != null)
            {
                _transport.Dispose();
            }
            _transport = new Transport()
            {
                Host = Host,
                Port = Port,
                Thumbprint = Thumbprint,
            };
        }

        protected override void CloseTarget()
        {
            _transport.Dispose();
            _transport = null;
        }

        protected override void Write(AsyncLogEventInfo logEvent)
        {
            var packet = CreatePacket(logEvent.LogEvent);
            _transport.Send(packet);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var packet = CreatePacket(logEvent);
            _transport.Send(packet);
        }

        protected override void Write(AsyncLogEventInfo[] logEvents)
        {
            Array.ForEach(logEvents, e => Write(e));
        }

        private byte[] CreatePacket(LogEventInfo logEvent)
        {
            var data = logEvent.Properties["data"] as LumberjackMessageBase;
            var log = new Dictionary<string, object>
            {
                { "source", data.Source  },
                { "app_id", data.ApplicationId },
                { "component", data.Component },
            };

            if (!string.IsNullOrWhiteSpace(data.MachineName))
            {
                log["machine_name"] = data.MachineName.ToLowerInvariant();
            }

            {
                var message = data as LumberjackLogMessage;
                if (message != null)
                {
                    log["type"] = "logs";
                    log["level"] = message.Level.Name;
                    log["line"] = message.Message;
                    log["props"] = JsonConvert.SerializeObject(new Dictionary<string, object>
                    {
                        { "fields", message.Fields },
                        { "tags", message.Tags }
                    });

                    return _protocol.CreatePacket(log, logEvent.SequenceID);
                }
            }

            {
                var message = data as LumberjackMetricMessage;
                if (message != null)
                {
                    log["type"] = "metric";
                    if (!string.IsNullOrWhiteSpace(data.MachineName))
                    {
                        log["line"] = string.Format("{0}.{1}.{2}.{3}.{4} {5} {6}", data.Source, data.MachineName.ToLowerInvariant(), data.ApplicationId, data.Component, message.Name, message.Value, message.UnixTimestamp);
                    }
                    else
                    {
                        log["line"] = string.Format("{0}.{1}.{2}.{3} {4} {5}", data.Source, data.ApplicationId, data.Component, message.Name, message.Value, message.UnixTimestamp);
                    }
                    return _protocol.CreatePacket(log, logEvent.SequenceID);
                }


            }

            {
                var message = data as LumberjackAlertMessage;
                if (message != null)
                {
                    log["type"] = "alert";
                    log["line"] = message.Text;
                    log["rule"] = message.RuleName;

                    return _protocol.CreatePacket(log, logEvent.SequenceID);
                }
            }

            throw new NotSupportedException("Log type is not supported");
        }
    }
}

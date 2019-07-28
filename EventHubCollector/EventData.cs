using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace EventHubCollector
{
    [DataContract(Name = "EventData", Namespace = "Microsoft.ServiceBus.Messaging")]
    public class EventData
    {
        [DataMember(Name = "SequenceNumber")]
        public long SequenceNumber { get; set; }

        [DataMember(Name = "Offset")]
        public string Offset { get; set; }

        [DataMember(Name = "EnqueuedTimeUtc")]
        public DateTime EnqueuedTimeUtc { get; set; }

        [DataMember(Name = "SystemProperties")]
        public Dictionary<string, object> SystemProperties { get; set; }

        [DataMember(Name = "Properties")]
        public Dictionary<string, object> Properties { get; set; }

        [DataMember(Name = "Body")]
        public byte[] Body { get; set; }

        public EventData(dynamic record)
        {
            SequenceNumber = (long)record.SequenceNumber;
            Offset = (string)record.Offset;
            DateTime.TryParse((string)record.EnqueuedTimeUtc, out var enqueuedTimeUtc);
            EnqueuedTimeUtc = enqueuedTimeUtc;
            SystemProperties = (Dictionary<string, object>)record.SystemProperties;
            Properties = (Dictionary<string, object>)record.Properties;
            Body = (byte[])record.Body;
        }


    }
}

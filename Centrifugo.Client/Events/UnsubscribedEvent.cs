using System;

namespace Centrifugo.Client.Events
{
    public class UnsubscribedEvent
    {
//        public bool ShouldResubscribe { get; }
        public UInt32 Code { get; }
        public string Reason { get; }

        public string Channel { get; }

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public UnsubscribedEvent(string channel, UInt32 code, string reason)
        {
//            ShouldResubscribe = shouldResubscribe;
            Code = code;
            Reason = reason;
            Channel = channel;
        }
    }
}
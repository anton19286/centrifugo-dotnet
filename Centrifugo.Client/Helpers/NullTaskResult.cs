using System;
using System.Threading.Tasks;
using Google.Protobuf;
using Protocol;

namespace Centrifugo.Client.Helpers
{
    public static class NullTaskResult
    {
        public static Task<Reply> Instance { get; } = Task.FromResult(new Reply());

        public static Task<Reply> NotConnected { get; } = Task.FromException<Reply>(new Exception("Not connected"));
    }
}
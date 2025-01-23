using System;
using System.Collections.Generic;
using System.Text;

namespace Game.Common.Encryption
{
    public class AuthData
    {
        public string PlayFabId { get; set; }
        public string SessionTicket { get; set; }
        public long Timestamp { get; set; }
        public int ProtocolVersion { get; set; }
        public string DeviceFingerprint { get; set; }
        public string Nonce { get; set; }
    }
}

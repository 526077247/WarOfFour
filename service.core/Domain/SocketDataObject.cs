﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service.SocketCore
{
    [ProtoContract]
    public class SocketDataObject
    {
        [ProtoMember(1)]
        public string ServiceName { get; set; }
        [ProtoMember(2)]
        public string MethodName { get; set; }
        [ProtoMember(3)]
        public string Paras { get; set; }
        public string ClientId { get; set; }
    }
}

using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service.SocketCore
{
    [ProtoContract]
    public class SocketDataObject
    {
        /// <summary>
        /// 服务名
        /// </summary>
        [ProtoMember(1)]
        public string ServiceName { get; set; }
        /// <summary>
        /// 方法名
        /// </summary>
        [ProtoMember(2)]
        public string MethodName { get; set; }
        /// <summary>
        /// 参数
        /// </summary>
        [ProtoMember(3)]
        public string Paras { get; set; }
        /// <summary>
        /// 时间
        /// </summary>
        [ProtoMember(4)]
        public string Time { get; set; }
        /// <summary>
        /// 会话标识
        /// </summary>
        [ProtoMember(5)]
        public string ClientId { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        [ProtoMember(6)]
        public string Version { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirplaneCrash.Entity
{
    public class MessageEntity
    {
        public MessageType Code { get; set; }


        public string Message { get; set; }

    }

    public class RequestMessage :MessageEntity
    {

        /// <summary>
        /// 请求地址
        /// </summary>
        public string RequestIpAddress { get; set; }
    }

}

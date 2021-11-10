using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirplaneCrash.Server.Entity
{
    internal class BattleUser
    {
        /// <summary>
        /// 用户编号
        /// </summary>
        public int UserSysNo { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        public UserStatus Status { get; set; }

        /// <summary>
        /// 对战分数
        /// </summary>
        public int BattleSocre { get; set; }

        /// <summary>
        /// 上一次心跳时间
        /// </summary>
        public DateTime LastTime { get; set; }

    }




}

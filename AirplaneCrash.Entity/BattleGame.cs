using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirplaneCrash.Entity
{
    public class BattleGame
    {

        public string GameId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        /// <summary>
        /// 对局状态
        /// </summary>
        public GameStatus Status { get; set; }

        /// <summary>
        /// 当前用户
        /// </summary>
        public BattleUser CurrentUser { get; set; }

        /// <summary>
        /// 对局回合
        /// </summary>
        public int Round { get; set; }

        /// <summary>
        /// 对战用户集合
        /// </summary>
        public List<BattleUser> BattleUsers { get; set; }

        /// <summary>
        /// 用户飞机
        /// </summary>
        public Dictionary<int,List<BattleAirplane>> UserAirplane { get; set; }

    }


    public class BattleGameUser
    {
        public string GameId { get; set; }

        public int UserSysNo { get; set; }

        public int Round { get; set; }

        public List<BattleAirplane> Airplane { get; set; }

    }

    public class BattleGameUserChoice
    {
        public string GameId { get; set; }

        public int UserSysNo { get; set; }

        /// <summary>
        /// 坐标X
        /// </summary>
        public string LocationX { get; set; }

        /// <summary>
        /// 坐标Y
        /// </summary>
        public string LocationY { get; set; }

        /// <summary>
        /// 是否点击
        /// </summary>
        public bool IsClick { get; set; }

    }
}

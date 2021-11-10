using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirplaneCrash.Server.Entity
{
    /// <summary>
    /// 飞机基础信息实体
    /// </summary>
    internal class AirplaneEntity
    {
        /// <summary>
        /// 坐标X
        /// </summary>
        public string LocationX { get; set; }

        /// <summary>
        /// 坐标Y
        /// </summary>
        public string LocationY { get; set; }

        /// <summary>
        /// 飞机方向
        /// </summary>
        public AirplaneDirect Direct { get; set; }

        /// <summary>
        /// 飞机部位
        /// </summary>
        public AirplanePosition Position { get; set; }

        /// <summary>
        /// 是否被摧毁
        /// </summary>
        public bool IsCrash { get; set; }

    }


    /// <summary>
    /// 对局飞机
    /// </summary>
    internal class BattleAirplane
    {

        /// <summary>
        /// 是否被摧毁
        /// </summary>
        public bool IsCrash { get; set; }

        /// <summary>
        /// 分数
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// 飞机部位
        /// </summary>
        public List<AirplaneEntity> AirPlanePositions { get; set; }
    }
}

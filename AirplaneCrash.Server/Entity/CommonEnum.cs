namespace AirplaneCrash.Server.Entity
{
    using System.ComponentModel;

    internal enum AirplaneDirect
    {
        [Description("上")] Top = 0,
        [Description("下")] Down = 1,
        [Description("左")] Left = 2,
        [Description("右")] Right = 3
    }

    internal enum AirplanePosition
    {
        [Description("机头")] Head = 0,
        [Description("机身")] Body = 1,
        [Description("机翼")] Airfoil = 2,
        [Description("机尾")] Tail = 3


    }

    internal enum UserStatus
    {
        [Description("正常")] Normal = 0,
        [Description("等待")] Wait = 1,
        [Description("预备中")] Prepare = 2,
        [Description("对战中")] Battle = 3

    }

    internal enum GameStatus
    {
        [Description("预备飞机")] Prepared = 0,
        [Description("暂停")] Pause = 1,
        [Description("对局中")] Running = 2,
        [Description("小局结束")] RoundOver = 3,
        [Description("游戏结束")] Over = 4,
    }


    internal enum MessageType
    {
        [Description("心跳")] HeartBeat = 0,
        [Description("登录")] Login = 1,
        [Description("准备开始")] Prepare = 2,
        [Description("飞机数据")] AirPlaneData = 3,
        [Description("选择目标")] TargetChoice = 4,
        [Description("游戏数据")] GameData = 5,
    }
}

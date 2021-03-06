namespace AirplaneCrash.Business
{
    using System;
    using Newtonsoft.Json;
    using AirplaneCrash.Entity;
    using AirplaneCrash.Model;
    using AirplaneCrash.Core.Hub;

    [Hub(Name = "飞机数据集线器", Condition1 = 1000, Condition2 = (int)MessageType.AirPlaneData, Condition4 = "飞机数据")]
    public class AirPlaneDataHandle : IHub<RequestMessage, int>
    {
        public int Handle(RequestMessage obj)
        {
            BattleGameUser gameuser = JsonConvert.DeserializeObject<BattleGameUser>(obj.Message);
            GameModel.Instance.RefreshBattleGameUser(gameuser);

            return gameuser.UserSysNo;
        }
    }
}

namespace AirplaneCrash.Business
{
    using System;
    using Newtonsoft.Json;
    using AirplaneCrash.Entity;
    using AirplaneCrash.Model;
    using AirplaneCrash.Core.Hub;

    [Hub(Name = "目标选择集线器", Condition1 = 1000, Condition2 = (int)MessageType.TargetChoice, Condition4 = "选择目标")]
    public class TargetChoiceHandle : IHub<RequestMessage, int>
    {
        public int Handle(RequestMessage obj)
        {
            BattleGameUserChoice choice = JsonConvert.DeserializeObject<BattleGameUserChoice>(obj.Message);
            GameModel.Instance.RefreshBattleGameCrash(choice);

            return choice.UserSysNo;
        }
    }
}

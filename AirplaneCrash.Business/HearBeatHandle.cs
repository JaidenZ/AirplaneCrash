namespace AirplaneCrash.Business
{
    using System;
    using Newtonsoft.Json;
    using AirplaneCrash.Entity;
    using AirplaneCrash.Model;
    using AirplaneCrash.Core.Hub;

    [Hub(Name = "心跳集线器", Condition1 = 1000, Condition2 = (int)MessageType.HeartBeat, Condition4 = "心跳")]
    public class HearBeatHandle : IHub<MessageEntity, int>
    {
        public int Handle(MessageEntity obj)
        {
            if (obj == null)
                throw new ArgumentNullException("心跳用户信息参数为空");

            BattleUser user = (BattleUser)JsonConvert.DeserializeObject(obj.Message);
            BattleUserModel.Instance.ProcessBattleUserHear(user);

            return user.UserSysNo;
        }
    }
}

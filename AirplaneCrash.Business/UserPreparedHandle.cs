namespace AirplaneCrash.Business
{
    using Newtonsoft.Json;
    using AirplaneCrash.Entity;
    using AirplaneCrash.Model;
    using AirplaneCrash.Core.Hub;

    [Hub(Name = "用户准备集线器", Condition1 = 1000, Condition2 = (int)MessageType.Prepare, Condition4 = "准备开始")]
    public class UserPreparedHandle : IHub<MessageEntity, int>
    {
        public int Handle(MessageEntity obj)
        {

            BattleUser prepare = (BattleUser)JsonConvert.DeserializeObject(obj.Message);
            BattleUserModel.Instance.ChangeBattleUserStatus(prepare);

            return prepare.UserSysNo;
        }
    }
}

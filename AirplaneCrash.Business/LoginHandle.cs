namespace AirplaneCrash.Business
{
    using System;
    using Newtonsoft.Json;
    using AirplaneCrash.Entity;
    using AirplaneCrash.Model;
    using AirplaneCrash.Core.Hub;

    [Hub(Name = "登录集线器", Condition1 = 1000, Condition2 = (int)MessageType.Login, Condition4 = "登录")]
    public class LoginHandle : IHub<MessageEntity, int>
    {
        public int Handle(MessageEntity obj)
        {
            if (obj == null)
                throw new ArgumentNullException("登录用户信息参数为空");

            BattleUser login = (BattleUser)JsonConvert.DeserializeObject(obj.Message);

            BattleUser user = new BattleUser();
            user.UserSysNo = login.UserSysNo == 0 ? int.Parse(Guid.NewGuid().ToString()) : login.UserSysNo;
            user.NickName = login.NickName;
            user.Status = UserStatus.Normal;
            user.BattleSocre = 0;
            user.LastTime = DateTime.Now;
            BattleUserModel.Instance.AddBattleUser(user);

            return user.UserSysNo;

        }
    }
}

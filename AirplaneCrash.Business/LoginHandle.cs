using AirplaneCrash.Core.Hub;
using AirplaneCrash.Core.Utilits;
using AirplaneCrash.Entity;
using AirplaneCrash.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirplaneCrash.Business
{



    [HubAttribute(Name = "登录集线器", Condition1 = 1000, Condition2 = (int)MessageType.Login, Condition3 = 0, Condition4 = "登录", Condition5 = "", Condition6 = "")]
    public class LoginHandle : IHub<MessageEntity, int>
    {
        public int Handle(MessageEntity obj)
        {
            BattleUser login = (BattleUser)JsonConvert.DeserializeObject(obj.Message);
            BattleUser user = new BattleUser();
            user.UserSysNo = login.UserSysNo == 0 ? int.Parse(Guid.NewGuid().ToString()) : login.UserSysNo;
            user.NickName = login.NickName;
            user.Status = UserStatus.Normal;
            user.BattleSocre = 0;
            user.LastTime = DateTime.Now;
            //BattleContainer.GetInstance().AddBattleUser(user);

            BattleUserModel.Instance.AddBattleUser(user);


            return user.UserSysNo;

        }
    }
}

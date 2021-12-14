namespace AirplaneCrash.Model
{

    using AirplaneCrash.Entity;
    using System.Collections.Generic;
    using System.Linq;

    public class BattleUserModel
    {

        private static BattleUserModel _instance;

        private List<BattleUser> allBattleUsers = new List<BattleUser>();

        public delegate void BattleUserInfoRefreshHandle(BattleUser user);
        public event BattleUserInfoRefreshHandle BattleUserInfoLogin;//用户信息更新事件
        public event BattleUserInfoRefreshHandle BattleUserHearbeat;//用户心跳更新事件

        public BattleUserModel()
        {



        }

        public static BattleUserModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new BattleUserModel();
                return _instance;
            }
        }

        public void AddBattleUser(BattleUser user)
        {
            var battlerUser = allBattleUsers.FirstOrDefault(s => s.UserSysNo == user.UserSysNo);

            if (battlerUser == null)
            {
                allBattleUsers.Add(user);
            }
            else
            {
                UpdateBattleUser(user);
            }

            if(BattleUserInfoLogin != null)
                BattleUserInfoLogin(user);
        }


        public void ProcessBattleUserHear(BattleUser user)
        {
            var battlerUser = allBattleUsers.FirstOrDefault(s => s.UserSysNo == user.UserSysNo);
            if (battlerUser == null)
                return;

            battlerUser.LastTime = user.LastTime;

            if (BattleUserHearbeat != null)
                BattleUserHearbeat(battlerUser);
        }

        public void UpdateBattleUser(BattleUser user)
        {
            var battlerUser = allBattleUsers.FirstOrDefault(s => s.UserSysNo == user.UserSysNo);
            if (battlerUser == null)
                return;
            battlerUser.NickName = user.NickName;
            battlerUser.LastTime = user.LastTime;
        }

        public void RemoveBattleUser(BattleUser user)
        {
            var battlerUser = allBattleUsers.FirstOrDefault(s => s.UserSysNo == user.UserSysNo);

            if (battlerUser == null)
                allBattleUsers.Remove(user);
        }

        public void ChangeBattleUserStatus(BattleUser user)
        {
            var battlerUser = allBattleUsers.FirstOrDefault(s => s.UserSysNo == user.UserSysNo);
            if (battlerUser == null)
                return;
            battlerUser.Status = user.Status;
        }

        public void ChangeBattleUserScore(BattleUser user)
        {
            var battlerUser = allBattleUsers.FirstOrDefault(s => s.UserSysNo == user.UserSysNo);
            if (battlerUser == null)
                return;
            battlerUser.BattleSocre = user.BattleSocre;
        }

        /// <summary>
        /// 根据用户状态获取用户集合
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public List<BattleUser> GetUserByStatus(UserStatus status)
        {
            return allBattleUsers.Where(s => s.Status == status).ToList();
        }


    }
}

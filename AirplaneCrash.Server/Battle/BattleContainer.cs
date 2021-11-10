using AirplaneCrash.Server.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AirplaneCrash.Server.Battle
{
    internal class BattleContainer
    {
        private const int roundAirplaneCount = 3;
        private const int roundCount = 3;

        private static BattleContainer _container;

        private Dictionary<string, BattleGame> battleGames = new Dictionary<string, BattleGame>();

        private List<BattleUser> allBattleUsers = new List<BattleUser>();

        public delegate void BattleGameChangeHandle(BattleGame game);
        public event BattleGameChangeHandle ChangeHandle;

        public delegate void BattleGameUserChoiceHandle(BattleUser sendBattleUser, BattleGameUserChoice userChoice);
        public event BattleGameUserChoiceHandle UserChoiceHandle;

        internal BattleContainer()
        {
            Thread th = new Thread(() =>
            {
                while (true)
                {
                    CreatBattleGame();
                    Thread.Sleep(1000 * 2);
                }
            });
            th.Start();
        }

        public static BattleContainer GetInstance()
        {
            if (_container == null)
                _container = new BattleContainer();
            return _container;
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
        /// 准备阶段刷新游戏玩家数据
        /// </summary>
        /// <param name="gameUser"></param>
        public void RefreshBattleGameUser(BattleGameUser gameUser)
        {
            if (!gameUser.Airplane.Any())
                return;
            if (gameUser.Airplane.Count() != roundAirplaneCount)
                return;
            if (!battleGames.ContainsKey(gameUser.GameId))
                return;
            if (battleGames[gameUser.GameId] == null)
                return;
            if (battleGames[gameUser.GameId].Status != GameStatus.Prepared)
                return;


            if (!battleGames[gameUser.GameId].UserAirplane.ContainsKey(gameUser.UserSysNo))
                return;
            battleGames[gameUser.GameId].UserAirplane[gameUser.UserSysNo] = gameUser.Airplane;

            bool gameStart = true;
            var airplanes = battleGames[gameUser.GameId].UserAirplane.Values.ToList();
            foreach (var airplane in airplanes)
            {
                if (airplane.Count != roundAirplaneCount)
                    gameStart = false;
            }
            if (!gameStart)
                return;

            //游戏开始
            battleGames[gameUser.GameId].Status = GameStatus.Running;

            //设置一个先手玩家
            int userNumber = new Random().Next(0, 1);
            battleGames[gameUser.GameId].CurrentUser = battleGames[gameUser.GameId].BattleUsers.Skip(userNumber).FirstOrDefault();


            //发送到用户信息通知开局
            if (ChangeHandle != null)
                ChangeHandle(battleGames[gameUser.GameId]);

        }

        /// <summary>
        /// 进行阶段刷新游戏选择轰炸
        /// </summary>
        /// <param name="choice"></param>
        public void RefreshBattleGameCrash(BattleGameUserChoice choice)
        {
            if (choice == null)
                return;
            if (!battleGames.ContainsKey(choice.GameId))
                return;
            if (battleGames[choice.GameId] == null)
                return;
            if (battleGames[choice.GameId].Status != GameStatus.Running)
                return;

            var targetUser = battleGames[choice.GameId].BattleUsers.Where(s => s.UserSysNo != choice.UserSysNo).First();


            //用户选择轰炸
            if (choice.IsClick)
            {
                if(battleGames[choice.GameId].CurrentUser.UserSysNo == choice.UserSysNo)
                {
                    //是该用户回合,判断是否命中

                    var targetValue = battleGames[choice.GameId].UserAirplane.Where(s => s.Key != choice.UserSysNo).First().Value;
                    if (targetValue.Any())
                    {
                        foreach (BattleAirplane item in targetValue)
                        {
                            foreach (var location in item.AirPlanePositions)
                            {
                                if(location.LocationX == choice.LocationX && location.LocationY == choice.LocationY)
                                {
                                    location.IsCrash = true;
                                    if(location.Position == AirplanePosition.Head)
                                    {
                                        item.IsCrash = true;
                                        targetUser.BattleSocre += 100;
                                    }
                                    else
                                    {
                                        targetUser.BattleSocre += 20;
                                    }
                                    break;
                                }
                            }
                        }


                        if(targetValue.Any(s=>s.IsCrash != false))
                        {
                            battleGames[choice.GameId].Status = GameStatus.RoundOver;
                        }
                    }
                    //变更选手
                    battleGames[choice.GameId].CurrentUser = targetUser;


                    //发送到用户信息通知游戏数据
                    if (ChangeHandle != null)
                        ChangeHandle(battleGames[choice.GameId]);
                }
            }
            else
            {
              
                //响应用户选择
                if (UserChoiceHandle != null)
                {
                    UserChoiceHandle(targetUser, choice);
                }
            }


 

        }

        private void UpdateBattleUser(BattleUser user)
        {
            var battlerUser = allBattleUsers.FirstOrDefault(s => s.UserSysNo == user.UserSysNo);
            if (battlerUser == null)
                return;
            battlerUser.IpAddress = user.IpAddress;
            battlerUser.NickName = user.NickName;
            battlerUser.LastTime = user.LastTime;
        }

        /// <summary>
        /// 根据状态获取用户信息
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        private List<BattleUser> GetUserByStatus(UserStatus status)
        {
            return allBattleUsers.Where(s => s.Status == status).ToList();

        }

        private void CreatBattleGame()
        {
            //获取准备中的用户
            List<BattleUser> users = GetUserByStatus(UserStatus.Wait).Take(2).ToList();
            if (users.Count < 2)
                return;

            foreach (var item in users)
            {
                item.Status = UserStatus.Battle;
                //更新用户对战状态
                UpdateBattleUser(item);
            }

            //创建对局
            DateTime creatTime = DateTime.Now;
            BattleGame game = new BattleGame();
            game.GameId = $"{creatTime.ToString("yyyyMMddHHmmss")}-{Guid.NewGuid().ToString()}";
            game.Round = 1;
            game.BattleUsers.AddRange(users);
            game.StartTime = creatTime;
            game.Status = GameStatus.Prepared;

            //初始化用户飞机
            for (int i = 0; i < users.Count(); i++)
            {
                game.UserAirplane.Add(users[i].UserSysNo, new List<BattleAirplane>());
            }



            battleGames.Add(game.GameId, game);
            //发送到用户信息通知开局
            if (ChangeHandle != null)
                ChangeHandle(game);

        }


    }
}

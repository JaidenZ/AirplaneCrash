namespace AirplaneCrash.Model
{
    using AirplaneCrash.Entity;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    public class GameModel
    {

        private static GameModel _instance;
        private static Thread gameThread;
        private const int roundAirplaneCount = 3;
        private const int roundCount = 3;

        //游戏对局集合
        private List<BattleGame> battleGames = new List<BattleGame>();

        public delegate void BattleGameChangeHandle(BattleGame game);
        public event BattleGameChangeHandle ChangeHandle;

        public delegate void BattleGameUserChoiceHandle(BattleUser sendBattleUser, BattleGameUserChoice userChoice);
        public event BattleGameUserChoiceHandle UserChoiceHandle;


        private GameModel()
        {

            if (gameThread == null)
            {
                gameThread = new Thread(() =>
                {
                    while (true)
                    {
                        CreatBattleGame();
                        Thread.Sleep(1000 * 2);
                    }
                });
                gameThread.Start();
            }

        }

        public static GameModel Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GameModel();

                return _instance;
            }
        }

        /// <summary>
        /// 自动创建游戏对局
        /// </summary>
        private void CreatBattleGame()
        {
            //获取准备中的用户
            List<BattleUser> users = BattleUserModel.Instance.GetUserByStatus(UserStatus.Wait).Take(2).ToList();
            if (users.Count < 2)
                return;

            foreach (var item in users)
            {
                item.Status = UserStatus.Battle;
                //更新用户对战状态
                BattleUserModel.Instance.ChangeBattleUserStatus(item);
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


            battleGames.Add(game);
            //发送到用户信息通知开局
            if (ChangeHandle != null)
                ChangeHandle(game);

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

            #region 校验飞机位置数据
            foreach (var item in gameUser.Airplane)
            {
                if (!item.AirPlanePositions.Any())
                {
                    return;
                }

                foreach (var position in item.AirPlanePositions)
                {
                    if (string.IsNullOrEmpty(position.LocationX))
                    {
                        return;
                    }
                    if (string.IsNullOrEmpty(position.LocationY))
                    {
                        return;
                    }
                }
            }
            #endregion


            var game = battleGames.FirstOrDefault(s => s.GameId == gameUser.GameId);
            if (game == null)
                return;
            //不在准备阶段与对局结束阶段
            if (game.Status != GameStatus.Prepared && game.Status != GameStatus.RoundOver)
                return;

            if (!game.UserAirplane.ContainsKey(gameUser.UserSysNo))
                return;

            //设置用户飞机设置
            game.UserAirplane[gameUser.UserSysNo] = gameUser.Airplane;

            bool gameStart = true;
            var airplanes = game.UserAirplane.Values.ToList();
            foreach (var airplane in airplanes)
            {
                if (airplane.Count != roundAirplaneCount)
                    gameStart = false;
            }
            if (!gameStart)
            {
                UpdateBattleGames(game);
                return;
            }


            //游戏开始
            game.Status = GameStatus.Running;

            //设置一个先手玩家
            int userNumber = new Random().Next(0, 1);
            game.CurrentUser = game.BattleUsers.Skip(userNumber).FirstOrDefault();


            UpdateBattleGames(game);
            //发送到用户信息通知开局
            if (ChangeHandle != null)
                ChangeHandle(game);

        }

        /// <summary>
        /// 进行阶段刷新游戏选择轰炸
        /// </summary>
        /// <param name="choice"></param>
        public void RefreshBattleGameCrash(BattleGameUserChoice choice)
        {
            if (choice == null)
                return;
            if (string.IsNullOrEmpty(choice.LocationX))
                return;
            if (string.IsNullOrEmpty(choice.LocationY))
                return;


            var game = battleGames.FirstOrDefault(s => s.GameId == choice.GameId);
            if (game == null)
                return;
            //不在对局阶段
            if (game.Status != GameStatus.Running)
                return;

            //目标用户
            var targetUser = game.BattleUsers.Where(s => s.UserSysNo != choice.UserSysNo).First();


            //用户选择轰炸
            if (choice.IsClick)
            {
                if (game.CurrentUser.UserSysNo == choice.UserSysNo)
                {
                    //是该用户回合,判断是否命中

                    var targetValue = game.UserAirplane.Where(s => s.Key != choice.UserSysNo).First().Value;
                    if (targetValue.Any())
                    {
                        foreach (BattleAirplane item in targetValue)
                        {
                            foreach (var location in item.AirPlanePositions)
                            {
                                if (location.LocationX == choice.LocationX && location.LocationY == choice.LocationY)
                                {
                                    location.IsCrash = true;
                                    if (location.Position == AirplanePosition.Head)
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


                        if (targetValue.Any(s => s.IsCrash != false))
                        {
                            //对局结束
                            game.Status = GameStatus.RoundOver;
                            game.Round++;

                            //游戏结束
                            if (game.Round >= roundCount)
                            {
                                game.Status = GameStatus.Over;
                            }
                        }
                    }
                    //变更选手
                    game.CurrentUser = targetUser;

                    if (game.Status == GameStatus.Over)
                    {
                        //重置游戏与用户状态
                        game.CurrentUser = null;
                        foreach (var item in game.BattleUsers)
                        {
                            item.Status = UserStatus.Normal;
                            //变更用户状态
                            BattleUserModel.Instance.ChangeBattleUserStatus(new BattleUser() { UserSysNo = item.UserSysNo, Status = item.Status });
                        }
                    }

                    UpdateBattleGames(game);
                    //发送到用户信息通知游戏数据
                    if (ChangeHandle != null)
                        ChangeHandle(game);
                }
            }
            else
            {
                UpdateBattleGames(game);
                //响应用户选择
                if (UserChoiceHandle != null)
                {
                    UserChoiceHandle(targetUser, choice);
                }
            }
        }


        private void UpdateBattleGames(BattleGame game)
        {

            for (int i = 0; i < battleGames.Count; i++)
            {
                if (battleGames[i].GameId == game.GameId)
                {
                    battleGames[i] = game;
                    break;
                }
            }
        }

    }
}

namespace AirplaneCrash.Server
{

    using System;
    using System.Collections.Generic;
    using System.Text;
    using AirplaneCrash.Server.Battle;
    using Fleck;
    using Entity;
    using Newtonsoft.Json;
    using System.Linq;
    using AirplaneCrash.Core.Hub;
    using AirplaneCrash.Core.Utilits;

    internal class AirplaneServer
    {

        private static AirplaneServer _instance;
        private static WebSocketServer server;

        private Dictionary<string,IWebSocketConnection> webSockectConnections = new Dictionary<string, IWebSocketConnection>();
        private Dictionary<int, string> socketUsers = new Dictionary<int, string>();

        internal AirplaneServer()
        {
            BattleContainer.GetInstance().ChangeHandle += AirplaneServer_ChangeHandle;
            BattleContainer.GetInstance().UserChoiceHandle += AirplaneServer_UserChoiceHandle;


        }

        private void AirplaneServer_UserChoiceHandle(BattleUser sendBattleUser, BattleGameUserChoice userChoice)
        {
            if (sendBattleUser == null)
                return;
            if (userChoice == null)
                return;

            //通知目标用户 选择数据
            SendMessage(sendBattleUser.IpAddress, MessageType.TargetChoice, userChoice);

        }

        private void AirplaneServer_ChangeHandle(BattleGame game)
        {
            if (game == null)
                return;

            //通知两个用户 发送游戏数据
            var ipaddressList = game.BattleUsers.Select(s=>s.IpAddress).ToList();
            SendMessage(ipaddressList, MessageType.GameData, game);

        }

        internal static AirplaneServer GetInstance()
        {
            if (_instance == null)
                _instance = new AirplaneServer();

            return _instance;
        }

        internal void Start()
        {

            server = new WebSocketServer("ws://172.16.160.119:9096");
            server.Start(socket =>
            {
                socket.OnOpen = () => OnConnect(socket);
                socket.OnClose = () => OnClose(socket);
                socket.OnMessage = message => OnMessage(socket,message);
                
            });

            Console.WriteLine("初始化服务器成功.");
        }


        private void OnConnect(IWebSocketConnection sockect)
        {
            string clientIp = sockect.ConnectionInfo.ClientIpAddress;

            if (webSockectConnections.ContainsKey(clientIp))
            {
                webSockectConnections[clientIp].Close();
                webSockectConnections.Remove(clientIp);
            }

            webSockectConnections.Add(clientIp, sockect);
            Console.WriteLine($"用户:{clientIp}连接到服务器!");

        }

        private void OnClose(IWebSocketConnection sockect)
        {
            string clientIp = sockect.ConnectionInfo.ClientIpAddress;

            if (webSockectConnections.ContainsKey(clientIp))
            {
                webSockectConnections[clientIp].Close();
                webSockectConnections.Remove(clientIp);
            }
            Console.WriteLine($"用户:{clientIp}断开服务器连接!");
        }

        private void OnMessage(IWebSocketConnection sockect,string message)
        {
            if (string.IsNullOrEmpty(message))
                return;
            try
            {
                MessageEntity entity = (MessageEntity)JsonConvert.DeserializeObject(message);

                IHub<MessageEntity, int> hub = HubContainer.Get<MessageEntity, int>(1000, (int)entity.Code, 0, entity.Code.GetDescription());
                int userSysNo = hub.Handle(entity);
                if(socketUsers.ContainsKey(userSysNo))
                {
                    socketUsers[userSysNo] = sockect.ConnectionInfo.ClientIpAddress.ToString();
                }
                else
                {
                    socketUsers.Add(userSysNo, sockect.ConnectionInfo.ClientIpAddress.ToString());
                }


                switch (entity.Code)
                {
                    case MessageType.HeartBeat:
                        BattleUser heart = (BattleUser)JsonConvert.DeserializeObject(entity.Message);
                        heart.LastTime = DateTime.Now;
                        BattleContainer.GetInstance().AddBattleUser(heart);
                        //回复
                        SendMessage(sockect.ConnectionInfo.ClientIpAddress, MessageType.HeartBeat, heart);
                        break;
                    case MessageType.Login://登录 创建新用户
                        BattleUser login = (BattleUser)JsonConvert.DeserializeObject(entity.Message);
                        BattleUser user = new BattleUser();
                        user.UserSysNo = login.UserSysNo == 0 ? int.Parse(Guid.NewGuid().ToString()) : login.UserSysNo;
                        user.IpAddress = sockect.ConnectionInfo.ClientIpAddress;
                        user.NickName = login.NickName;
                        user.Status = UserStatus.Normal;
                        user.BattleSocre = 0;
                        user.LastTime = DateTime.Now;
                        BattleContainer.GetInstance().AddBattleUser(user);
                        //回复
                        SendMessage(sockect.ConnectionInfo.ClientIpAddress, MessageType.Login, user);
                        break;
                    case MessageType.Prepare://用户准备 
                        BattleUser prepare = (BattleUser)JsonConvert.DeserializeObject(entity.Message);
                        BattleContainer.GetInstance().ChangeBattleUserStatus(prepare);
                        break;
                    case MessageType.AirPlaneData://飞机数据
                        BattleGameUser gameuser = (BattleGameUser)JsonConvert.DeserializeObject(entity.Message);
                        BattleContainer.GetInstance().RefreshBattleGameUser(gameuser);
                        break;
                    case MessageType.TargetChoice://选择数据
                        BattleGameUserChoice choice = (BattleGameUserChoice)JsonConvert.DeserializeObject(entity.Message);
                        BattleContainer.GetInstance().RefreshBattleGameCrash(choice);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"接受数据异常:{ex.Message}");
            }
        }


        private void SendMessage<T>(List<string> ipAddresss, MessageType type, T data) where T : class
        {

            for (int i = 0; i < ipAddresss.Count; i++)
            {
                SendMessage(ipAddresss[i], type, data);
            }

        }

        private void SendMessage<T>(string ipAddresss,MessageType type,T data) where T : class
        {
            MessageEntity entity = new MessageEntity();
            entity.Code = type;
            entity.Message = JsonConvert.SerializeObject(data);

            SendMessage(ipAddresss,entity);

        }

        private void SendMessage(string ipAddresss, MessageType type)
        {
            MessageEntity entity = new MessageEntity();
            entity.Code = type;

            SendMessage(ipAddresss, entity);

        }

        private void SendMessage(string ipAddress,MessageEntity message)
        {
            if (string.IsNullOrEmpty(ipAddress) || message == null)
                return;
            if (!webSockectConnections.ContainsKey(ipAddress))
                return;

            IWebSocketConnection sockect = webSockectConnections[ipAddress];
            SendMessage(sockect, JsonConvert.SerializeObject(message));

        }

        private void SendMessage(IWebSocketConnection sockect,string message)
        {
            if (!sockect.IsAvailable)
                return;
            try
            {
                sockect.Send(message);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"发送数据异常:{ex.Message}");
            }
        }

    }
}

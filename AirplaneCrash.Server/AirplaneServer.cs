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
    using AirplaneCrash.Model;
    using System.Net;

    public class AirplaneServer
    {

        private static AirplaneServer _instance;
        private static WebSocketServer server;

        private Dictionary<string,IWebSocketConnection> webSockectConnections = new Dictionary<string, IWebSocketConnection>();

        public AirplaneServer()
        {
            GameModel.Instance.ChangeHandle += AirplaneServer_ChangeHandle;
            GameModel.Instance.UserChoiceHandle += AirplaneServer_UserChoiceHandle;
            BattleUserModel.Instance.BattleUserInfoLogin += Instance_BattleUserInfoLogin; ;
            BattleUserModel.Instance.BattleUserHearbeat += Instance_BattleUserHearbeat;
        }

        private void Instance_BattleUserHearbeat(BattleUser user)
        {
            if (user == null)
                return;

            SendMessage(user.IpAddress, MessageType.HeartBeat, user);
        }

        private void Instance_BattleUserInfoLogin(BattleUser user)
        {
            if (user == null)
                return;

            SendMessage(user.IpAddress, MessageType.Login, user);
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
            
            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            var ipv4address = hostEntry.AddressList?.First(s => s.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

            server = new WebSocketServer($"ws://{ipv4address}:9096");
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
                RequestMessage entity = JsonConvert.DeserializeObject<RequestMessage>(message);
                entity.RequestIpAddress = sockect.ConnectionInfo.ClientIpAddress;
                IHub<RequestMessage, int> hub = HubContainer.Get<RequestMessage, int>(1000, (int)entity.Code, 0, entity.Code.GetDescription());
                int userSysNo = hub.Handle(entity);
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

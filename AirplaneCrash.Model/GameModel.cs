namespace AirplaneCrash.Model
{
    using AirplaneCrash.Entity;
    using System.Collections.Generic;
    using System.Linq;

    public class GameModel
    {

        private static GameModel _instance;
        private const int roundAirplaneCount = 3;
        private const int roundCount = 3;

        //游戏对局集合
        private List<BattleGame> battleGames = new List<BattleGame>();

        private GameModel()
        {
            if(_instance == null)
            {
                _instance = new GameModel();
            }
            
        }

        public static GameModel Instance
        {
            get { return _instance; }
        }


    }
}

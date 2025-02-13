using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetrisClone
{
    internal class Player
    {
        public string PlayerName {  get; set; }
        public int Score { get; set; }

        public Player(string playerName, int score)
        {
            PlayerName = playerName; 
            Score = score;
        }
    }

    internal class PlayerScoreList
    {
        public List<Player> PlayerScore { get; set; }
        public PlayerScoreList()
        {
            PlayerScore = new List<Player>();
        }
        public void CreatePSList(string playerName, int score)
        {
            PlayerScore.Add(new Player(playerName, score));
        }
    }
}

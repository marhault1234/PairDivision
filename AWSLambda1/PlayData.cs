using System;
using System.Collections.Generic;
using System.Text;

namespace AWSLambda1
{
    class PlayData
    {
    }

    public class GamePairCombi
    {
        public int player1Id {
            get { return _player1Id; }
            set {
                _player1Id = value;
                Alignment();
            }
        }
        int _player1Id;
        public int player2Id {
            get { return _player2Id; }
            set
            {
                _player2Id = value;
                Alignment();
            }
        }
        int _player2Id;
        private void Alignment()
        {
            if (_player1Id == 0 || _player2Id == 0) return;
            if(_player1Id > _player2Id)
            {
                int buf = _player2Id;
                _player2Id = _player1Id;
                _player1Id = buf;
            }
        }
        public string pairIdValuer { get { return player1Id.ToString("0000") + player2Id.ToString("0000"); } }
    }
    public class GameCombi
    {
        public GamePairCombi comb1 { get; set; }
        public GamePairCombi comb2 { get; set; }
        public string combStrValue
        {
            get
            {
                return comb1.player1Id.ToString("0000") + comb1.player2Id.ToString("0000") + comb2.player1Id.ToString("0000") + comb2.player2Id.ToString("0000");
            }
        }
        public string testCombStrValue
        {
            get
            {
                return comb1.player1Id.ToString() + "|" + comb1.player2Id.ToString() + "  " + comb2.player1Id.ToString() + "|" + comb2.player2Id.ToString();
            }
        }
        public List<string> getPairList { get { return new List<string>() { comb1.pairIdValuer, comb2.pairIdValuer }; } }

    }
}

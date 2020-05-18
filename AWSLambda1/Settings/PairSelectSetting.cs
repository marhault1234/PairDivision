using Amazon.Lambda.Core;
using AWSLambda1.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static AWSLambda1.Entity.GameLogEntity;

namespace AWSLambda1.Settings
{
    public abstract class AbstractPairSelectSetting
    {
        /// <summary>
        /// ゲームプレイヤーと過去の試合ログから次の試合の組み合わせを決定する
        /// </summary>
        /// <param name="gamePlayers"></param>
        /// <param name="gameLogEntity"></param>
        /// <returns></returns>
        public List<GameCombi> getPairList(ILambdaContext context, List<Player> gamePlayers, GameLogEntity gameLogEntity)
        {
            // 次の試合のペア
            List<GameCombi> gamePairs = new List<GameCombi>();

            // 組み合わせ試行回数：1000、1000回試行してもだめだったらランダムに組み合わせてログをリセットする。
            int i = 0;
            while (i++ <= 1000)
            {
                gamePairs = logic(gamePlayers, gameLogEntity.gameLogList);

                bool loopFlg = false;
                // 決定した全ての組み合わせに過去と同一のものが無ければ決定
                foreach (GameCombi gameComb in gamePairs)
                {
                    loopFlg = loopFlg || gameLogEntity.gameLogList.Where(obj =>obj.combStrValue.Equals(gameComb.combStrValue)).Any();
                }
                if (!loopFlg) break;
            }
            // 1000回試行してペアが決まらなかった場合ログをリセットして最初から
            if (i >= 1000)
            {
                gameLogEntity.logClear();
                gamePairs = logic(gamePlayers, gameLogEntity.gameLogList);
            }
            // 組み合わせ結果を保存
            foreach (GameCombi comb in gamePairs) gameLogEntity.addLog(comb);
            gameLogEntity.Save(context);

            return gamePairs;
        }
        protected abstract List<GameCombi> logic(List<Player> gamePlayers, List<GameCombi> logList);
    }

    /// <summary>
    /// ランダムパターン、組み合わせに条件を付けず適当にランダムで組み合わせる
    /// </summary>
    public class RandomPairSelect : AbstractPairSelectSetting
    {
        protected override List<GameCombi> logic(List<Player> gamePlayers, List<GameCombi> logList)
        {
            int member = gamePlayers.Count;
            bool[] rndFlgs = new bool[member];
            int[] ids = new int[member];
            Random rnd = new System.Random();

            for (int i = 0; i < member; i++)
            {
                int r = rnd.Next(0, member);
                while (rndFlgs[r]) r = rnd.Next(0, member);
                rndFlgs[r] = true;
                ids[r] = gamePlayers[i].Id;
            }

            // ペアの先頭はIDが小さい方にする
            for (int i = 0; i < member; i += 2)
            {
                if (ids[i] > ids[i + 1])
                {
                    int buf = ids[i];
                    ids[i] = ids[i + 1];
                    ids[i + 1] = buf;
                }
            }

            List<GameCombi> rtnList = new List<GameCombi>();
            for (int i = 0; i < member; i += 4)
            {
                rtnList.Add(new GameCombi()
                {
                    comb1 = new GamePairCombi()
                    {
                        player1Id = ids[i],
                        player2Id = ids[i + 1],
                    },
                    comb2 = new GamePairCombi()
                    {
                        player1Id = ids[i + 2],
                        player2Id = ids[i + 3],
                    },
                });
            }
            return rtnList;
        }
    }
}

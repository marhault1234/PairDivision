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
        public List<GameCombi> getPairList( List<Player> gamePlayers, ref GameLogEntity gameLogEntity)
        {
            // 次の試合のペア
            List<GameCombi> gamePairs = new List<GameCombi>();

            gamePairs = logic(gamePlayers, gameLogEntity);

            // 組み合わせ結果を保存
            foreach (GameCombi comb in gamePairs) gameLogEntity.addLog(comb);

            return gamePairs;
        }

        /// <summary>
        /// ゲーム参加者と過去ログから次の試合のペアを決定する。
        /// 次の試合のペアが決まらない場合、ログをリセットし、最初から振り分けを行う。
        /// </summary>
        /// <param name="gamePlayers"></param>
        /// <param name="gameLogEntity"></param>
        /// <returns></returns>
        protected abstract List<GameCombi> logic(List<Player> gamePlayers, GameLogEntity gameLogEntity);
    }

    /// <summary>
    /// ランダムパターン、組み合わせに条件を付けず適当にランダムで組み合わせる
    /// </summary>
    public class RandomPairSelect : AbstractPairSelectSetting
    {
        protected override List<GameCombi> logic(List<Player> gamePlayers, GameLogEntity gameLogEntity)
        {
            List<GameCombi> createCombi(List<Player> players, List<GameCombi> logList)
            {
                int member = players.Count;
                bool[] rndFlgs = new bool[member];
                int[] ids = new int[member];
                Random rnd = new System.Random();

                for (int i = 0; i < member; i++)
                {
                    int r = rnd.Next(0, member);
                    while (rndFlgs[r]) r = rnd.Next(0, member);
                    rndFlgs[r] = true;
                    ids[r] = players[i].Id;
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

            List<GameCombi> results = null;

            // 直近1,2試合分のペアリスト
            List<GameCombi> pairValueList = new List<GameCombi>();

            // 直近の試合ログがある場合それを保持
            int coatCount = gamePlayers.Count / 4;
            if (gameLogEntity.gameLogList.Count >= coatCount * 2)
            {
                pairValueList.AddRange(gameLogEntity.gameLogList.GetRange(gameLogEntity.gameLogList.Count - coatCount * 2, coatCount * 2));
            }
            else if (gameLogEntity.gameLogList.Count >= coatCount)
            {
                pairValueList.AddRange(gameLogEntity.gameLogList.GetRange(gameLogEntity.gameLogList.Count - coatCount, coatCount));
            }
            List<string> pairIdList = new List<string>();
            pairValueList.ForEach(obj => pairIdList.AddRange(obj.getPairList));

            // 組み合わせ試行回数：1000、1000回試行してもだめだったらランダムに組み合わせてログをリセットする。
            int count = 0;
            while (count++ <= 1000)
            {
                results = createCombi(gamePlayers, gameLogEntity.gameLogList);

                bool loopFlg = false;
                // 決定した全ての組み合わせに過去と同一のものが無く、直近の試合と同じペアでなければ決定
                foreach (GameCombi gameComb in results)
                {
                    loopFlg = loopFlg || 
                        gameLogEntity.gameLogList.Where(obj => obj.combStrValue.Equals(gameComb.combStrValue)).Any();
                }
                if (!loopFlg) break;
            }
            // 1000回試行してペアが決まらなかった場合ログをリセットして最初から
            if (count >= 1000)
            {
                gameLogEntity.logClear();
                results = createCombi(gamePlayers, gameLogEntity.gameLogList);
            }
            return results;
        }
    }
}

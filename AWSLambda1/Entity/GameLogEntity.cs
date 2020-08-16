using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using AWSLambda1.Entity.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AWSLambda1.Entity
{
    /// <summary>
    /// チームメンバー情報
    /// TeamMemberHelperを介してアクセスすること
    /// </summary>
    [DynamoDBTable("GameLog")]
    public class GameLogEntity : EntityBase
    {
        /// <summary>
        /// 試合のペア履歴
        /// </summary>
        [DynamoDBProperty("Log")]
        private List<string> GameLog { get; set; }

        /// <summary>
        /// 最終更新時刻
        /// </summary>
        [DynamoDBProperty("LastCallTime")]
        public DateTime lastCallTime { get; set; }

        [DynamoDBIgnore]
        private static readonly string PAIR_SPLIT = ",";
        [DynamoDBIgnore]
        private static readonly string TEAM_SPLIT = "-";

        /// <summary>
        /// 過去の試合リスト、
        /// 0-1で１ペア2-3で１ペア
        /// </summary>
        [DynamoDBIgnore]
        public List<GameCombi> gameLogList
        {
            get
            {
                // 最終コールから１日以上経過してる場合試合結果をリセット
                if (lastCallTime.AddDays(1) < DateTime.Now)
                {
                    logClear();
                }
                // DBで保存している試合ログをユーザーID毎のリストにして返却
                List<GameCombi> retList = new List<GameCombi>();
                foreach (string log in GameLog)
                    retList.Add(new GameCombi()
                    {
                        comb1 = new GamePairCombi()
                        {
                            player1Id = int.Parse(log.Split(TEAM_SPLIT)[0].Split(PAIR_SPLIT)[0]),
                            player2Id = int.Parse(log.Split(TEAM_SPLIT)[0].Split(PAIR_SPLIT)[1]),
                        },
                        comb2 = new GamePairCombi()
                        {
                            player1Id = int.Parse(log.Split(TEAM_SPLIT)[1].Split(PAIR_SPLIT)[0]),
                            player2Id = int.Parse(log.Split(TEAM_SPLIT)[1].Split(PAIR_SPLIT)[1]),
                        }
                    });
                return retList;
            }
        }

        public void logClear()
        {
            this.GameLog = new List<string>();
        }

        public void addLog(GameCombi combi)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(combi.comb1.player1Id).Append(PAIR_SPLIT).Append(combi.comb1.player2Id).Append(TEAM_SPLIT).Append(combi.comb2.player1Id).Append(PAIR_SPLIT).Append(combi.comb2.player2Id);
            GameLog.Add(sb.ToString());
            lastCallTime = DateTime.Now;
        }

        public override void Save(ILambdaContext context)
        {
            context.Logger.LogLine(JsonConvert.SerializeObject(this));

            // 最初にDynamoDBContextを生成する
            using (var dbContext = new DynamoDBContext(Static.Client))
            {
                // SaveAsyncメソッドでentityがTodosテーブルへ追加
                var insertTask = dbContext.SaveAsync(this);
                insertTask.Wait();
            }
        }
    }

}

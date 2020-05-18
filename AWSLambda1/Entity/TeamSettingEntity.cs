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
    [DynamoDBTable("TeamMember")]
    public class TeamSettingEntity : EntityBase
    {

        /// <summary>
        /// コート数
        /// </summary>
        [DynamoDBProperty("CoatNumber")]
        public int CoatNumber { get; set; }
        /// <summary>
        /// 名称|性別|ランクを"|"区切りで接続
        /// </summary>
        [DynamoDBProperty("PlayerState")]
        private List<string> PlayerState { get; set; }

        [DynamoDBIgnore]
        private static readonly string SPLIT_STR = "┃";
        [DynamoDBIgnore]
        private static readonly string MALE = "M";
        [DynamoDBIgnore]
        private static readonly string FEMALE = "F";

        /// <summary>
        /// ID
        /// </summary>
        [DynamoDBIgnore]
        public List<Player> players {
            get
            {
                List<Player> playerList = new List<Player>();
                for (int i = 0; i < this.PlayerState.Count; i++)
                {
                    var temp = this.PlayerState[i].Split(SPLIT_STR);
                    if (string.IsNullOrEmpty(temp[1]))continue;

                    playerList.Add(new Player()
                    {
                        Id = int.Parse(temp[0]),
                        Name = temp[1],
                        Sex = temp[2].Equals(MALE) ? true : false,
                        Rank = int.Parse(temp[3]),
                        GameCount = int.Parse(temp[4]),
                    });
                }
                return playerList;
            }
            set
            {
                List<string> PlayerState = new List<string>();
                if (value != null)
                {
                    foreach (Player player in value)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(player.Id).Append(SPLIT_STR)
                            .Append(player.Name).Append(SPLIT_STR)
                            .Append(player.Sex ? MALE : FEMALE).Append(SPLIT_STR)
                            .Append(player.Rank).Append(SPLIT_STR)
                            .Append(player.GameCount);
                        PlayerState.Add(sb.ToString());
                    }
                }
                this.PlayerState = PlayerState;
            }
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
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool Sex { get; set; }
        public int Rank { get; set; }
        public int GameCount { get; set; }
    }

}

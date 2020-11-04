using AWSLambda1.DBAccess;
using AWSLambda1.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AWSLambda1.PlayerSelect
{

    /// <summary>
    /// デフォルトパターン、出来るだけ男女別かミックスで振り分ける。
    /// 男女比が異なる場合、真ん中のコートが適当。
    /// </summary>
    public class DefaultPairSelect : CreatePair, IPairSelect
    {
        public List<Results> getPairList(List<Users> users, List<Results> results, int practice_id)
        {
            List<Results> result = new List<Results>();

            Random rnd = new Random(DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second);

            int randomInt = rnd.Next(0, 2);
            // 1/2で男女別かミックスか
            List<Users> sortList = new List<Users>();
            
            if (randomInt == 0)
            {
                // 男女別
                sortList = users.OrderBy(user => user.gender).ThenBy(user => user.ContinuousCount).ThenBy(user => user.randomInt).ToList();
            }
            else
            {
                // ミックス
                int userCount = users.Count;
                int pairCount = userCount / 2;
                // 男女別にソートして先頭と末尾から一人ずつ先頭になるよう並び替える
                var temp = users.OrderBy(user => user.gender).ThenBy(user => user.ContinuousCount).ThenBy(user => user.randomInt).ToArray();
                for(int i = 0; i < pairCount; i++)
                {
                    sortList.Add(temp[i]);
                    sortList.Add(temp[userCount - 1 - i]);
                }
            }

            return this.makeMixResults(sortList, practice_id);
        }
    }
}

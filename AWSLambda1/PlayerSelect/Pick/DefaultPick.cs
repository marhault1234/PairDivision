using AWSLambda1.DBAccess;
using AWSLambda1.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AWSLambda1.PlayerSelect
{
    public class DefaultPick : IPlayerSelect
    {
        /// <summary>
        /// 順位１：試合参加割合が低い順
        /// 順位２：ランダム
        /// </summary>
        /// <param name="userData"></param>
        /// <param name="matches"></param>
        /// <returns></returns>
        public List<Users> playerSelect(List<Users> users, List<Results> matches, int coatNumber)
        {
            // 参加者が4で割り切れる数の場合メンバーが固定されるため、１人ずつ交代するように対応
            List<Users> result = new List<Users>();
            if (users.Count > coatNumber * 4 && users.Count % 4 == 0)
            {
                result = users.OrderBy(user => user.playPercentage).ThenBy(user => user.ContinuousCount).ThenBy(user => user.randomInt).Take(coatNumber * 4 + 1).ToList();
                result.RemoveAt(3);
            }
            else
            {
                result = users.OrderBy(user => user.playPercentage).ThenBy(user => user.ContinuousCount).ThenBy(user => user.randomInt).Take(coatNumber * 4).ToList();
            }

            return result;
        }
    }
}

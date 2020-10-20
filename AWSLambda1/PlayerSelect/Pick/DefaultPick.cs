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
            return users.OrderBy(user => user.playPercentage).ThenBy(user => user.randomInt).Take(coatNumber * 4).ToList();
        }
    }
}

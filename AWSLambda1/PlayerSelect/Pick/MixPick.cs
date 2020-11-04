using AWSLambda1.DBAccess;
using AWSLambda1.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AWSLambda1.PlayerSelect
{
    public class MixPick : IPlayerSelect
    {
        /// <summary>
        /// 男女同数になるように
        /// 順位１：試合参加割合が低い順
        /// 順位２：ランダム
        /// でピックする。
        /// </summary>
        /// <param name="userData"></param>
        /// <param name="matches"></param>
        /// <returns></returns>
        public List<Users> playerSelect(List<Users> users, List<Results> matches, int coatNumber)
        {
            List<Users> result = new List<Users>();

            // 各性別必要人数
            int needNumber = coatNumber * 2;
            // ミックス構成人数
            int mensCount = users.Count(u => u.gender == Static.MAN);
            int womensCount = users.Count(u => u.gender == Static.WOMAN);

            // 男女同数のピックが可能
            if (mensCount >= needNumber && womensCount >= needNumber)
            {
                var mens = users.Where(user => user.gender == Static.MAN).OrderBy(user => user.playPercentage).ThenBy(user => user.randomInt).Take(coatNumber * 2);
                var womens = users.Where(user => user.gender == Static.WOMAN).OrderBy(user => user.playPercentage).ThenBy(user => user.randomInt).Take(coatNumber * 2);
                result.AddRange(mens);
                result.AddRange(womens);
            }
            else
            {
                // 男女同数にピック出来ない場合は可能な限りミックスを作るようにピックする
                List<Users> temp = new List<Users>(users);
                while(temp.Count(u => u.gender == Static.MAN) >= 2 && temp.Count(u => u.gender == Static.WOMAN) >= 2)
                {
                    var mens = users.Where(user => user.gender == Static.MAN).OrderBy(user => user.playPercentage).ThenBy(user => user.randomInt).Take(2).ToList();
                    var womens = users.Where(user => user.gender == Static.WOMAN).OrderBy(user => user.playPercentage).ThenBy(user => user.randomInt).Take(2).ToList();
                    mens.ForEach(men => temp.Remove(men));
                    womens.ForEach(women => temp.Remove(women));
                    result.AddRange(mens);
                    result.AddRange(womens);
                }
                // 足りない人数は試合割合が少ない順にピック
                result.AddRange(temp.OrderBy(user => user.playPercentage).ThenBy(user => user.randomInt).Take(coatNumber * 4 - result.Count));
            }
            return result;
        }
    }
}

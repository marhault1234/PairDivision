using AWSLambda1.DBAccess;
using AWSLambda1.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AWSLambda1.PlayerSelect
{

    /// <summary>
    /// ランダムパターン、組み合わせに条件を付けず適当にランダムで組み合わせる
    /// </summary>
    public class RandomPairSelect : CreatePair, IPairSelect
    {
        public List<Results> getPairList(List<Users> users, List<Results> results, int practice_id)
        {
            var sortUsers = users.OrderBy(user => user.randomInt).ToList();
            return makeResults(sortUsers, practice_id);
        }
    }

}

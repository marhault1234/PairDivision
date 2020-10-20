using AWSLambda1.DBAccess;
using System.Collections.Generic;

namespace AWSLambda1.Settings
{
    public interface IPlayerSelect
    {
        /// <summary>
        /// 練習参加者・試合の組み合わせログ、コート数を渡し、次の試合の参加者を返却する。
        /// </summary>
        /// <param name="users"></param>
        /// <param name="matches"></param>
        /// <returns></returns>
        List<Users> playerSelect(List<Users> users, List<Results> matches, int coatNumber);
    }

}

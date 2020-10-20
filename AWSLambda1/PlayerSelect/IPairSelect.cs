using Amazon.Lambda.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using AWSLambda1.DBAccess;

namespace AWSLambda1.Settings
{
    public interface IPairSelect
    {
        /// <summary>
        /// ゲームプレイヤーと過去の試合ログから次の試合の組み合わせを決定する
        /// </summary>
        /// <param name="gamePlayers"></param>
        /// <param name="gameLogEntity"></param>
        /// <returns></returns>
        List<Results> getPairList(List<Users> users, List<Results> results, int practice_id);
    }

    public abstract class CreatePair
    {
        /// <summary>
        /// 受け取ったUsersのリスト順で組み合わせを作成する。
        /// [0][1]対[2][3]の形式
        /// </summary>
        /// <param name="users"></param>
        /// <param name="practice_id"></param>
        /// <returns></returns>
        public List<Results> makeResults(List<Users> users, int practice_id)
        {
            List<Results> result = new List<Results>();
            int userCount = users.Count;
            for(int i=0;i< userCount / 4;i++)
            {
                result.Add(new Results()
                {
                    practice_id = practice_id,
                    count = i + 1,
                    pair = 1,
                    user1 = users[4 * i + 0].id,
                    user2 = users[4 * i + 1].id,
                });
                result.Add(new Results()
                {
                    practice_id = practice_id,
                    count = i + 1,
                    pair = 2,
                    user1 = users[4 * i + 2].id,
                    user2 = users[4 * i + 3].id,
                });
            }
            return result;
        }
        /// <summary>
        /// 受け取ったUsersのリスト順で組み合わせを作成する。
        /// リスト中に男女が二人ずついる場合、ミックスとなるように補正する。
        /// </summary>
        /// <param name="users"></param>
        /// <param name="practice_id"></param>
        /// <returns></returns>
        public List<Results> makeMixResults(List<Users> users, int practice_id)
        {
            List<Users> temp = new List<Users>();
            int userCount = users.Count;
            for (int i = 0; i < userCount / 4; i++)
            {
                // 男女二人の場合
                if(users.Skip(i*4).Take(4).Count(user=>user.gender == Static.MAN) == 2)
                {
                    var mens = users.Skip(i * 4).Take(4).Where(user => user.gender == Static.MAN).ToArray();
                    var womens = users.Skip(i * 4).Take(4).Where(user => user.gender == Static.WOMAN).ToArray();
                    temp.Add(mens[0]);
                    temp.Add(womens[0]);
                    temp.Add(mens[1]);
                    temp.Add(womens[1]);
                }
                else
                {
                    temp.AddRange(users.Skip(i * 4).Take(4));
                }
            }
            return makeResults(temp,practice_id);
        }
    }
}

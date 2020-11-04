using System;
using System.Collections.Generic;
using System.Text;

namespace AWSLambda1.DBAccess
{
    public class Users : Play_counts
    {
        public string name { get; set; }
        public string name_kana { get; set; }
        public int gender { get; set; }

        /// <summary>
        /// 試合参加数 / カウント
        /// </summary>
        [NoDbColumn]
        public decimal playPercentage { get; set; }

        /// <summary>
        /// ランダム値
        /// </summary>
        [NoDbColumn]
        public int randomInt { get; set; }

        /// <summary>
        /// 連続試合数
        /// </summary>
        [NoDbColumn]
        public int ContinuousCount { get; set; }
    }
    public class Play_counts : TableData
    {
        public int practice_id { get; set; }
        public int user_id { get; set; }
        public int? count { get; set; }
    }
}

using Alexa.NET.Response;
using Amazon.Lambda.Core;
using AWSLambda1.DBAccess;
using AWSLambda1.Settings;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using static AWSLambda1.Logic;

namespace AWSLambda1
{

    public class Logic
    {
        public enum SelectionPatternEnum
        {
            /// <summary>
            /// 前試合に参加していない人を優先に選択し、
            /// ランダムな組み合わせでコールする
            /// </summary>
            RandomCall,
            /// <summary>
            /// 前試合に参加していない人を優先に選択し、
            /// 出来るだけミックス・男女別にコールする。
            /// </summary>
            DefaultCall,
        }

        #region 次の試合をコール
        public (string, SimpleCard) callNextGame(ILambdaContext context, string uid)
        {
            Random rnd = new Random(DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second);

            // ***** データ読み込み *****
            this.dataLoad(out Practices practice, out List<Results> resultList, out List<Users> userList);

            // 全ゲームに参加したユーザー情報（回数カウント用）
            List<int> gamePlayerLog = new List<int>();
            resultList.ForEach(r => {
                gamePlayerLog.Add(r.user1);
                gamePlayerLog.Add(r.user2); 
            });

            // 各ユーザーにランダム値と試合率設定
            userList.ForEach(user => { 
                user.randomInt = rnd.Next(9999999); 
                user.playPercentage = gamePlayerLog.Count(obj => obj == user.id) / (user.count ?? 1); 
            });

            // コート数取得。最大３コート
            int coatCount = userList.Count / 4;
            coatCount = coatCount > 3 ? 3 : coatCount;

            // ペア決めのロジックを設定【ToDo 変更を容易に出来るように】
            SelectionPatternEnum selectionPattern = SelectionPatternEnum.DefaultCall;

            // 次ゲームの参加者決め
            List<Users> gamePlayers = selectionPattern.getPicker().playerSelect(userList, resultList, coatCount);

            // ペア決め
            List<Results> addMatchesAndResults = selectionPattern.getSelecter().getPairList(gamePlayers, resultList, coatCount);

            // DB更新
            this.saveData(addMatchesAndResults, gamePlayers, practice);

            return createAlexaResponse(addMatchesAndResults, gamePlayers);
        }
        #endregion

        #region DBデータ取得処理
        public void dataLoad(out Practices practice, out List<Results> resultList, out List<Users> userList)
        {
            practice = new Practices();                                     // 練習データ
            resultList = new List<Results>();                               // 試合履歴
            userList = new List<Users>();                                   // 参加者

            using (MySqlConnection connection = new MySqlConnection(Static.BUILDER.ConnectionString))
            {
                connection.Open();

                // 練習データ取得
                StringBuilder practicesSqlBuilder = new StringBuilder();
                practicesSqlBuilder.Append("SELECT * ");
                practicesSqlBuilder.Append("FROM rmaster.practices ");
                practicesSqlBuilder.Append("WHERE start_at <= '{0}' and end_at >= '{0}' ");
                string practicesSql = string.Format(practicesSqlBuilder.ToString(), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss "));

                DataLoad<Practices> practicesAccess = new DataLoad<Practices>(connection, practicesSql);
                var practicesAccessLoad = practicesAccess.load();
                if (practicesAccessLoad != null) practice = practicesAccessLoad[0];
                else return;


                // 練習・過去試合データのロードSQL
                StringBuilder resultsSqlBuilder = new StringBuilder();
                resultsSqlBuilder.Append("SELECT * ");
                resultsSqlBuilder.Append("FROM rmaster.matches A ");
                resultsSqlBuilder.Append("LEFT OUTER JOIN rmaster.results B ON A.id = B.match_id ");
                resultsSqlBuilder.Append("WHERE A.practice_id = {0} ");
                string resultsSql = string.Format(resultsSqlBuilder.ToString(), practice.id);

                DataLoad<Results> resultAccess = new DataLoad<Results>(connection, resultsSql);
                resultList = resultAccess.load();


                // 練習参加者ロードSQL
                StringBuilder usersSqlBuilder = new StringBuilder();
                usersSqlBuilder.Append("SELECT * ");
                usersSqlBuilder.Append("FROM rmaster.users A ");
                usersSqlBuilder.Append("LEFT JOIN rmaster.play_counts B ON B.user_id = A.id AND B.practice_id = {0} ");
                usersSqlBuilder.Append("WHERE A.id in ({1}) ");
                string usersSql = string.Format(usersSqlBuilder.ToString(), practice.id, string.Join(",", practice.organizePlayers));

                DataLoad<Users> usersAccess = new DataLoad<Users>(connection, usersSql);
                userList = usersAccess.load();
            }
        }
        #endregion

        #region データ保存処理
        public void saveData(List<Results> results, List<Users> gamePlayers, Practices practice)
        {
            // データ更新用現在時刻
            DateTime now = DateTime.Now;


            using (MySqlConnection connection = new MySqlConnection(Static.BUILDER.ConnectionString))
            {
                MySqlTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    MySqlCommand matchesInsertCommand = this.matchesInsertCommand(now, practice);
                    matchesInsertCommand.Connection = connection;
                    // matchesのInsertとID取得
                    int matchesId = (int)matchesInsertCommand.ExecuteScalar();

                    // resultへmatchesIdを設定
                    results.ForEach(obj => obj.match_id = matchesId);
                    MySqlCommand resultInsertCommand = this.resultsInsertCommand(now, results);
                    resultInsertCommand.Connection = connection;
                    resultInsertCommand.Transaction = transaction;
                    resultInsertCommand.ExecuteNonQuery();

                    MySqlCommand play_countsUpdateCommand = this.play_countsUpdateCommand(now, practice);
                    play_countsUpdateCommand.Connection = connection;
                    resultInsertCommand.Transaction = transaction;
                    play_countsUpdateCommand.ExecuteNonQuery();

                    // Insertが不要の場合がある
                    MySqlCommand play_countsInsertCommand = this.play_countsInsertCommand(now, practice, gamePlayers);
                    if (play_countsInsertCommand != null)
                    {
                        play_countsInsertCommand.Connection = connection;
                        resultInsertCommand.Transaction = transaction;
                        play_countsInsertCommand.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }
        }

        /// <summary>
        /// matchesのInsert用コマンド
        /// 加えてIDを取得する
        /// </summary>
        private MySqlCommand matchesInsertCommand(DateTime nowTime, Practices practice)
        {

            // matches 挿入用SQL作成
            StringBuilder matchesBuilder = new StringBuilder();
            matchesBuilder.Append("INSERT INTO rmaster.matches ");
            matchesBuilder.Append("(practice_id, link, type, created_at, updated_at) ");
            matchesBuilder.Append("VALUES (@practice_id, @link, @type, @created_at, @updated_at); ");
            matchesBuilder.Append("SELECT MAX(id) FROM rmaster.matches WHERE practice_id = " + practice.id);
            MySqlCommand matchesInsertCommand = new MySqlCommand(matchesBuilder.ToString());
            matchesInsertCommand.Parameters.Add(new MySqlParameter("practice_id", practice.id));
            matchesInsertCommand.Parameters.Add(new MySqlParameter("link", null));
            matchesInsertCommand.Parameters.Add(new MySqlParameter("type", 2));
            matchesInsertCommand.Parameters.Add(new MySqlParameter("created_at", nowTime));
            matchesInsertCommand.Parameters.Add(new MySqlParameter("updated_at", nowTime));
            return matchesInsertCommand;
        }

        private MySqlCommand resultsInsertCommand(DateTime nowTime, List<Results> results)
        {
            // result 挿入用SQL作成
            int resultCount = results.Count;
            List<MySqlParameter> resultSqlParameter = new List<MySqlParameter>();
            StringBuilder resultBuilder = new StringBuilder();
            for (int i = 0; i < resultCount; i++)
            {
                resultBuilder.Append("INSERT INTO rmaster.results ");
                resultBuilder.Append("(court, pair, match_id, user1, user2, created_at, updated_at) ");
                resultBuilder.Append(string.Format("VALUES (@court_{0}, @pair_{0}, @match_id_{0}, @user1_{0}, @user2_{0}, @created_at_{0}, @updated_at{0} );", i));
                resultSqlParameter.Add(new MySqlParameter("court_" + i, results[i].count));
                resultSqlParameter.Add(new MySqlParameter("pair_" + i, results[i].pair));
                resultSqlParameter.Add(new MySqlParameter("match_id_" + i, results[i].match_id));
                resultSqlParameter.Add(new MySqlParameter("user1_" + i, results[i].user1));
                resultSqlParameter.Add(new MySqlParameter("user2_" + i, results[i].user2));
                resultSqlParameter.Add(new MySqlParameter("created_at_" + i, nowTime));
                resultSqlParameter.Add(new MySqlParameter("updated_at" + i, nowTime));
            }
            MySqlCommand resultInsertCommand = new MySqlCommand(resultBuilder.ToString());
            resultInsertCommand.Parameters.AddRange(resultSqlParameter.ToArray());
            return resultInsertCommand;
        }

        private MySqlCommand play_countsUpdateCommand(DateTime nowTime, Practices practice)
        {
            // play_counts更新用SQL
            StringBuilder play_countsUpdateBuilder = new StringBuilder();
            play_countsUpdateBuilder.Append("UPDATE rmaster.play_counts ");
            play_countsUpdateBuilder.Append("SET count = count + 1, updated_at = @updated_at ");
            play_countsUpdateBuilder.Append("where practice_id = @practiceId ");
            MySqlCommand play_countsUpdateCommand = new MySqlCommand(play_countsUpdateBuilder.ToString());
            play_countsUpdateCommand.Parameters.Add(new MySqlParameter("practiceId", practice.id));
            play_countsUpdateCommand.Parameters.Add(new MySqlParameter("updated_at", nowTime));
            return play_countsUpdateCommand;
        }
        private MySqlCommand play_countsInsertCommand(DateTime nowTime, Practices practice, List<Users> gamePlayers)
        {
            // play_counts挿入用SQL
            List<Users> playCountsInsertUsers = gamePlayers.Where(user => user.count == null).ToList();
            int play_countsInsertCount = playCountsInsertUsers.Count();
            if (play_countsInsertCount == 0) return null;
            List<MySqlParameter> play_countsSqlParameter = new List<MySqlParameter>();
            StringBuilder play_countsInsertBuilder = new StringBuilder();
            for (int i = 0; i < play_countsInsertCount; i++)
            {
                play_countsInsertBuilder.Append("INSERT INTO rmaster.play_counts ");
                play_countsInsertBuilder.Append("(practice_id, user_id, count, created_at, updated_at) ");
                play_countsInsertBuilder.Append(string.Format("VALUES (@practice_id_{0}, @user_id_{0}, @count_{0}, @created_at_{0}, @updated_at{0} ); ", i));
                play_countsSqlParameter.Add(new MySqlParameter("practice_id_" + i, practice.id));
                play_countsSqlParameter.Add(new MySqlParameter("user_id_" + i, playCountsInsertUsers[i].id));
                play_countsSqlParameter.Add(new MySqlParameter("count_" + i, 1));
                play_countsSqlParameter.Add(new MySqlParameter("created_at_" + i, nowTime));
                play_countsSqlParameter.Add(new MySqlParameter("updated_at" + i, nowTime));
            }
            MySqlCommand play_countsInsertCommand = new MySqlCommand(play_countsInsertBuilder.ToString());
            play_countsInsertCommand.Parameters.AddRange(play_countsSqlParameter.ToArray());
            return play_countsInsertCommand;
        }

        #endregion


        private static readonly string CALL_COAT = "第{0}コート";
        private static readonly string CALL_PAIR = "[{0}] [{1}]";
        private static readonly string CALL_VIRSUS = " 対 ";
        private static readonly string CARD_ROW = "{0} : [{1}]-[{2}]";

        private (string, SimpleCard) createAlexaResponse(List<Results> gameCombis, List<Users> gamePlayers)
        {
            StringBuilder callSb = new StringBuilder();
            StringBuilder cardSb = new StringBuilder();
            for (int i = 0; i < gameCombis.Count/2; i++)
            {
                callSb.Append(string.Format(CALL_COAT, i + 1));
                callSb.Append(string.Format(CALL_PAIR, gamePlayers.Where(usr => usr.id == gameCombis[2*i].user1).FirstOrDefault().name_kana, gamePlayers.Where(usr => usr.id == gameCombis[2 * i].user2).FirstOrDefault().name_kana));
                callSb.Append(string.Format(CALL_VIRSUS, i));
                callSb.Append(string.Format(CALL_PAIR, gamePlayers.Where(usr => usr.id == gameCombis[2 * i + 1].user1).FirstOrDefault().name_kana, gamePlayers.Where(usr => usr.id == gameCombis[2 * i + 1].user2).FirstOrDefault().name_kana));
                callSb.Append("　。　");

                cardSb.Append(string.Format(CARD_ROW, i + 1, gamePlayers.Where(usr => usr.id == gameCombis[2 * i].user1).FirstOrDefault().name, gamePlayers.Where(usr => usr.id == gameCombis[2 * i].user2).FirstOrDefault().name));
                cardSb.Append("\n");
                cardSb.Append(string.Format(CARD_ROW, i + 1, gamePlayers.Where(usr => usr.id == gameCombis[2 * i + 1].user1).FirstOrDefault().name, gamePlayers.Where(usr => usr.id == gameCombis[2 * i + 1].user2).FirstOrDefault().name));
                cardSb.Append("\n");
            }

            var card = new SimpleCard()
            {
                Title = "ペア決め",
                Content = cardSb.ToString(),
            };

            return (callSb.ToString(), card);
        }
    }
}


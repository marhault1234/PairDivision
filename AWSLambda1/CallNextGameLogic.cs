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
using static AWSLambda1.CallNextGameLogic;
using static AWSLambda1.Function;

namespace AWSLambda1
{

    public class CallNextGameLogic
    {
        public enum SelectionPatternEnum
        {
            /// <summary>
            /// 試合割合が少ない人を優先に選択し、
            /// ランダムな組み合わせでコールする
            /// </summary>
            RandomCall,
            /// <summary>
            /// 試合割合が少ない人を優先に選択し、
            /// 出来るだけミックス・男女別にコールする。
            /// </summary>
            DefaultCall,
            /// <summary>
            /// 男女が同数になるように、
            /// それぞれ試合割合が少ない人を優先に選択しコールする。
            /// コート数に対していずれかの性別が足りない場合は適当に設定する。
            /// </summary>
            MixCall,
        }

        #region 次の試合をコール
        public SkillResponse2 callNextGame(ILambdaContext context, string uid, SelectionPatternEnum pattern)
        {
            Random rnd = new Random(DateTime.Now.Hour * 3600 + DateTime.Now.Minute * 60 + DateTime.Now.Second);

            // ***** データ読み込み *****
            this.loadPracticeData(out Practices practice, out List<Results> resultList, out List<Users> userList);

            // 全ゲームに参加したユーザー情報（回数カウント用）
            List<int> gamePlayerLog = new List<int>();
            List<int> lastGameUser = new List<int>();
            List<Results> lastGames = new List<Results>();
            if (resultList.Any())
            {
                int maxMatchId = resultList.Select(r => r.match_id).Max();
                foreach (var r in resultList.OrderByDescending(r => r.match_id))
                {
                    gamePlayerLog.Add(r.user1);
                    gamePlayerLog.Add(r.user2);
                    if (r.match_id == maxMatchId)
                    {
                        lastGameUser.Add(r.user1);
                        lastGameUser.Add(r.user2);
                        lastGames.Add(r);
                    }
                };
            }
            // 各ユーザーにランダム値と試合率、直前の試合有無を設定
            userList.ForEach(user => { 
                user.randomInt = rnd.Next(9999999); 
                user.playPercentage = (user.count ?? 0) == 0 ? 1 : gamePlayerLog.Count(obj => obj == user.id) / (decimal)user.count ;
                user.ContinuousCount = lastGameUser.Count(obj => obj == user.id);
            });

            // コート数取得。最大３コート
            int coatCount = getCoat(userList);

            // 次ゲームの参加者決め
            List<Users> gamePlayers = pattern.getPicker().playerSelect(userList, resultList, coatCount);

            List<Results> addMatchesAndResults;
            bool decision;
            do
            {
                decision = false;
                // 各ユーザーのランダム値更新
                gamePlayers.ForEach(user => { user.randomInt = rnd.Next(9999999); });
                // ペア決め
                addMatchesAndResults = pattern.getSelecter().getPairList(gamePlayers, resultList, coatCount);
                addMatchesAndResults.ForEach(r => decision = decision || lastGames.Select(l => l.pairCheckStr).Contains(r.pairCheckStr));
            } while (decision);

            // DB更新
            this.saveData(addMatchesAndResults, gamePlayers, practice, userList);

            (string msg,SimpleCard card) msgCard  = createAlexaResponse(addMatchesAndResults, gamePlayers);
            SkillResponse2 skillResponse = new SkillResponse2();
            skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = msgCard.msg };
            skillResponse.Response.Card = msgCard.card;
            skillResponse.Response.ShouldEndSession = null;

            return skillResponse;
        }
        #endregion

        private int getCoat(List<Users> users)
        {
            int coatCount = users.Count / 4;
            coatCount = coatCount > Static.MAX_COAT ? Static.MAX_COAT : coatCount;
            return coatCount;
        }


        #region RepeatIntent 過去の最新の試合を再コール
        public SkillResponse2 callRepeat(ILambdaContext context, string uid)
        {
            // ***** データ読み込み *****
            this.loadPracticeData(out Practices practice, out List<Results> resultList, out List<Users> userList);
            this.loadAllUser(out List<Users> loadAllUser);

            var repeatResult = resultList.Where(result => result.match_id == resultList.Max(obj => obj.match_id)).OrderBy(obj => obj.id).ToList();

            (string msg, SimpleCard card) msgCard = createAlexaResponse(repeatResult, loadAllUser);
            SkillResponse2 skillResponse = new SkillResponse2();
            skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = msgCard.msg };
            skillResponse.Response.Card = msgCard.card;
            skillResponse.Response.ShouldEndSession = null;
            

            return skillResponse;

        }
        #endregion


        #region DBデータ取得処理
        /// <summary>
        /// 練習データ取得。練習情報、履歴情報、参加者情報
        /// </summary>
        /// <param name="practice"></param>
        /// <param name="resultList"></param>
        /// <param name="userList"></param>
        public void loadPracticeData(out Practices practice, out List<Results> resultList, out List<Users> userList)
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
        public void loadAllUser(out List<Users> allUsersList)
        {
            using (MySqlConnection connection = new MySqlConnection(Static.BUILDER.ConnectionString))
            {
                connection.Open();
                // 練習参加者ロードSQL
                StringBuilder usersSqlBuilder = new StringBuilder();
                usersSqlBuilder.Append("SELECT * ");
                usersSqlBuilder.Append("FROM rmaster.users ");
                string usersSql = usersSqlBuilder.ToString();

                DataLoad<Users> usersAccess = new DataLoad<Users>(connection, usersSql);
                allUsersList = usersAccess.load();
            }
        }
            #endregion

        #region データ保存処理
            public void saveData(List<Results> results, List<Users> gamePlayers, Practices practice, List<Users> allUsers)
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

                    // カウントは全ユーザーでプラス
                    MySqlCommand play_countsUpdateCommand = this.play_countsUpdateCommand(now, practice, allUsers);
                    play_countsUpdateCommand.Connection = connection;
                    resultInsertCommand.Transaction = transaction;
                    play_countsUpdateCommand.ExecuteNonQuery();

                    // Insertが不要の場合がある
                    MySqlCommand play_countsInsertCommand = this.play_countsInsertCommand(now, practice, allUsers);
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

        private MySqlCommand play_countsUpdateCommand(DateTime nowTime, Practices practice, List<Users> allUsers)
        {
            // play_counts更新用SQL
            StringBuilder play_countsUpdateBuilder = new StringBuilder();
            play_countsUpdateBuilder.Append("UPDATE rmaster.play_counts ");
            play_countsUpdateBuilder.Append("SET count = count + 1, updated_at = @updated_at ");
            play_countsUpdateBuilder.Append("WHERE practice_id = @practiceId ");
            play_countsUpdateBuilder.Append("AND user_id in ( ");
            play_countsUpdateBuilder.Append(string.Join(",", allUsers.Select(u => u.id).ToArray()));
            play_countsUpdateBuilder.Append(") ");
            MySqlCommand play_countsUpdateCommand = new MySqlCommand(play_countsUpdateBuilder.ToString());
            play_countsUpdateCommand.Parameters.Add(new MySqlParameter("practiceId", practice.id));
            play_countsUpdateCommand.Parameters.Add(new MySqlParameter("updated_at", nowTime));
            return play_countsUpdateCommand;
        }
        private MySqlCommand play_countsInsertCommand(DateTime nowTime, Practices practice, List<Users> allUsers)
        {
            // play_counts挿入用SQL
            List<Users> playCountsInsertUsers = allUsers.Where(user => user.count == null).ToList();
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

        #region Alexa用レスポンス作成
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
        #endregion
    }
}


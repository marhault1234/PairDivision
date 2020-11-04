using Alexa.NET.Request;
using Alexa.NET.Request.Type;
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
using static AWSLambda1.Function;
using static AWSLambda1.UpdateResultLogic;

namespace AWSLambda1
{

    public class UpdateResultLogic
    {
        public SkillResponse2 showLastResult(ILambdaContext context, string uid)
        {
            // ***** データ読み込み *****
            this.loadPracticeData(out Practices practice, out List<Results> resultList, out List<Users> userList);
            this.loadAllUser(out List<Users> loadAllUser);
            var repeatResult = resultList.Where(result => result.match_id == resultList.Max(obj => obj.match_id)).OrderBy(obj => obj.id).ToList();
            (string msg,SimpleCard card) msgCard = prepareResponse(repeatResult, loadAllUser);
            SkillResponse2 skillResponse = new SkillResponse2();
            skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = msgCard.msg };
            skillResponse.Response.Card = msgCard.card;
            skillResponse.Response.ShouldEndSession = false;
            return skillResponse;
        }

        public SkillResponse2 saveResult(ILambdaContext context, SkillRequest input, string uid)
        {
            IntentRequest ir = (IntentRequest)input.Request;

            // ***** データ読み込み *****
            this.loadPracticeData(out Practices practice, out List<Results> resultList, out List<Users> userList);
            this.loadAllUser(out List<Users> loadAllUser);
            var results = resultList.Where(result => result.match_id == resultList.Max(obj => obj.match_id)).OrderBy(obj => obj.id).ToList();

            //int coat = int.Parse(ir.Intent.Slots[Static.INTENT_SLOT_COAT_NUMBER].Value);
            //int point1 = int.Parse(ir.Intent.Slots[Static.INTENT_SLOT_POINT_ONE].Value);
            //int point2 = int.Parse(ir.Intent.Slots[Static.INTENT_SLOT_POINT_TWO].Value);
            List<string> slotList = new List<string>() { "第一コート", "第二コート", "第三コート", "第四コート", "第五コート", "第六コート", "第七コート", "第八コート" };
            int coat = 0, point1 = 0, point2 = 0;
            bool b1, b2, b3;
            b1 = int.TryParse(ir.Intent.Slots[Static.INTENT_SLOT_COAT_NUMBER].Value, out coat);
            b2 = int.TryParse(ir.Intent.Slots[Static.INTENT_SLOT_POINT_ONE].Value, out point1);
            b3 =int.TryParse(ir.Intent.Slots[Static.INTENT_SLOT_POINT_TWO].Value, out point2);
            try
            {
                if (!b1 || !b2 || !b3) throw new Exception();
                //if(!results.Any(result => result.count == coat))
                //{
                //    SkillResponse2 errorResponse = new SkillResponse2();
                //    errorResponse.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "エラー" };
                //    errorResponse.Response.Card = new SimpleCard() { Content="指定したコート番号が存在しません。", Title= "第" + coat + "コート" };
                //    errorResponse.Response.ShouldEndSession = true;
                //    return errorResponse;
                //}
                updResults(results[0].match_id, coat, point1, point2);

                // ***** データ再読み込み *****
                this.loadPracticeData(out Practices practice2, out List<Results> resultList2, out List<Users> userList2);
                var results2 = resultList2.Where(result => result.match_id == resultList.Max(obj => obj.match_id)).OrderBy(obj => obj.id).ToList();
                (string msg, SimpleCard card) msgCard = executeResponse(results2, loadAllUser);
                SkillResponse2 skillResponse = new SkillResponse2();
                skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = msgCard.msg };
                skillResponse.Response.Card = msgCard.card;
                skillResponse.Response.ShouldEndSession = true;
                return skillResponse;
            }
            catch(Exception e)
            {
                SkillResponse2 errorResponse = new SkillResponse2();
                errorResponse.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "Error" };
                errorResponse.Response.Card = new SimpleCard() { Content="coat = " + coat + "\r\n point1 = " + point1 + "\r\npoint2 = "+point2, Title="Error" };
                errorResponse.Response.ShouldEndSession = true;
                return errorResponse;
            }

        }

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
            public void updResults(int matchId, int coat, int point1, int point2)
        {
            // データ更新用現在時刻
            DateTime now = DateTime.Now;

            using (MySqlConnection connection = new MySqlConnection(Static.BUILDER.ConnectionString))
            {
                MySqlTransaction transaction = null;
                try
                {
                    StringBuilder resultBuilder = new StringBuilder();
                    resultBuilder.Append("UPDATE rmaster.results ");
                    resultBuilder.Append("SET point = @point1,updated_at = @now ");
                    resultBuilder.Append("WHERE match_id = @match_id AND court = @court AND pair = @pair1; ");
                    resultBuilder.Append("UPDATE rmaster.results ");
                    resultBuilder.Append("SET point = @point2 ");
                    resultBuilder.Append("WHERE match_id = @match_id AND court = @court AND pair = @pair2; ");
                    MySqlCommand resultInsertCommand = new MySqlCommand(resultBuilder.ToString());
                    resultInsertCommand.Parameters.Add(new MySqlParameter("now", now));
                    resultInsertCommand.Parameters.Add(new MySqlParameter("match_id", matchId));
                    resultInsertCommand.Parameters.Add(new MySqlParameter("court", coat));
                    resultInsertCommand.Parameters.Add(new MySqlParameter("pair1", 1));
                    resultInsertCommand.Parameters.Add(new MySqlParameter("point1", point1));
                    resultInsertCommand.Parameters.Add(new MySqlParameter("pair2", 2));
                    resultInsertCommand.Parameters.Add(new MySqlParameter("point2", point2));

                    connection.Open();
                    transaction = connection.BeginTransaction();
                    resultInsertCommand.Connection = connection;
                    resultInsertCommand.Transaction = transaction;
                    resultInsertCommand.ExecuteNonQuery();

                    transaction.Commit();
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }
        }


        #endregion

        #region Alexa用レスポンス作成
        private static readonly string CARD_ROW = "{0} : [{1}]-[{2}] {3}点";

        private (string, SimpleCard) prepareResponse(List<Results> gameCombis, List<Users> gamePlayers)
        {
            StringBuilder callSb = new StringBuilder();
            StringBuilder cardSb = new StringBuilder();
            for (int i = 0; i < gameCombis.Count/2; i++)
            {

                cardSb.Append(string.Format(CARD_ROW, i + 1, gamePlayers.Where(usr => usr.id == gameCombis[2 * i].user1).FirstOrDefault().name, gamePlayers.Where(usr => usr.id == gameCombis[2 * i].user2).FirstOrDefault().name, gameCombis[2 * i].point.ToString("00")));
                cardSb.Append("\n");
                cardSb.Append(string.Format(CARD_ROW, i + 1, gamePlayers.Where(usr => usr.id == gameCombis[2 * i + 1].user1).FirstOrDefault().name, gamePlayers.Where(usr => usr.id == gameCombis[2 * i + 1].user2).FirstOrDefault().name, gameCombis[2 * i + 1].point.ToString("00")));
                cardSb.Append("\n");
            }
            callSb.Append("入力をどうぞ。");
            cardSb.Append("「第〇コート、XX 対 XX」の形式で入力してください。");
            var card = new SimpleCard()
            {
                Title = "結果入力",
                Content = cardSb.ToString(),
            };

            return (callSb.ToString(), card);
        }
        private (string, SimpleCard) executeResponse(List<Results> gameCombis, List<Users> gamePlayers)
        {
            StringBuilder callSb = new StringBuilder();
            StringBuilder cardSb = new StringBuilder();
            for (int i = 0; i < gameCombis.Count / 2; i++)
            {

                cardSb.Append(string.Format(CARD_ROW, i + 1, gamePlayers.Where(usr => usr.id == gameCombis[2 * i].user1).FirstOrDefault().name, gamePlayers.Where(usr => usr.id == gameCombis[2 * i].user2).FirstOrDefault().name, gameCombis[2 * i].point.ToString("00")));
                cardSb.Append("\n");
                cardSb.Append(string.Format(CARD_ROW, i + 1, gamePlayers.Where(usr => usr.id == gameCombis[2 * i + 1].user1).FirstOrDefault().name, gamePlayers.Where(usr => usr.id == gameCombis[2 * i + 1].user2).FirstOrDefault().name, gameCombis[2 * i + 1].point.ToString("00")));
                cardSb.Append("\n");
            }
            callSb.Append("入力を行いました。");
            var card = new SimpleCard()
            {
                Title = "結果",
                Content = cardSb.ToString(),
            };

            return (callSb.ToString(), card);
        }
        #endregion
    }
}



using System.Collections.Generic;
using AWSLambda1.Entity;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Alexa.NET.Request.Type;
using AWSLambda1.DynamoDBHelper;
using System;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSLambda1
{
    public class Function
    {
        private class SkillResponse2 : SkillResponse
        {
            public SkillResponse2()
            {
                base.Version = "1.0";
                base.Response = new ResponseBody();
            }
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            var skillResponse = new SkillResponse2();

            switch (input.Request)
            {
                case LaunchRequest lr:
                    skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "スキルを起動しました。" };
                    break;
                case IntentRequest ir:
                    var (msg,card) =MakeResponse(ir, context, input.Session.User.UserId);
                    skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = msg };
                    skillResponse.Response.Card = card;
                    skillResponse.Response.ShouldEndSession = true;
                    break;
                default:
                    skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "不明な呼び出しがされました。" };
                    break;
            }

            return skillResponse;
        }

        private new (string,SimpleCard) MakeResponse(IntentRequest ir, ILambdaContext context, string uid)
        {
            //testdata(context, uid);
            
            switch (ir.Intent.Name)
            {
                case "NextIntent":
                    return Logic.callNextGame(context, uid);
                //case "BackIntent":
                //    break;
                case "RepeatIntent":
                    return Logic.callGameAgain(context, uid);
            }
            throw new Exception();
        }


        private void testdata(ILambdaContext context, string uid)
        {
            // Testデータ生成
            List<Player> p = new List<Player>();
            p.Add(new Player() { Id = 1, GameCount = 0, Rank = 0, Sex = true, Name = "１", });
            p.Add(new Player() { Id = 2, GameCount = 0, Rank = 0, Sex = true, Name = "２", });
            p.Add(new Player() { Id = 3, GameCount = 0, Rank = 0, Sex = true, Name = "３", });
            p.Add(new Player() { Id = 4, GameCount = 0, Rank = 0, Sex = true, Name = "４", });
            p.Add(new Player() { Id = 5, GameCount = 0, Rank = 0, Sex = true, Name = "５", });
            p.Add(new Player() { Id = 6, GameCount = 0, Rank = 0, Sex = true, Name = "６", });
            p.Add(new Player() { Id = 7, GameCount = 0, Rank = 0, Sex = true, Name = "７", });
            p.Add(new Player() { Id = 8, GameCount = 0, Rank = 0, Sex = true, Name = "８", });
            p.Add(new Player() { Id = 9, GameCount = 0, Rank = 0, Sex = true, Name = "９", });
            p.Add(new Player() { Id = 10, GameCount = 0, Rank = 0, Sex = true, Name = "１０", });
            TeamSettingEntity ent1 = new TeamSettingEntity()
            {
                Key = uid,
                SortKey = uid,
                CoatNumber = 2,
                players = p,
            };
            ent1.Save(context);

            GameLogEntity ent2 = new GameLogEntity()
            {
                Key = uid,
                SortKey = uid,
                lastCallTime = new DateTime(1900, 1, 1),

            };
            ent2.Save(context);
        }
    }
}

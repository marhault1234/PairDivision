using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AWSLambda1.Entity;
using AWSLambda1.EntityHelper;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Alexa.NET.Request.Type;

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
            CallValueHelper ch = new CallValueHelper();
            var entity = ch.CallValueSelect(uid,context);
            if (entity == null || entity.Players == null || entity.Players.Count < 4)
            {
                CallValueHelper cvh = new CallValueHelper();
                entity = new CallValueEntity() { GameCount = 0, ID = uid, Players = new List<string>{"enpty",} };
                cvh.CallValueInsert(entity, context);
                return ("ユーザーが見つかりませんでした。\n登録を行ってください。", new SimpleCard() { Title = "エラー", Content = "ユーザー未設定" });
            }
            switch (ir.Intent.Name)
            {
                case "NextIntent":
                    entity.GameCount++;
                    break;
                case "BackIntent":
                    entity.GameCount--;
                    break;
                case "RepeatIntent":
                    break;
            }
            ch.CallValueUpdate(entity, context);
            return Logic.callGame(entity.Players,entity.GameCount);
        }

    }
}

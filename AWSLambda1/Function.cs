
using System.Collections.Generic;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Alexa.NET.Request.Type;
using System;
using static AWSLambda1.UpdateResultLogic;
using static AWSLambda1.CallNextGameLogic;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSLambda1
{
    public class Function
    {
        public class SkillResponse2 : SkillResponse
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
                    skillResponse = MakeResponse(input, context, input.Session.User.UserId);
                    break;
                default:
                    skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = "不明な呼び出しがされました。" };
                    break;
            }

            return skillResponse;
        }

        private SkillResponse2 MakeResponse(SkillRequest input, ILambdaContext context, string uid)
        {
            var skillResponse = new SkillResponse2();
            IntentRequest ir = (IntentRequest)input.Request;

            (string msg, SimpleCard card) msgCard;
            switch (ir.Intent.Name)
            {
                case "dummy":
                    msgCard = ("dummy", new SimpleCard() { Title = "dummy", Content = "dummy" });
                    skillResponse.Response.OutputSpeech = new PlainTextOutputSpeech() { Text = msgCard.msg };
                    skillResponse.Response.Card = msgCard.card;
                    skillResponse.Response.ShouldEndSession = false;
                    break;
                case "NextIntent":
                    skillResponse = new CallNextGameLogic().callNextGame(context, uid, SelectionPatternEnum.DefaultCall);
                    break;
                case "MixIntent":
                    skillResponse = new CallNextGameLogic().callNextGame(context, uid, SelectionPatternEnum.MixCall);
                    break;
                case "RepeatIntent":
                    skillResponse = new CallNextGameLogic().callRepeat(context, uid);
                    break;
                case "ResultUpdatePrepareIntent":
                    skillResponse = new UpdateResultLogic().showLastResult(context, uid);
                    break;
                case "ResultUpdateExecuteIntent":
                    skillResponse = new UpdateResultLogic().saveResult(context, input, uid);
                    break;
            }
            return skillResponse;
        }

    }
}


using System.Collections.Generic;
using Alexa.NET.Request;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Alexa.NET.Request.Type;
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
                    var (msg,card) = MakeResponse(ir, context, input.Session.User.UserId);
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

        private (string,SimpleCard) MakeResponse(IntentRequest ir, ILambdaContext context, string uid)
        {
            //testdata(context, uid);
            
            switch (ir.Intent.Name)
            {
                case "NextIntent":
                    return new Logic().callNextGame(context, uid);
                case "RepeatIntent":
                    return new Logic().callRepeat(context, uid);
                case "dummy":
                    return ("dummy", new SimpleCard() { Title = "dummy", Content = "dummy"});
                    //case "BackIntent":
                    //    break;
                    //case "RepeatIntent":
                    //return new Logic().callGameAgain(context, uid);
            }
            throw new Exception();
        }
    }
}

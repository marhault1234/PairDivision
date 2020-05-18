using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using AWSLambda1.Entity.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AWSLambda1.Entity
{
    [DynamoDBTable("CallValueEntity2")]
    public class CallValueEntity : EntityBase
    {
        [DynamoDBProperty("Players")]
        public List<string> Players { get; set; }

        [DynamoDBProperty("GameCount")]
        public int GameCount { get; set; }

        public static CallValueEntity LoadEntity(ILambdaContext context, string uid)
        {
            return Load<CallValueEntity>(context, uid, uid);
        }

        public override void Save(ILambdaContext context)
        {
            context.Logger.LogLine(JsonConvert.SerializeObject(this));

            // 最初にDynamoDBContextを生成する
            using (var dbContext = new DynamoDBContext(Static.Client))
            {
                // SaveAsyncメソッドでentityがTodosテーブルへ追加
                var insertTask = dbContext.SaveAsync(this);
                insertTask.Wait();
            }
        }
    }
}

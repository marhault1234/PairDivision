using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using AWSLambda1.Entity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;


namespace AWSLambda1.EntityHelper
{
    public class CallValueHelper
    {

        private static readonly AmazonDynamoDBClient Client = new AmazonDynamoDBClient(RegionEndpoint.APNortheast1);

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public void CallValueInsert(CallValueEntity entity, ILambdaContext context)
        {
            context.Logger.LogLine(JsonConvert.SerializeObject(entity));

            // 最初にDynamoDBContextを生成する
            using (var dbContext = new DynamoDBContext(Client))
            {
                // SaveAsyncメソッドでentityがTodosテーブルへ追加
                var insertTask = dbContext.SaveAsync(entity);
                insertTask.Wait();
            }
        }
        public CallValueEntity CallValueSelect(string keyId, ILambdaContext context)
        {
            using (var dbContext = new DynamoDBContext(Client))
            {
                // LoadAsyncメソッドでパーティションキーとソートキーが一致するレコードを取得
                var selectTask = dbContext.LoadAsync<CallValueEntity>(hashKey: keyId);
                selectTask.Wait();
                context.Logger.LogLine(JsonConvert.SerializeObject(selectTask.Result));
                return selectTask.Result;
            }
        }
        public void CallValueUpdate(CallValueEntity entity, ILambdaContext context)
        {
            using (var dbContext = new DynamoDBContext(Client))
            {
                // 更新対象のレコードをLoadAsyncで取得して、
                var updateTask1 = dbContext.LoadAsync<CallValueEntity>(hashKey: entity.ID);
                updateTask1.Wait();
                var updatedEntity = updateTask1.Result;

                // 更新後のレコードをSaveAsyncでテーブルに追加する
                // （すなわち、パーティションキーとソートキーが一致する場合は上書きされる）
                updatedEntity.GameCount = entity.GameCount;
                var updateTask2 = dbContext.SaveAsync(entity);
                updateTask2.Wait();

            }
        }
    }
}

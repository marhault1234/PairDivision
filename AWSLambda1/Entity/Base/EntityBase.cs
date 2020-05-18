using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AWSLambda1.Entity.Base
{
    public abstract class EntityBase
    {
        [DynamoDBHashKey("Key")]
        public string Key { get; set; }

        [DynamoDBRangeKey("SortKey")]
        public  string SortKey { get; set; }


        /// <summary>
        /// データの挿入・更新
        /// キーが一致する場合更新。しない場合挿入
        /// 基底クラスで実装するとうまくいかないので継承後に実装するように。
        /// </summary>
        /// <param name="context"></param>
        public abstract void Save(ILambdaContext context);

        /// <summary>
        /// UID,SortKeyを指定し、一致するエンティティを取得る。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="UID"></param>
        /// <param name="SortKey"></param>
        /// <returns></returns>
        public static T Load<T>(ILambdaContext context, string Key, string SortKey)
        {
            using (var dbContext = new DynamoDBContext(Static.Client))
            {
                // LoadAsyncメソッドでパーティションキーとソートキーが一致するレコードを取得
                // var selectTask = dbContext.LoadAsync<T>(hashKey: Key, rangeKey: SortKey);
                var selectTask = dbContext.LoadAsync<T>(hashKey: Key, rangeKey: SortKey);
                selectTask.Wait();
                context.Logger.LogLine(JsonConvert.SerializeObject(selectTask.Result));
                return selectTask.Result;
            }
        }
    }
}

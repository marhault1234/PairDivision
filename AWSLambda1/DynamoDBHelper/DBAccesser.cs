using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using AWSLambda1.Entity;
using AWSLambda1.Entity.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AWSLambda1.DynamoDBHelper
{
    public class DBAccesser <T> where T : EntityBase
    {
        /// <summary>
        /// データの挿入・更新
        /// キーが一致する場合更新。しない場合挿入
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entity">挿入対象エンティティ</param>
        public void Update(ILambdaContext context, T entity)
        {
            context.Logger.LogLine(JsonConvert.SerializeObject(entity));

            // 最初にDynamoDBContextを生成する
            using (var dbContext = new DynamoDBContext(Static.Client))
            {
                // SaveAsyncメソッドでentityがTodosテーブルへ追加
                var insertTask = dbContext.SaveAsync(entity);
                insertTask.Wait();
            }
        }
        /// <summary>
        /// データの挿入・更新
        /// キーが一致する場合更新。しない場合挿入
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entity">挿入対象エンティティ</param>
        public void Update(ILambdaContext context, List<T> entityList)
        {
            // 最初にDynamoDBContextを生成する
            using (var dbContext = new DynamoDBContext(Static.Client))
            {
                // SaveAsyncメソッドでentityがTodosテーブルへ追加
                foreach(T entity in entityList) 
                {
                    var insertTask = dbContext.SaveAsync(entity);
                    insertTask.Wait();
                }
            }
        }

        /// <summary>
        /// UID,SortKeyを指定し、一致するエンティティを取得る。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="UID"></param>
        /// <param name="SortKey"></param>
        /// <returns></returns>
        public T Select(ILambdaContext context, string Key, string SortKey)
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
        /*
        /// <summary>
        /// UIDを指定し、該当するレコードを全て返却する。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="UID"></param>
        public List<T> Select(ILambdaContext context, string Key)
        {
            var property = typeof(T).GetProperty("DynamoDBTable");

            DynamoDBTableAttribute attr = (DynamoDBTableAttribute)typeof(T).GetProperty("DynamoDBTable").GetCustomAttributes(false)[0];
            var request = new QueryRequest
            {
                TableName = attr.TableName,
                KeyConditionExpression = "UID = :uid",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue> {
                {":uid", new AttributeValue { S =  Key}}}
            };
            var response = Static.Client.QueryAsync(request);
            response.Wait();

            List<T> result = (List<T>)Activator.CreateInstance(typeof(List<T>));
            foreach (var dic in response.Result.Items)
            {
                T instance = (T)Activator.CreateInstance(typeof(T));
                foreach (var props in typeof(T).GetProperties())
                {
                    props.SetValue(instance, dic[props.Name]);
                }
                result.Add(instance);
            }

            return result;
        }*/
    }
}

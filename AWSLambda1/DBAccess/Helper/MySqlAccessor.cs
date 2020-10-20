using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AWSLambda1.DBAccess
{
    /// <summary>
    /// データロード用クラス
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataLoad<T> where T: TableData, new()
    {
        private DataTable dataTable { get; set; }
        public DataLoad(MySqlConnection connection, string sql) 
        {
            using (var Adapter = new MySqlDataAdapter(sql, connection))
            {
                DataTable dt = new DataTable();
                Adapter.Fill(dt);
                this.dataTable = dt;
            }
        }

        public List<T> load()
        {
            List<T> result = new List<T>();
            foreach (DataRow dr in this.dataTable.Rows)
            {
                var temp = new T();
                foreach (var prop in typeof(T).GetProperties())
                {
                    if (prop.GetCustomAttributes(typeof(NoDbColumnAttribute), false) == null) continue;
                    try
                    {
                        prop.SetValue(temp, dr[prop.Name]);
                    }
                    catch (Exception e)
                    {
                        // データが取得出来ない場合は何もしない
                    }
                }
                result.Add(temp);
            }
            return result;
        }
    }

    /// <summary>
    /// DB カラムでないことを表すAttribute
    /// </summary>
    public class NoDbColumnAttribute : Attribute { }

    public abstract class TableData
    {
        public int id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime deleted_at { get; set; }
    }
}

using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace AWSLambda1.Entity
{
    [DynamoDBTable("CallValues")]
    public class CallValueEntity
    {
        [DynamoDBHashKey("ID")]
        public string ID { get; set; }

        [DynamoDBProperty("Players")]
        public List<string> Players { get; set; }

        [DynamoDBProperty("GameCount")]
        public int GameCount { get; set; }


    }
}

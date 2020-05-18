using Amazon;
using Amazon.DynamoDBv2;
using System;
using System.Collections.Generic;
using System.Text;

namespace AWSLambda1
{
    public static class Static
    {
        public static readonly AmazonDynamoDBClient Client = new AmazonDynamoDBClient(RegionEndpoint.APNortheast1);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace AWSLambda1.DBAccess
{

    public class Results : Matches
    {
        public int count { get; set; }
        public int pair { get; set; }
        public int match_id { get; set; }
        public int userq { get; set; }
        public int user1 { get; set; }
        public int user2 { get; set; }
        public int point { get; set; }
    }
    public class Matches : TableData
    {
        public int practice_id { get; set; }
        public string link { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Text;

namespace AWSLambda1.DBAccess
{
    public class Practices : TableData
    {
        public int placeid { get; set; }
        public DateTime start_at { get; set; }
        public DateTime end_at { get; set; }
        public int is_active { get; set; }
        public string players { get; set; }

        [NoDbColumn]
        public string organizePlayers { 
            get 
            {
                return this.players.Replace("[", "").Replace("]", "");
            } 
        }
    }
}

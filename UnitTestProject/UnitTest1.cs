using Microsoft.VisualStudio.TestTools.UnitTesting;
using AWSLambda1.Entity;
using System.Collections.Generic;
using AWSLambda1.Settings;
using static AWSLambda1.Entity.GameLogEntity;
using System;
using AWSLambda1;
using AWSLambda1.Distribution;

namespace UnitTestProject
{
    [TestClass]
    public class UnitTest1
    {
        public class TestDate
        {
            int coat = 3;
            int player = 14;

            public TeamSettingEntity getTeamSettingData()
            {
                List<Player> players = new List<Player>();
                for (int i = 1; i < player+1; i++)
                    players.Add(new Player()
                    {
                        Id = i,
                        Name = i.ToString("0000"),
                        Sex = true,
                        Rank = 0,
                        GameCount = 0,
                    });

                return new TeamSettingEntity()
                {
                    CoatNumber = coat,
                    players = players,
                };
            }

            public GameLogEntity getLogData()
            {
                GameLogEntity result = new GameLogEntity();
                return result;
            }
        }

        public static TeamSettingEntity setting;
        public static GameLogEntity log;
        public static int Count = 0;


        public void createPair()
        {
            if (setting == null) setting = new TestDate().getTeamSettingData();
            if (log == null) log = new TestDate().getLogData();

            // ゲームプレイヤー選択、設定で変更できるようにするならそれ用にする
            IPlayerSelect playerSelect = new DefaultPick();
            List<Player> gamePlayers = playerSelect.playerSelect(setting);

            // ペア決め
            AbstractPairSelectSetting pairSelect = new RandomPairSelect();
            List<GameCombi> nextCombi = pairSelect.getPairList(gamePlayers, ref log);

            for (int i = 0; i < nextCombi.Count; i++)
                //System.Diagnostics.Trace.WriteLine(++Count + "：" + nextCombi[i].testCombStrValue); 
                Console.WriteLine(++Count + ":" + nextCombi[i].testCombStrValue);
        }


        [TestMethod]
        public void TestMethod1()
        {
            Console.WriteLine("開始：" + DateTime.Now);
            for (int i = 0; i < 10; i++) createPair();
            Console.WriteLine("終了：" + DateTime.Now);
        }
    }
}

using Alexa.NET.Response;
using Amazon.Lambda.Core;
using AWSLambda1.Distribution;
using AWSLambda1.DynamoDBHelper;
using AWSLambda1.Entity;
using AWSLambda1.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AWSLambda1.Entity.GameLogEntity;
using static AWSLambda1.Logic;

namespace AWSLambda1
{
    public static class EnumExt
    {
        /// <summary>
        /// プレイヤーのピック条件
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        public static IPlayerSelect getPicker(this SelectionPatternEnum selection)
        {
            switch(selection)
            {
                case SelectionPatternEnum.EqualityRandomCall:
                    return new DefaultPick();
                default:return null;
            }
        }
        /// <summary>
        /// プレイヤーの組み合わせ条件
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        public static AbstractPairSelectSetting getSelecter(this SelectionPatternEnum selection)
        {
            switch (selection)
            {
                case SelectionPatternEnum.EqualityRandomCall:
                    return new RandomPairSelect();
                default: return null;
            }
        }
    }
    public static class Logic
    {
        public enum SelectionPatternEnum
        {
            /// <summary>
            /// 試合数が均一になるように人選し、
            /// ランダムな組み合わせでコールする
            /// </summary>
            EqualityRandomCall,
        }

        private static readonly string CALL_COAT = "第{0}コート";
        private static readonly string CALL_PAIR = "[{0}] [{1}]";
        private static readonly string CALL_VIRSUS = " 対 ";
        private static readonly string CARD_ROW = "{0} : [{1}]-[{2}]";

        public static (string, SimpleCard) callGameAgain(ILambdaContext context, string uid)
        {
            // プレイヤー情報のロード
            TeamSettingEntity teamSettingEntity = TeamSettingEntity.Load<TeamSettingEntity>(context, uid, uid);     // チーム情報
            if (teamSettingEntity.players.Count == 0) return ("データの登録を行ってください。", new SimpleCard() {
                Title = "エラー",
                Content = "プレイヤーの登録を行ってください。", });

            GameLogEntity gameLogEntity = GameLogEntity.Load<GameLogEntity>(context, uid, uid);                     // 過去試合情報
            if (gameLogEntity == null) return ("次の試合をコールしてください。", new SimpleCard(){   
                Title = "エラー",
                Content = "次の試合をコールしてください。",});
            List<GameCombi> combi = new List<GameCombi>();
            int listCount = gameLogEntity.gameLogList.Count();
            for (int i = 0; i < teamSettingEntity.CoatNumber; i++)
            {
                combi.Add(gameLogEntity.gameLogList[listCount - 1 - i]);
            }

            // ログの最終試合のコールを行う
            return createAlexaResponse(teamSettingEntity.players, combi);

        }
        public static (string, SimpleCard) callNextGame(ILambdaContext context, string uid)
        {
            // プレイヤー情報のロード
            TeamSettingEntity teamSettingEntity = TeamSettingEntity.Load<TeamSettingEntity>(context, uid, uid);     // チーム情報
            if (teamSettingEntity.players.Count == 0) return ("データの登録を行ってください。", new SimpleCard()
            {
                Title = "エラー",
                Content ="プレイヤーの登録を行ってください。",
            });

            GameLogEntity gameLogEntity = GameLogEntity.Load<GameLogEntity>(context, uid, uid);                     // 過去試合情報
            if (gameLogEntity == null) gameLogEntity = new GameLogEntity() { Key = uid, SortKey = uid };
            var logList = gameLogEntity.gameLogList;

            // 試合ログが無い場合、各プレイヤーの試合数をリセット、ログリストを初期化
            if (logList.Count == 0)
            {
                var players = teamSettingEntity.players;
                players.ForEach(player => player.GameCount = 0);
                teamSettingEntity.players = players;
                gameLogEntity.logClear();
            }

            // チーム設定からペア決めのロジックを設定【予定】
            SelectionPatternEnum selectionPattern = SelectionPatternEnum.EqualityRandomCall;

            // ゲームプレイヤー選択、設定で変更できるようにするならそれ用にする
            IPlayerSelect playerSelect = selectionPattern.getPicker();
            List<Player> gamePlayers = playerSelect.playerSelect(teamSettingEntity);

            // ペア決め
            AbstractPairSelectSetting pairSelect = selectionPattern.getSelecter();
            List<GameCombi> nextCombi = pairSelect.getPairList(gamePlayers, ref gameLogEntity);

            gameLogEntity.Save(context);

            // ゲームプレイヤーの試合数を増やす
            var gamePlayersId = gamePlayers.Select(obj => obj.Id);
            List<Player> playersCopy = teamSettingEntity.players;
            playersCopy.ForEach(obj => { if (gamePlayersId.Contains(obj.Id)) obj.GameCount++; });
            teamSettingEntity.players = playersCopy;

            teamSettingEntity.Save(context);

            return createAlexaResponse(gamePlayers, nextCombi);
        }
        private static (string, SimpleCard) createAlexaResponse(List<Player> gamePlayers, List<GameCombi> gameCombis)
        {
            // コール作成
            List<Player> callPlayers = new List<Player>();
            foreach (GameCombi comb in gameCombis)
            {
                callPlayers.Add(gamePlayers.Where(obj => obj.Id == comb.comb1.player1Id).FirstOrDefault());
                callPlayers.Add(gamePlayers.Where(obj => obj.Id == comb.comb1.player2Id).FirstOrDefault());
                callPlayers.Add(gamePlayers.Where(obj => obj.Id == comb.comb2.player1Id).FirstOrDefault());
                callPlayers.Add(gamePlayers.Where(obj => obj.Id == comb.comb2.player2Id).FirstOrDefault());
            }

            StringBuilder callSb = new StringBuilder();
            StringBuilder cardSb = new StringBuilder();
            for (int i = 0; i < gameCombis.Count; i++)
            {
                callSb.Append(string.Format(CALL_COAT, i + 1));
                callSb.Append(string.Format(CALL_PAIR, callPlayers[i * 4].Name, callPlayers[i * 4 + 1].Name));
                callSb.Append(string.Format(CALL_VIRSUS, i));
                callSb.Append(string.Format(CALL_PAIR, callPlayers[i * 4 + 2].Name, callPlayers[i * 4 + 3].Name));
                callSb.Append("　。　");

                cardSb.Append(string.Format(CARD_ROW, i + 1, callPlayers[i * 4].Name, callPlayers[i * 4 + 1].Name));
                cardSb.Append("\n");
                cardSb.Append(string.Format(CARD_ROW, i + 1, callPlayers[i * 4 + 2].Name, callPlayers[i * 4 + 3].Name));
                cardSb.Append("\n");
            }

            var card = new SimpleCard()
            {
                Title = "ペア決め",
                Content = cardSb.ToString(),
            };

            return (callSb.ToString(), card);
        }
    }

}


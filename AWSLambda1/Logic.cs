using Alexa.NET.Response;
using Amazon.Lambda.Core;
using AWSLambda1.DynamoDBHelper;
using AWSLambda1.Entity;
using AWSLambda1.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AWSLambda1.Entity.GameLogEntity;

namespace AWSLambda1
{
    public static class Logic
    {
        private static readonly string CALL_COAT = "第{0}コート";
        private static readonly string CALL_PAIR = "[{0}] [{1}]";
        private static readonly string CALL_VIRSUS = " 対 ";

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
                teamSettingEntity.players.ForEach(player => player.GameCount = 0);
                gameLogEntity.logClear();
            }
            // ゲームプレイヤー選択、設定で変更できるようにするならそれ用にする
            IPlayerSelect playerSelect = new PlayerSelectDefault();
            List<Player> gamePlayers = playerSelect.playerSelect(teamSettingEntity);

            // ペア決め
            AbstractPairSelectSetting pairSelect = new RandomPairSelect();
            List<GameCombi> nextCombi = pairSelect.getPairList(context, gamePlayers, gameLogEntity);

            // ゲームプレイヤーの試合数を増やす
            var gamePlayersId = gamePlayers.Select(obj => obj.Id);

            foreach (var id in gamePlayersId)
            {
                teamSettingEntity.players.Where(obj => obj.Id == id).FirstOrDefault().GameCount++;
            }

            teamSettingEntity.players.ForEach(obj => { if (gamePlayersId.Contains(obj.Id)) obj.GameCount++; });

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

                cardSb.Append(string.Format(CALL_COAT, i + 1));
                cardSb.Append("\n");
                cardSb.Append(string.Format(CALL_PAIR, callPlayers[i * 4].Name, callPlayers[i * 4 + 1].Name));
                cardSb.Append("\n");
                cardSb.Append(string.Format(CALL_PAIR, callPlayers[i * 4 + 2].Name, callPlayers[i * 4 + 3].Name));
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


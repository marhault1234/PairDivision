using AWSLambda1.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AWSLambda1.Settings
{
    public interface IPlayerSelect
    {
        List<Player> playerSelect(TeamSettingEntity teamSettingEntity);
    }
    public class PlayerSelectDefault : IPlayerSelect
    {
        List<Player> IPlayerSelect.playerSelect(TeamSettingEntity teamSettingEntity)
        {
            var candidatePlayers = new List<Player>();                                                              // 選定対象プレイヤー
            var playersBuf = new List<Player>(teamSettingEntity.players);                                           // バッファ
            playersBuf = playersBuf.Where(obj => obj.GameCount >= 0).ToList();

            // 試合候補者を試合数が少ない人から試合メンバー選定
            while (candidatePlayers.Count < teamSettingEntity.CoatNumber * 4)
            {
                int minCount = playersBuf.Select(obj => obj.GameCount).Min();
                candidatePlayers.AddRange(playersBuf.Where(obj => obj.GameCount == minCount).ToList());
                playersBuf = new List<Player>(playersBuf.Where(obj => obj.GameCount != minCount).ToList());
            }

            var gamePlayers = new List<Player>();                                                                   // ゲームプレイヤー

            // 試合候補者で試合数が少ない順にソートし、必要人数を超える場合は試合数が多い人からランダム選択
            int surplusCount = candidatePlayers.Count - teamSettingEntity.CoatNumber * 4;                           // 余剰人数
            if (surplusCount > 0)
            {
                // ゲーム人数の最大以外のメンバーは試合確定。
                gamePlayers.AddRange(candidatePlayers.Where(obj => obj.GameCount < candidatePlayers.Select(obj2 => obj2.GameCount).Max()));
                // ランダム選択対象
                var buf = candidatePlayers.Where(obj => obj.GameCount == candidatePlayers.Select(obj2 => obj2.GameCount).Max()).ToList();
                // ランダムに余剰人数分メンバーを削る
                Random rnd = new System.Random();
                for (int i = 0; i < surplusCount; i++)
                {
                    buf.RemoveAt(rnd.Next(0, buf.Count - 1));
                }
                gamePlayers.AddRange(buf);
            }
            else
            {
                gamePlayers = candidatePlayers;
            }
            return gamePlayers;
        }
    }
}

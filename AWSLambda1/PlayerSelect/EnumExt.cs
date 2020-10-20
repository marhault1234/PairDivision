using AWSLambda1.PlayerSelect;
using AWSLambda1.Settings;
using System;
using System.Collections.Generic;
using System.Text;
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
            switch (selection)
            {
                case SelectionPatternEnum.RandomCall:
                case SelectionPatternEnum.DefaultCall:
                    return new DefaultPick();
                default: return null;
            }
        }
        /// <summary>
        /// プレイヤーの組み合わせ条件
        /// </summary>
        /// <param name="selection"></param>
        /// <returns></returns>
        public static IPairSelect getSelecter(this SelectionPatternEnum selection)
        {
            switch (selection)
            {
                case SelectionPatternEnum.RandomCall:
                    return new RandomPairSelect();
                case SelectionPatternEnum.DefaultCall:
                    return new DefaultPairSelect();
                default: return null;
            }
        }
    }
}

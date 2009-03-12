using System;
using System.Collections.Generic;
using System.Text;

namespace RogueBasin
{
    static class Utility
    {
        public static int d20()
        {
            return 1 + Game.Random.Next(20);
        }

        public static int DamageRoll(int damageBase)
        {
            return 1 + Game.Random.Next(damageBase);
        }

        /// <summary>
        /// Properly cuts the string on a white space and append appendWhenCut when cutted.
        /// </summary>
        /// <param name="me"></param>
        /// <param name="appendWhenCut"></param>
        /// <param name="maxLength"></param>
        /// <returns></returns>
        public static string SubstringWordCut(string str, string appendWhenCut, uint maxLength)
        {
            if (str.Length > maxLength)
            {
                str = str.Substring(0, (int)maxLength - appendWhenCut.Length);
                char[] cutPossible = new char[] { ' ', ',', '.', '?', '!', ':', ';', '-', '\n', '\r', '\t' };
                int cutIndex = str.LastIndexOfAny(cutPossible);
                if (cutIndex > 0)
                { return str.Substring(0, cutIndex).Trim() + appendWhenCut; }
                else
                { return str.Substring(0, (int)maxLength - appendWhenCut.Length) + appendWhenCut; }
            }
            return str;
        } 
    }
}

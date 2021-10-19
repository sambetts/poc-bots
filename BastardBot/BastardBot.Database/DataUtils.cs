using System;
using System.Collections.Generic;
using System.Text;

namespace BastardBot.Common
{
    public class DataUtils
    {
        public static bool IsValidInsult(string insult)
        {
            if (string.IsNullOrWhiteSpace(insult))
            {
                return false;
            }
            return insult.Length > 3;
        }
    }
}

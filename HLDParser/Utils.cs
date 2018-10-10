using System;
using System.Collections.Generic;

namespace CascadeParser
{
    public static class Utils
    {
        internal static Tuple<int, int>[] GetStringPairs(string line, int line_number, ILogger inLoger)
        {
            List<int> indices = new List<int>();
            int i = line.IndexOf('"');
            while (i != -1)
            {
                indices.Add(i);
                i = line.IndexOf('"', i + 1);
            }

            if (indices.Count % 2 == 1)
            {
                inLoger.LogError(EErrorCode.NotEvenQuoteCount, line, line_number);
                return new Tuple<int, int>[0];
            }

            var pairs = new Tuple<int, int>[indices.Count / 2];
            for (int k = 0; k < indices.Count; k += 2)
            {
                pairs[k / 2] = new Tuple<int, int>(indices[k], indices[k + 1]);
            }

            return pairs;
        }

        public static bool IsDataType(ETokenType inType)
        {
            return inType == ETokenType.Word || inType == ETokenType.Float || 
                inType == ETokenType.Int || inType == ETokenType.UInt ||
                inType == ETokenType.True || inType == ETokenType.False;
        }

        public static bool IsChangeKeyPrefix(ETokenType inType)
        {
            return inType == ETokenType.AddKey;// || inType == ETokenType.OverrideKey;
        }

        public static ETokenType GetTokenType(string string_value)
        {
            return GetTokenType(string_value.Length, 0, string_value).Item1;
        }

        public static Tuple<ETokenType, string> GetTokenType(int curr_pos, int world_start_pos, string line)
        {
            int len = curr_pos - world_start_pos;
            string word = line.Substring(world_start_pos, len);

            bool only_digit = true;
            int point_count = 0;
            bool minus_was = false;
            for (int i = 0; i < word.Length && only_digit; ++i)
            {
                char c = word[i];

                bool point = c == '.';
                point_count += point ? 1 : 0;

                bool minus = c == '-' && i == 0;
                if (minus)
                    minus_was = true;

                only_digit = (minus || point || char.IsDigit(c)) && point_count < 2;
            }

            ETokenType tt = ETokenType.Word;
            if (only_digit)
            {
                if (point_count > 0)
                    tt = ETokenType.Float;
                else if (word.Length <= 20)
                {
                    //long:  -9223372036854775808   to 9223372036854775807
                    //ulong: 0                      to 18446744073709551615
                    if (!minus_was && word.Length == 20)
                        tt = ETokenType.UInt;
                    else
                        tt = ETokenType.Int;
                }
            }
            return new Tuple<ETokenType, string>(tt, word);
        }
    }
}

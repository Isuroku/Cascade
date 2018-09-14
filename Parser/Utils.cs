using System;
using System.Collections.Generic;

namespace Parser
{
    public static class Utils
    {
        public static Tuple<int, int>[] GetStringPairs(string line, int line_number, CLoger inLoger)
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
    }
}

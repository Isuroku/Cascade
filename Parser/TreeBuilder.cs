using System;
using System.Collections.Generic;

namespace Parser
{
    public static class CTreeBuilder
    {
        enum EMultiArrayType { NotMultiArray, List, MultiArray }

        class CBuildObjects
        {

        }

        public static CKey Build(List<CTokenLine> inLines, CLoger inLoger)
        {
            var root = new CKey();
            Collect(root, inLines, 0, inLoger);
            root.CheckOnOneArray(inLoger);

            if(root.ElementCount == 1 && root[0].GetElementType() == EElementType.Key)
            {
                root = root[0] as CKey;
                root.SetParent(null);
            }

            return root;
        }

        static int Collect(CKey inParent, List<CTokenLine> inLines, int inStartIndex, CLoger inLoger)
        {
            CArrayKey ar_key = null;
            CKey last_key = null;

            int curr_rank = inParent.Rank + 1;

            int i = inStartIndex;
            while (i < inLines.Count)
            {
                int t = i;
                CTokenLine line = inLines[i];
                if (line.Rank < curr_rank)
                {
                    CheckEmptyKey(last_key, inLoger);
                    return i;
                }
                else if (line.Rank > curr_rank)
                {
                    if (last_key == null)
                    {
                        inLoger.LogError(EErrorCode.TooDeepRank, line);

                        Tuple<CArrayKey, CKey> res = AddLine(inParent, ar_key, line, inLoger);
                        ar_key = res.Item1;
                        last_key = res.Item2;
                    }
                    else
                    {
                        i = Collect(last_key, inLines, i, inLoger);
                        last_key.CheckOnOneArray(inLoger);
                    }
                }
                else
                {
                    CheckEmptyKey(last_key, inLoger);

                    Tuple<CArrayKey, CKey> res = AddLine(inParent, ar_key, line, inLoger);
                    ar_key = res.Item1;
                    last_key = res.Item2;
                }

                if (t == i)
                    i++;
            }
            return i;
        }

        static void CheckEmptyKey(CBaseKey key, CLoger inLoger)
        {
            if (key != null && key.ElementCount == 0)
                inLoger.LogError(EErrorCode.HeadWithoutValues, key);
        }

        static Tuple<CArrayKey, CKey> AddLine(CBaseKey inParent, CArrayKey arr_key, CTokenLine line, CLoger inLoger)
        {
            CKey key = null;
            CArrayKey res_arr_key = null;
            if (line.IsEmpty())
            {
                res_arr_key = arr_key;
            }
            else if (line.IsRecordDivider())
            {
                int index = 0;
                if (arr_key != null)
                    index = arr_key.Index + 1;
                else
                    inLoger.LogError(EErrorCode.RecordBeforeRecordDividerDoesntPresent, line);
                res_arr_key = new CArrayKey(inParent, line.Position, index);
            }
            else if (line.IsCommandLine())
            {
                if (arr_key == null)
                    arr_key = new CArrayKey(inParent, line.Position, 0);
                res_arr_key = arr_key;

                if (line.Command == ECommands.Name)
                {
                    if (line.CommandParams.Length < 1)
                        inLoger.LogError(EErrorCode.EmptyCommand, line);
                    else
                        arr_key.SetName(line.CommandParams[0]);
                }
            }
            else if (line.Head != null)
            {
                if (arr_key == null)
                    arr_key = new CArrayKey(inParent, line.Position, 0);
                res_arr_key = arr_key;

                key = new CKey(arr_key, line, inLoger);
            }
            else if(!line.IsTailEmpty)
            {
                if (line.TailLength > 1)
                {
                    int index = 0;
                    if (arr_key != null)
                        index = arr_key.Index + 1;
                    res_arr_key = new CArrayKey(inParent, line.Position, index);
                }
                else
                {
                    if (arr_key == null)
                        arr_key = new CArrayKey(inParent, line.Position, 0);
                    res_arr_key = arr_key;
                }

                res_arr_key.AddTokenTail(line, inLoger);
            }

            return new Tuple<CArrayKey, CKey>(res_arr_key, key);
        }
    }
}


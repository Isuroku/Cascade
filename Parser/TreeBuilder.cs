using System;
using System.Collections.Generic;

namespace Parser
{
    public interface ITreeBuildSupport
    {
        void LogWarning(EErrorCode inErrorCode, CToken inToken);
        void LogError(EErrorCode inErrorCode, string inText, int inLineNumber);
        void LogError(EErrorCode inErrorCode, CBaseKey inKey);
        void LogError(EErrorCode inErrorCode, CToken inToken);
        void LogError(EErrorCode inErrorCode, CTokenLine inLine);
        void LogInternalError(EInternalErrorCode inErrorCode, string inDebugText);
        void Trace(string inText);

        CKey GetTree(string inFileName);
    }

    public static class CTreeBuilder
    {
        enum EMultiArrayType { NotMultiArray, List, MultiArray }

        class CBuildObjects
        {

        }

        public static CKey Build(List<CTokenLine> inLines, ITreeBuildSupport inSupport)
        {
            var root = new CKey();
            Collect(root, -1, inLines, 0, inSupport);
            root.CheckOnOneArray(inSupport);

            if(root.ElementCount == 1 && root[0].GetElementType() == EElementType.Key)
            {
                root = root[0] as CKey;
                root.SetParent(null);
            }

            return root;
        }

        static int Collect(CKey inParent, int inParentRank, List<CTokenLine> inLines, int inStartIndex, ITreeBuildSupport inSupport)
        {
            CArrayKey ar_key = null;
            CKey last_key = null;

            int curr_rank = inParentRank + 1;

            int i = inStartIndex;
            while (i < inLines.Count)
            {
                int t = i;
                CTokenLine line = inLines[i];
                if (line.Rank < curr_rank)
                {
                    CheckEmptyKey(last_key, inSupport);
                    return i;
                }
                else if (line.Rank > curr_rank)
                {
                    if (last_key == null)
                    {
                        inSupport.LogError(EErrorCode.TooDeepRank, line);

                        Tuple<CArrayKey, CKey> res = AddLine(inParent, ar_key, line, inSupport);
                        ar_key = res.Item1;
                        last_key = res.Item2;
                    }
                    else
                    {
                        i = Collect(last_key, curr_rank, inLines, i, inSupport);
                        last_key.CheckOnOneArray(inSupport);
                    }
                }
                else
                {
                    CheckEmptyKey(last_key, inSupport);

                    Tuple<CArrayKey, CKey> res = AddLine(inParent, ar_key, line, inSupport);
                    ar_key = res.Item1;
                    last_key = res.Item2;
                }

                if (t == i)
                    i++;
            }
            return i;
        }

        static void CheckEmptyKey(CBaseKey key, ITreeBuildSupport inSupport)
        {
            if (key != null && key.ElementCount == 0)
                inSupport.LogError(EErrorCode.HeadWithoutValues, key);
        }

        static Tuple<CArrayKey, CKey> AddLine(CBaseKey inParent, CArrayKey arr_key, CTokenLine line, ITreeBuildSupport inSupport)
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
                    inSupport.LogError(EErrorCode.RecordBeforeRecordDividerDoesntPresent, line);
                res_arr_key = new CArrayKey(inParent, line.Position, index);
            }
            else if (line.IsCommandLine())
            {
                if (arr_key == null)
                    arr_key = new CArrayKey(inParent, line.Position, 0);
                res_arr_key = arr_key;

                ExecuteCommand(arr_key, line, inSupport);
            }
            else if (line.Head != null)
            {
                if (arr_key == null)
                    arr_key = new CArrayKey(inParent, line.Position, 0);
                res_arr_key = arr_key;

                key = new CKey(arr_key, line, inSupport);
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

                res_arr_key.AddTokenTail(line, inSupport);
            }

            return new Tuple<CArrayKey, CKey>(res_arr_key, key);
        }

        static void ExecuteCommand(CArrayKey arr_key, CTokenLine line, ITreeBuildSupport inSupport)
        {
            if (line.Command == ECommands.Name)
            {
                if (line.CommandParams.Length < 1)
                    inSupport.LogError(EErrorCode.EmptyCommand, line);
                else
                    arr_key.SetName(line.CommandParams[0]);
            }
            else if(line.Command == ECommands.Insert || line.Command == ECommands.Inherit)
            {
                SCommandParams prms = GetFileAndKeys(line, inSupport);

                if(!string.IsNullOrEmpty(prms.key_path))
                {
                    string[] path = prms.key_path.Split(new char[] { '\\', '/' });

                    CBaseKey root = null;
                    if (!string.IsNullOrEmpty(prms.file_name))
                        root = inSupport.GetTree(prms.file_name);
                    else
                        root = arr_key.GetRoot();

                    if(root == null)
                    {
                        inSupport.LogError(EErrorCode.CantFindRootInFile, line);
                        return;
                    }

                    CBaseKey key = root.FindKey(path);
                    if (key == null)
                    {
                        inSupport.LogError(EErrorCode.CantFindKey, line);
                        return;
                    }

                    if (line.Command == ECommands.Insert)
                    {
                        CBaseKey copy_key = key.GetCopy() as CBaseKey;
                        if (!prms.insert_only_elements)
                            copy_key.SetParent(arr_key);
                        else
                            arr_key.TakeAllElements(copy_key, false);
                    }
                }
                else
                    inSupport.LogError(EErrorCode.LocalPathEmpty, line);
            }
        }

        struct SCommandParams
        {
            public string file_name;
            public string key_path;
            public bool insert_only_elements;
        }

        static SCommandParams GetFileAndKeys(CTokenLine line, ITreeBuildSupport inSupport)
        {
            if (line.CommandParams.Length < 1)
            {
                inSupport.LogError(EErrorCode.EmptyCommand, line);
                return new SCommandParams
                {
                    file_name = string.Empty,
                    key_path = string.Empty,
                    insert_only_elements = false,
                };
            }

            string fn = string.Empty;
            string kp = string.Empty;
            string[] pathes = line.CommandParams[0].Split(new char[] { ':' });
            if (pathes.Length > 1)
            {
                fn = pathes[0];
                kp = pathes[1];
            }
            else
                kp = pathes[0];

            bool oe = line.CommandParams.Length > 1;

            return new SCommandParams
            {
                file_name = fn,
                key_path = kp,
                insert_only_elements = oe,
            };
        }
    }
}


using System.Collections.Generic;

namespace CascadeParser
{
    internal interface ITreeBuildSupport
    {
        IKey GetTree(string inFileName);
        ILogger GetLogger();
    }

    internal static class CTreeBuilder
    {
        enum EMultiArrayType { NotMultiArray, List, MultiArray }

        public static CKey Build(List<CTokenLine> inLines, ITreeBuildSupport inSupport)
        {
            var root = CKey.CreateRoot();

            CBuildCommands commands = new CBuildCommands(inSupport.GetLogger());

            SCollectResult collect_res = Collect(root, -1, inLines, 0, inSupport, commands);
            if(!collect_res.WasRecordDivider)
                root.CheckOnOneArray();

            return root;
        }

        struct SAddLineResult
        {
            public CKey current_array_key;
            public CKey last_record_key;
            public EKeyAddingMode last_key_add_mode;
            public bool WasRecordDivider;
        }

        struct SCollectResult
        {
            public int CurrentLineIndex;
            public bool WasRecordDivider;
        }

        static SCollectResult Collect(CKey inParent, int inParentRank, List<CTokenLine> inLines, int inStartIndex, ITreeBuildSupport inSupport, CBuildCommands inCommands)
        {
            SAddLineResult current_state = new SAddLineResult();

            int curr_rank = inParentRank + 1;

            bool rec_divider_was = false;

            int i = inStartIndex;
            while (i < inLines.Count)
            {
                int t = i;
                CTokenLine line = inLines[i];
                if (!line.IsEmpty() || line.Comments != null)
                {
                    if (line.Rank < curr_rank)
                    {
                        OnClosingKey(current_state.last_record_key, current_state.last_key_add_mode, inSupport);
                        return new SCollectResult
                        {
                            CurrentLineIndex = i,
                            WasRecordDivider = rec_divider_was
                        };
                    }
                    else if (line.Rank > curr_rank)
                    {
                        if (current_state.last_record_key == null)
                        {
                            inSupport.GetLogger().LogError(EErrorCode.TooDeepRank, line);
                        }
                        else
                        {
                            SCollectResult collect_res = Collect(current_state.last_record_key, curr_rank, inLines, i, inSupport, inCommands);
                            if(!collect_res.WasRecordDivider)
                                current_state.last_record_key.CheckOnOneArray();
                            i = collect_res.CurrentLineIndex;
                        }
                    }
                    else
                    {
                        OnClosingKey(current_state.last_record_key, current_state.last_key_add_mode, inSupport);
                        current_state = AddLine(inParent, current_state.current_array_key, line, inSupport, inCommands);

                        if (current_state.WasRecordDivider)
                            rec_divider_was = true;
                    }
                }

                if (t == i)
                    i++;
            }

            //if(last_key != null)
            //    last_key.CheckOnOneArray(inSupport);
            OnClosingKey(current_state.last_record_key, current_state.last_key_add_mode, inSupport);

            return new SCollectResult
            {
                CurrentLineIndex = i,
                WasRecordDivider = rec_divider_was
            };
        }

        static void OnClosingKey(CKey key, EKeyAddingMode inKeyAddMode, ITreeBuildSupport inSupport)
        {
            if (key == null)
                return;

            if (key.IsEmpty)
            {
                inSupport.GetLogger().LogError(EErrorCode.HeadWithoutValues, key);
                return;
            }

            if (inKeyAddMode == EKeyAddingMode.AddUnique)
                return;

            CKey parent = key.Parent;
            if (parent == null)
            {
                inSupport.GetLogger().LogError(EErrorCode.KeyMustHaveParent, key);
                return;
            }

            key.SetParent(null);

            if (inKeyAddMode == EKeyAddingMode.Override)
            {
                var pathes = new List<List<string>>();
                key.GetTerminalPathes(pathes, new List<string>());

                for(int i = 0; i < pathes.Count; ++i)
                {
                    var path = pathes[i];
                    RemoveKeysByPath(parent, path);
                }

                inKeyAddMode = EKeyAddingMode.Add;
            }
            
            if (inKeyAddMode == EKeyAddingMode.Add)
            {
                CKey child_key = parent.FindChildKey(key.Name);
                if (child_key != null)
                    child_key.MergeKey(key);
                else
                    key.SetParent(parent);
            }
            
        }

        static CKey CreateNewArrayKey(CKey inParent, CTokenLine line, CBuildCommands inCommands)
        {
            var res_arr_key = CKey.CreateArrayKey(inParent, line.Position);
            if (inCommands.IsNextArrayKeyNamePresent)
                res_arr_key.SetName(inCommands.PopNextArrayKeyName(inParent));
            return res_arr_key;
        }

        static SAddLineResult AddLine(CKey inParent, CKey inCurrentArrayKey, CTokenLine line, ITreeBuildSupport inSupport, CBuildCommands inCommands)
        {
            SAddLineResult result = new SAddLineResult();

            if (line.IsEmpty())
            {
                if (line.Comments != null)
                    inCommands.SetNextLineComment(line.Comments.Text, line.Position.Line, inParent);
                result.current_array_key = inCurrentArrayKey;
            }
            else if (line.IsRecordDivider())
            {
                result.current_array_key = null;
                result.WasRecordDivider = true;
            }
            else if (line.IsCommandLine())
            {
                result.current_array_key = ExecuteCommand(inParent, inCurrentArrayKey, line, inSupport, inCommands);
            }
            else if (line.Head != null)
            {
                result.current_array_key = inCurrentArrayKey;
                if (result.current_array_key == null)
                    result.current_array_key = CreateNewArrayKey(inParent, line, inCommands);

                if (line.AdditionMode == EKeyAddingMode.AddUnique && result.current_array_key.IsKeyWithNamePresent(line.Head.Text))
                    inSupport.GetLogger().LogError(EErrorCode.ElementWithNameAlreadyPresent, line);

                result.last_key_add_mode = line.AdditionMode;
                result.last_record_key = CKey.Create(result.current_array_key, line, inSupport.GetLogger());

                if (inCommands.IsNextLineCommentPresent)
                    result.last_record_key.AddComments(inCommands.PopNextLineComments(inParent));
            }
            else if(!line.IsTailEmpty)
            {
                //if (!line.IsNewArrayLine && line.TailLength == 1 && !inParent.IsArray)
                //    inParent.AddTokenTail(line, true, inSupport.GetLogger());
                //else
                {
                    result.current_array_key = CreateNewArrayKey(inParent, line, inCommands);
                    result.current_array_key.AddTokenTail(line, false, inSupport.GetLogger());
                }
            }

            return result;
        }

        static CKey ExecuteCommand(CKey inParent, CKey inArrKey, CTokenLine line, ITreeBuildSupport inSupport, CBuildCommands inCommands)
        {
            CKey arr_key = inArrKey;

            if (line.Command == ECommands.Name)
            {
                if (line.CommandParams.Length < 1)
                    inSupport.GetLogger().LogError(EErrorCode.EmptyCommand, line);
                else
                    inCommands.SetNextArrayKeyName(line.CommandParams[0], line.Position.Line, inParent);
            }
            else if (line.Command == ECommands.Insert)
            {
                if (arr_key == null)
                    arr_key = CreateNewArrayKey(inParent, line, inCommands);

                ExecuteCommand_Insert(arr_key, line, inSupport);
            }
            else if (line.Command == ECommands.Delete)
            {
                if (arr_key != null)
                    ExecuteCommand_Delete(arr_key, line, inSupport);
            }
            return arr_key;
        }

        static void ExecuteCommand_Delete(CKey arr_key, CTokenLine line, ITreeBuildSupport inSupport)
        {
            if (line.CommandParams.Length == 0 || string.IsNullOrEmpty(line.CommandParams[0]))
            {
                inSupport.GetLogger().LogError(EErrorCode.PathEmpty, line);
                return;
            }

            string key_path = line.CommandParams[0];

            string[] path = key_path.Split(new char[] { '\\', '/' });

            if(!RemoveKeysByPath(arr_key, path))
                inSupport.GetLogger().LogError(EErrorCode.CantFindKey, line);
        }

        static bool RemoveKeysByPath(CKey inParent, IList<string> inPath)
        {
            CKey key = inParent.FindKey(inPath);
            if (key == null)
                return false;

            CKey parent = key.Parent;
            key.SetParent(null);
            while (parent != inParent && parent.IsEmpty)
            {
                CKey prev = parent;
                parent = parent.Parent;
                prev.SetParent(null);
            }
            return true;
        }

        static void ExecuteCommand_Insert(CKey arr_key, CTokenLine line, ITreeBuildSupport inSupport)
        {
            string file_name = line.CommandParams["file"];
            string key_path = line.CommandParams["key"];

            if (string.IsNullOrEmpty(file_name) && string.IsNullOrEmpty(key_path))
                inSupport.GetLogger().LogError(EErrorCode.PathEmpty, line);
            
            CKey root = null;
            if (!string.IsNullOrEmpty(file_name))
                root = (CKey)inSupport.GetTree(file_name);
            else
                root = arr_key.GetRoot();

            if (root == null)
            {
                inSupport.GetLogger().LogError(EErrorCode.CantFindRootInFile, line);
                return;
            }

            CKey key = root;
            if (!string.IsNullOrEmpty(key_path))
            {
                string[] path = key_path.Split(new char[] { '\\', '/' });

                key = root.FindKey(path);
                if (key == null)
                {
                    inSupport.GetLogger().LogError(EErrorCode.CantFindKey, line);
                    return;
                }
            }

            bool insert_parent = line.CommandParams.ContainsKey("parent");
            CKey copy_key = key.GetCopy() as CKey;
            if (insert_parent)
                copy_key.SetParent(arr_key);
            else
                arr_key.TakeAllElements(copy_key, false);
            arr_key.CheckOnOneArray();
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
                inSupport.GetLogger().LogError(EErrorCode.EmptyCommand, line);
                return new SCommandParams
                {
                    file_name = string.Empty,
                    key_path = string.Empty,
                    insert_only_elements = false,
                };
            }

            string fn = string.Empty;
            string kp = string.Empty;
            bool only_elements = false;

            if (line.CommandParams.Length > 2)
            {
                fn = line.CommandParams[0];
                kp = line.CommandParams[1];
                only_elements = true;
            }
            else if (line.CommandParams.Length == 2)
            {
                fn = line.CommandParams[0];
                kp = line.CommandParams[1];
            }
            else if (line.CommandParams.Length == 1)
            {
                kp = line.CommandParams[0];
            }

            return new SCommandParams
            {
                file_name = fn,
                key_path = kp,
                insert_only_elements = only_elements,
            };
        }
    }
}


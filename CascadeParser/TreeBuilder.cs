using System.Collections.Generic;

namespace CascadeParser
{
    internal static class CTreeBuilder
    {
        public static CKey Build(string inRootName, List<CTokenLine> inLines, ITreeBuildSupport inSupport)
        {
            var root = CKey.CreateRoot(inRootName);

            if (inLines.Count == 0)
                return root;

            CollectByDivs(root, -1, 0, inLines.Count, inLines, inSupport, root, EKeyAddingMode.AddUnique);

            if (root.KeyCount == 1 && root.GetKey(0).IsArrayKey() && root.GetKey(0).KeyCount == 0)
            {
                root.TakeAllElements(root.GetKey(0), false);
                root.GetKey(0).SetParent(null);
            }

            return root;
        }

        static Tuple<CKey, int> CreateKey(int inStartLine, int inEndLine, List<CTokenLine> inLines, ITreeBuildSupport inSupport, 
            CKey inRoot, CKey inParent, EKeyAddingMode inKeyAddingMode)
        {
            CTokenLine line = inLines[inStartLine];
            int key_rank = line.Rank;

            if (line.IsHeadEmpty)
            {
                CKey arr_key = CKey.CreateArrayKey(inParent, line.Position);
                arr_key.AddTokenTail(line, inSupport.GetLogger());
                return new Tuple<CKey, int>(arr_key, inStartLine + 1);
            }
            else
            {
                CKey key = null;
                if (inKeyAddingMode != EKeyAddingMode.AddUnique)
                {
                    key = inParent.FindChildKey(line.Head.Text);
                    if (key == null)
                    {
                        inSupport.GetLogger().LogError(EErrorCode.CantFindKey, line, $"Key name: {line.Head.Text}");
                    }
                    else
                    {
                        if (inKeyAddingMode == EKeyAddingMode.Override)
                        {
                            key.ClearComments();
                            key.ClearValues();
                        }

                        if (inKeyAddingMode == EKeyAddingMode.Override || inKeyAddingMode == EKeyAddingMode.Add)
                            key.AddTokenTail(line, inSupport.GetLogger());
                    }
                }
                else if (inParent.IsKeyWithNamePresent(line.Head.Text))
                    inSupport.GetLogger().LogError(EErrorCode.ElementWithNameAlreadyPresent, line);

                if (key == null)
                    key = CKey.Create(inParent, line, inSupport.GetLogger());

                int last_line = FindNextSameRankLine(inStartLine, inEndLine, key_rank, inLines);

                if (last_line > inStartLine + 1)
                {
                    EKeyAddingMode next_add_mode = inKeyAddingMode;
                    if (next_add_mode == EKeyAddingMode.Add)
                        next_add_mode = EKeyAddingMode.AddUnique;

                    CollectByDivs(key, key_rank, inStartLine + 1, last_line, inLines, inSupport, inRoot, next_add_mode);
                }

                if (key.KeyCount == 0 && key.ValuesCount == 0)
                    inSupport.GetLogger().LogError(EErrorCode.HeadWithoutValues, line);

                return new Tuple<CKey, int>(key, last_line);
            }
        }

        static int FindNextSameRankLine(int inStartLine, int inEndLine, int inRank, List<CTokenLine> inLines)
        {
            for(int i = inStartLine + 1; i < inEndLine; ++i)
            {
                if (!inLines[i].IsEmpty() && inLines[i].Rank == inRank)
                    return i;
            }
            return inEndLine;
        }

        //inStartLine - next of parent line
        static void CollectByDivs(CKey inParent, int inParentRank, int inStartLine, int inEndLine, List<CTokenLine> inLines, 
            ITreeBuildSupport inSupport, CKey inRoot, EKeyAddingMode inKeyAddingMode)
        {
            int curr_rank = inParentRank + 1;
            List<Tuple<int, int>> recs_by_divs = new List<Tuple<int, int>>();
            CollectDividers(inStartLine, inEndLine, curr_rank, inLines, inSupport, recs_by_divs);

            if (recs_by_divs.Count > 1)
            {
                if (inKeyAddingMode == EKeyAddingMode.Override)
                    inParent.ClearAllArrayKeys();

                for (int i = 0; i < recs_by_divs.Count; i++)
                {
                    int first_line = recs_by_divs[i].Item1;
                    int exlude_last_line = recs_by_divs[i].Item2;
                    if (first_line < exlude_last_line)
                    {
                        CKey arr_key = CKey.CreateArrayKey(inParent, inLines[first_line].Position);

                        if(IsLinePresent(curr_rank, first_line, exlude_last_line, inLines))
                            Collect(arr_key, curr_rank, first_line, exlude_last_line, inLines, inSupport, inRoot, EKeyAddingMode.AddUnique);
                        else
                            CollectByDivs(arr_key, curr_rank, first_line, exlude_last_line, inLines, inSupport, inRoot, EKeyAddingMode.AddUnique);

                        if (arr_key.IsEmpty)
                            arr_key.SetParent(null);
                    }
                }
            }
            else
                Collect(inParent, inParentRank, recs_by_divs[0].Item1, recs_by_divs[0].Item2, inLines, inSupport, inRoot, inKeyAddingMode);
        }

        static bool IsLinePresent(int inRank, int inStartLine, int inEndLine, List<CTokenLine> inLines)
        {
            for (int i = inStartLine; i < inEndLine; ++i)
            {
                if (!inLines[i].IsEmpty() && inLines[i].Rank == inRank)
                    return true;
            }
            return false;
        }

        //inStartLine - next of parent line
        static void Collect(CKey inParent, int inParentRank, int inStartLine, int inEndLine, List<CTokenLine> inLines, ITreeBuildSupport inSupport, 
            CKey inRoot, EKeyAddingMode inKeyAddingMode)
        {
            CBuildCommands command_for_next_string = null;
            int start = inStartLine;
            do
            {
                int old_start = start;
                CTokenLine line = inLines[start];
                if (line.IsCommandLine())
                {
                    if (line.Command == ECommands.Name)
                    {
                        if (line.CommandParams.Length < 1)
                            inSupport.GetLogger().LogError(EErrorCode.EmptyCommand, line);
                        else
                        {
                            if (inParent.IsArray && line.Rank == inParentRank)
                            {
                                if(!inParent.SetName(line.CommandParams[0]))
                                    inSupport.GetLogger().LogError(EErrorCode.DublicateKeyName, line);
                            }
                            else
                            {
                                //inSupport.GetLogger().LogError(EErrorCode.NextArrayKeyNameMissParent, line);
                                if (command_for_next_string == null)
                                    command_for_next_string = new CBuildCommands(inSupport.GetLogger());
                                command_for_next_string.SetNextArrayKeyName(line.CommandParams[0], line.Position.Line, inParent);
                            }
                        }
                    }
                    else if (line.Command == ECommands.Insert)
                        ExecuteCommand_Insert(inParent, line, inSupport, inRoot);
                    else if (line.Command == ECommands.Delete)
                        ExecuteCommand_Delete(inParent, line, inSupport);
                    else if (line.Command == ECommands.ChangeValue)
                        inParent.ChangeValues(line.CommandParams.GetDictionary());
                }
                else if (line.IsEmpty() && line.Comments != null)
                {
                    if(command_for_next_string == null)
                        command_for_next_string = new CBuildCommands(inSupport.GetLogger());
                    command_for_next_string.SetNextLineComment(line.Comments.Text, line.Position.Line, inParent);
                }
                else if (!line.IsEmpty())
                {
                    EKeyAddingMode adding_mode = inKeyAddingMode;
                    if (adding_mode == EKeyAddingMode.AddUnique && line.AdditionMode != EKeyAddingMode.AddUnique)
                        adding_mode = line.AdditionMode;

                    Tuple<CKey, int> new_key_new_line = CreateKey(start, inEndLine, inLines, inSupport, inRoot, inParent, adding_mode);

                    CKey new_key = new_key_new_line.Item1;

                    if (command_for_next_string != null && command_for_next_string.IsNextArrayKeyNamePresent)
                    { 
                        if(!new_key.SetName(command_for_next_string.PopNextArrayKeyName(inParent)))
                            inSupport.GetLogger().LogError(EErrorCode.DublicateKeyName, line);
                    }

                    if (command_for_next_string != null && command_for_next_string.IsNextLineCommentPresent)
                        new_key.AddComments(command_for_next_string.PopNextLineComments(inParent));

                    //if (!new_key.IsArray && line.AdditionMode == EKeyAddingMode.AddUnique && inParent.IsKeyWithNamePresent(new_key.Name))
                    //    inSupport.GetLogger().LogError(EErrorCode.ElementWithNameAlreadyPresent, inLines[start]);
                    
                    //if(line.AdditionMode == EKeyAddingMode.AddUnique)
                    //    new_key.SetParent(inParent);
                    //else if (line.AdditionMode == EKeyAddingMode.Override || line.AdditionMode == EKeyAddingMode.Add)
                    //{
                    //    if (line.AdditionMode == EKeyAddingMode.Override)
                    //    {
                    //        var pathes = new List<List<string>>();
                    //        new_key.GetTerminalPathes(pathes, new List<string>());

                    //        //for correct deleting array elems
                    //        for (int i = pathes.Count - 1; i >= 0; --i)
                    //        {
                    //            var path = pathes[i];
                    //            RemoveKeysByPath(inParent, path);
                    //        }
                    //    }

                    //    CKey child_key = inParent.FindChildKey(new_key.Name);
                    //    if (child_key != null)
                    //        child_key.MergeKey(new_key);
                    //    else
                    //        new_key.SetParent(inParent);
                    //}

                    start = new_key_new_line.Item2;
                }

                if (old_start == start)
                    start++;
            }
            while (start < inEndLine);
        }

        static void CollectDividers(int inStartLine, int inEndLine, int inNeedRank, List<CTokenLine> inLines, ITreeBuildSupport inSupport, List<Tuple<int, int>> outList)
        {
            int start = inStartLine;
            int end;
            do {
                end = FindDivider(start, inEndLine, inNeedRank, inLines);
                outList.Add(new Tuple<int, int>(start, end));
                start = end + 1;
            }
            while (start < inEndLine);
        }

        static int FindDivider(int inStartLine, int inEndLine, int inRank, List<CTokenLine> inLines)
        {
            for (int i = inStartLine; i < inEndLine; ++i)
            {
                if (inLines[i].IsRecordDivider() && inLines[i].Rank == inRank)
                    return i;
            }
            return inEndLine;
        }

        static void ExecuteCommand_Delete(CKey inParent, CTokenLine line, ITreeBuildSupport inSupport)
        {
            if (line.CommandParams.Length == 0 || string.IsNullOrEmpty(line.CommandParams[0]))
            {
                inSupport.GetLogger().LogError(EErrorCode.PathEmpty, line);
                return;
            }

            string key_path = line.CommandParams[0];

            CKey start_key = inParent;
            int i = 0;
            while(i < key_path.Length && key_path[i] == '<' && start_key.Parent != null)
            {
                start_key = start_key.Parent;
                i++;
            }
            key_path = key_path.Substring(i);

            string[] path = key_path.Split(new char[] { '\\', '/' });

            if (!RemoveKeysByPath(start_key, path))
                inSupport.GetLogger().LogError(EErrorCode.CantFindKey, line, $"Start key: {inParent.GetPath()}");
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

        static void ExecuteCommand_Insert(CKey inParent, CTokenLine line, ITreeBuildSupport inSupport, CKey inRoot)
        {
            string file_name = line.CommandParams["file"];
            string key_path = line.CommandParams["key"];

            if (string.IsNullOrEmpty(file_name) && string.IsNullOrEmpty(key_path))
                inSupport.GetLogger().LogError(EErrorCode.PathEmpty, line);

            CKey root = inRoot;
            if (!string.IsNullOrEmpty(file_name))
                root = (CKey)inSupport.GetTree(file_name);
            //else
              //  root = inParent.GetRoot();

            if (root == null)
            {
                inSupport.GetLogger().LogError(EErrorCode.CantFindInsertFile, line);
                return;
            }

            if (root.KeyCount == 1 && root.GetKey(0).IsArrayKey())
                root = root.GetKey(0);

            CKey key = root;
            if (!string.IsNullOrEmpty(key_path))
            {
                key = (CKey)root.FindKey(key_path);
                if (key == null)
                {
                    inSupport.GetLogger().LogError(EErrorCode.CantFindKey, line);
                    return;
                }
            }

            CKey destination_key = inParent;
            bool insert_in_parent = line.CommandParams.ContainsKey("insert_in_parent");
            if (insert_in_parent)
            {
                if(inParent.Parent != null)
                    destination_key = inParent.Parent;
                else
                    inSupport.GetLogger().LogError(EErrorCode.KeyMustHaveParent, line);
            }

            bool add_key = line.CommandParams.ContainsKey("add_key");
            CKey copy_key = key.GetCopy() as CKey;
            if (add_key)
                copy_key.SetParent(destination_key);
            else
                destination_key.TakeAllElements(copy_key, false);
            destination_key.CheckOnOneArray();
        }
    }
}

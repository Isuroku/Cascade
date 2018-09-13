using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class CTreeBuilder
    {
        CLoger _loger;

        CKey _root;
        public CKey Root { get { return _root; } }

        public CTreeBuilder(CLoger inLoger)
        {
            _loger = inLoger;
        }

        public void Build(List<CTokenLine> inLines)
        {
            _root = new CKey();
            DivideByRecords(_root, inLines, 0, inLines.Count, 0);
        }

        private void DivideByRecords(CKey inParent, List<CTokenLine> inLines, int inStartLine, int BorderLine, int inCurrRank)
        {
            int index = -1;
            while(inStartLine < BorderLine)
            {
                int rec_divider_line_number = FindRecDiv(inLines, inStartLine, BorderLine);

                CBaseKey parent = inParent;
                if (rec_divider_line_number < BorderLine || index >= 0)
                {
                    index++;
                    parent = new CArrayKey(inParent, new SPosition(inStartLine, 0), index);
                }

                BuildRecord(parent, inLines, inStartLine, rec_divider_line_number, inCurrRank);
                inStartLine = rec_divider_line_number + 1;
            }
        }

        private void BuildRecord(CBaseKey inParent, List<CTokenLine> inLines, int inStartLine, int BorderLine, int inCurrRank)
        {
            _loger.Trace(string.Format("{0} - {1}", inStartLine, BorderLine));

            int i = inStartLine;
            
            while(i < BorderLine)
            {
                CTokenLine curr_line = inLines[i];

                if(curr_line.Rank != inCurrRank)
                {
                    _loger.LogError(EErrorCode.UnwaitedRank, curr_line);
                }

                int ti = i;

                if (curr_line.IsEmpty())
                {
                }
                else if (curr_line.IsCommandLine())
                {
                }
                else if (curr_line.Head != null)
                { 
                    if (!curr_line.IsTailEmpty)
                    {
                        CKey key = new CKey(inParent, curr_line, _loger);
                    }
                    else
                    {
                        CKey key = new CKey(inParent, curr_line.Head, _loger);

                        int next_i = i + 1;
                        int next_eq_line = FindRecEnd(curr_line.Rank, inLines, next_i, BorderLine);
                        bool new_rec = next_eq_line > next_i;
                        if (new_rec)
                        {
                            EMultiArrayType is_array = CheckMultilineArray(inLines, next_i, next_eq_line);
                            if (is_array == EMultiArrayType.NotMultiArray)
                                DivideByRecords(key, inLines, next_i, next_eq_line, inCurrRank + 1);
                            else if (is_array == EMultiArrayType.MultiArray)
                                AddMultilineArray(key, inLines, next_i, next_eq_line);
                            else if (is_array == EMultiArrayType.List)
                                AddMultilineList(key, inLines, next_i, next_eq_line);
                        }
                        else
                        {
                            var v = new CBoolValue(key, curr_line.Head.Position, true);
                            _loger.LogWarning(EErrorCode.HeadWithoutValues, curr_line.Head);
                        }

                        i = next_eq_line;
                    }
                }
                else if (!curr_line.IsTailEmpty)
                {
                    if(curr_line.TailLength == 1)
                    {
                        CKey key = new CKey(inParent, curr_line.Tail[0], _loger);
                        var v = new CBoolValue(key, curr_line.Tail[0].Position, true);
                    }
                    else
                        _loger.LogError(EErrorCode.UndefinedLine, curr_line);
                }

                if (ti == i)
                    i++;
            }
        }

        int FindRecDiv(List<CTokenLine> inLines, int inStartLine, int BorderLine)
        {
            int curr_rank = inLines[inStartLine].Rank;
            for(int i = inStartLine; i < BorderLine && inLines[i].Rank <= curr_rank; ++i)
            {
                if (inLines[i].IsRecordDivider())
                    return i;
            }
            return BorderLine;
        }

        int FindRecEnd(int rank, List<CTokenLine> inLines, int inStartLine, int BorderLine)
        {
            int i = inStartLine;
            while (i < BorderLine && inLines[i].Rank > rank)
                ++i;
            return i;
        }

        enum EMultiArrayType { NotMultiArray, List, MultiArray }
        EMultiArrayType CheckMultilineArray(List<CTokenLine> inLines, int inStartLine, int BorderLine)
        {
            int curr_rank = inLines[inStartLine].Rank;
            bool lst = true;
            for (int i = inStartLine; i < BorderLine; ++i)
            {
                CTokenLine curr_line = inLines[i];
                if (curr_line.Rank != curr_rank || curr_line.Head != null)
                    return EMultiArrayType.NotMultiArray;

                if (lst && curr_line.TailLength > 1)
                    lst = false;
            }
            return lst ? EMultiArrayType.List: EMultiArrayType.MultiArray;
        }

        void AddMultilineArray(CBaseKey inParent, List<CTokenLine> inLines, int inStartLine, int BorderLine)
        {
            for (int i = inStartLine; i < BorderLine; ++i)
            {
                CTokenLine curr_line = inLines[i];
                CArrayKey key = new CArrayKey(inParent, curr_line.Position, i - inStartLine);
                key.AddTokenTail(curr_line, _loger);
            }
        }

        void AddMultilineList(CBaseKey inParent, List<CTokenLine> inLines, int inStartLine, int BorderLine)
        {
            for (int i = inStartLine; i < BorderLine; ++i)
            {
                CTokenLine curr_line = inLines[i];
                if(!curr_line.IsEmpty())
                    inParent.AddTokenTail(curr_line, _loger);
            }
        }
    }
}


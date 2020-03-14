using System.Collections.Generic;
using System.Text;

namespace CascadeParser
{
    class CBuildCommands
    {
        ILogger _logger;

        public CBuildCommands(ILogger logger)
        {
            _logger = logger;
        }

        //#region WriteComments
        //List<Tuple<int, string>> _add_comment = new List<Tuple<int, string>>();
        //public void AddComment(int line, string name) { _add_comment.Add(new Tuple<int, string>(line, name)); }

        //internal void WriteComments(CKey root)
        //{
        //    foreach (var t in _add_comment)
        //    {
        //        CKey fk = root.FindLowerNearestKey(t.Item1).Item1;
        //        if (fk == null)
        //            _logger.LogError(EErrorCode.CantAddComment, t.Item2, t.Item1);
        //        else
        //            fk.AddComments(t.Item2);
        //    }

        //    _add_comment.Clear();
        //}
        //#endregion Comments

        class CMemoryString
        {
            public string Value { get; private set; }
            public CKey Parent { get; private set; }

            int _line_number;
            EErrorCode _error_code;

            public CMemoryString(string value, CKey parent, int line_number, EErrorCode error_code)
            {
                Value = value;
                Parent = parent;
                _line_number = line_number;
                _error_code = error_code;
            }

            public bool CheckParent(CKey inParent, ILogger logger)
            {
                if (Parent == inParent)
                    return true;

                logger.LogError(_error_code,
                    string.Format("Name {0}. Setted inside {1}. Try to use inside {2}",
                    Value,
                    Parent.Name,
                    inParent.Name),
                    _line_number);

                return false;
            }
        }

        CMemoryString _next_array_key_name;

        public bool IsNextArrayKeyNamePresent { get { return _next_array_key_name != null; } }

        public void SetNextArrayKeyName(string inName, int inLineNumber, CKey inParent)
        {
            if (IsNextArrayKeyNamePresent)
                _logger.LogError(EErrorCode.NextArrayKeyNameAlreadySetted, _next_array_key_name.Value, inLineNumber);
            else
                _next_array_key_name = new CMemoryString(inName, inParent, inLineNumber, EErrorCode.NextArrayKeyNameMissParent);
        }

        public string PopNextArrayKeyName(CKey inParent)
        {
            if (!_next_array_key_name.CheckParent(inParent, _logger))
            {
                _next_array_key_name = null;
                return string.Empty;
            }

            string t = _next_array_key_name.Value;
            _next_array_key_name = null;
            return t;
        }

        List<CMemoryString> _next_line_comments = new List<CMemoryString>();

        public bool IsNextLineCommentPresent { get { return _next_line_comments.Count > 0; } }

        public void SetNextLineComment(string inName, int inLineNumber, CKey inParent)
        {
            _next_line_comments.Add(new CMemoryString(inName, inParent, inLineNumber, EErrorCode.NextLineCommentMissParent));
        }

        public string PopNextLineComments(CKey inParent)
        {
            StringBuilder sb = new StringBuilder();

            for(int i = 0; i < _next_line_comments.Count; ++i)
            {
                if (_next_line_comments[i].CheckParent(inParent, _logger))
                {
                    if(i != 0)
                        sb.AppendLine();
                    sb.Append(_next_line_comments[i].Value);
                }
            }

            _next_line_comments.Clear();
            return sb.ToString();
        }
    }
}

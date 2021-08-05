
namespace CascadeParser
{
    internal abstract class CBaseElement
    {
        protected CKey _parent;
        protected SPosition _pos;

        private string _comments;
        public string Comments { get { return _comments; } }

        public virtual bool IsKey() { return false; }

        public SPosition Position { get { return _pos; } }
        public CKey Parent { get { return _parent; } }

        public CBaseElement() { }
        public CBaseElement(CKey parent, SPosition pos)
        {
            _parent = parent;
            if (_parent != null)
                _parent.AddChild(this);
            _pos = pos;
        }

        public CBaseElement(CBaseElement other)
        {
            _pos = other._pos;
            _comments = other._comments;
        }

        public abstract CBaseElement GetCopy();

        public void SetParent(CKey parent)
        {
            if (_parent != null)
                _parent.RemoveChild(this);

            _parent = parent;
            if (_parent != null)
                _parent.AddChild(this);
        }

        public abstract string GetStringForSave();

        public void AddComments(string text)
        {
            if (string.IsNullOrEmpty(_comments))
                _comments = text;
            else
                _comments += string.Format(" {0}", text);
        }

        public void ClearComments()
        {
            _comments = string.Empty;
        }
    }

}

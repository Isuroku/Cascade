
namespace Parser
{
    public struct SPosition
    {
        public int Line { get; private set; }
        public int Col { get; private set; }
        public SPosition(int inLine, int inCol) { Line = inLine; Col = inCol; }
        public override string ToString() { return string.Format("Ln: {0} Col: {1}", Line, Col); }
    }
}

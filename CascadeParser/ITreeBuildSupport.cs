
namespace CascadeParser
{
    internal interface ITreeBuildSupport
    {
        IKey GetTree(string inFileName);
        ILogger GetLogger();
    }
}

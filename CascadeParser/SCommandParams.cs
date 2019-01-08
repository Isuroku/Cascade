
namespace CascadeParser
{
    //struct SCommandParams
    //{
    //    public string file_name;
    //    public string key_path;
    //    public bool insert_only_elements;

    //    static SCommandParams GetFileAndKeys(CTokenLine line, ITreeBuildSupport inSupport)
    //    {
    //        if (line.CommandParams.Length < 1)
    //        {
    //            inSupport.GetLogger().LogError(EErrorCode.EmptyCommand, line);
    //            return new SCommandParams
    //            {
    //                file_name = string.Empty,
    //                key_path = string.Empty,
    //                insert_only_elements = false,
    //            };
    //        }

    //        string fn = string.Empty;
    //        string kp = string.Empty;
    //        bool only_elements = false;

    //        if (line.CommandParams.Length > 2)
    //        {
    //            fn = line.CommandParams[0];
    //            kp = line.CommandParams[1];
    //            only_elements = true;
    //        }
    //        else if (line.CommandParams.Length == 2)
    //        {
    //            fn = line.CommandParams[0];
    //            kp = line.CommandParams[1];
    //        }
    //        else if (line.CommandParams.Length == 1)
    //        {
    //            kp = line.CommandParams[0];
    //        }

    //        return new SCommandParams
    //        {
    //            file_name = fn,
    //            key_path = kp,
    //            insert_only_elements = only_elements,
    //        };
    //    }
    //}
}

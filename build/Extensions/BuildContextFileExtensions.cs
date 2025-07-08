using Cake.Common.IO;
using Cake.Core.IO;

namespace Build.Extensions;

public static partial class BuildContextExtensions
{
    public static void DeleteFileIfExists(this BuildContext context, FilePath path)
    {
        if (context.FileExists(path))
        {
            context.DeleteFile(path);
        }
    }
}

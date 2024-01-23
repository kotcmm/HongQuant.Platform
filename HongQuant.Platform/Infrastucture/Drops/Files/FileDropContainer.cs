using System.IO;

namespace HongQuant.Platform.Infrastucture.Drops.Files
{
    public class FileDropContainer
    {
        public FileDropContainer(IEnumerable<string> files)
        {
            if (null == files)
            {
                Files = Array.Empty<string>();
                return;
            }

            Files = files
                .Where(x => null != x)
                .Select(Path.GetFileName)
                .Cast<string>()
                .ToArray();
        }

        public IEnumerable<string> Files { get; }
    }
}

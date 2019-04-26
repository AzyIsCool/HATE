using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace HATE
{
    class GetEmbeddedFile
    {
        private static Assembly ThisAssembly = typeof(GetEmbeddedFile).Assembly;
        private static string ThisAssemblyEntryPoint = ThisAssembly.GetName().Name;

        public static async Task<object> GetFile(string fileName, string fileExtension, string subDir = null)
        {
            object obj = null;
            var stream = await GetFileStream(fileName, fileExtension, subDir);
            if (stream != null)
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    obj = await reader.ReadToEndAsync();
                }
            }

            stream?.Dispose();
            return obj;
        }

        public static async Task<Stream> GetFileStream(string fileName, string fileExtension, string subDir = null)
        {
            string FilePath;
            if (string.IsNullOrWhiteSpace(subDir))
                FilePath = MakeResourceStreamString(fileName, fileExtension);
            else
                FilePath = MakeResourceStreamString(subDir, fileName, fileExtension);

            return ThisAssembly.GetManifestResourceStream(FilePath);
        }

        private static string MakeResourceStreamString(params string[] fileStructure)
        {
            string returnString = ThisAssemblyEntryPoint;
            foreach (var s in fileStructure)
            {
                returnString += $".{s}";
            }

            return returnString.Replace("-", "_");
        }
    }
}

using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace HATE
{
    class GetEmbeddedFile<T>
    {
        public static async Task<T> GetFile(string fileName, string fileExtension, string subDir = null)
        {
            T result = default;
            var stream = await GetEmbeddedFile.GetFileStream(fileName, fileExtension, subDir);
            if (stream != null)
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var obj = (object)await reader.ReadToEndAsync();
                    result = (T)obj;
                }
            }

            stream?.Dispose();
            return result;
        }
    }
    class GetEmbeddedFile
    {
        private static Assembly ThisAssembly = typeof(GetEmbeddedFile).Assembly;
        private static string ThisAssemblyEntryPoint = ThisAssembly.GetName().Name;

        public static Task<Stream> GetFileStream(string fileName, string fileExtension, string subDir = null)
        {
            string FilePath;
            if (string.IsNullOrWhiteSpace(subDir))
                FilePath = MakeResourceStreamString(fileName, fileExtension);
            else
                FilePath = MakeResourceStreamString(subDir, fileName, fileExtension);

            return Task.FromResult(ThisAssembly.GetManifestResourceStream(FilePath));
        }

        private static string MakeResourceStreamString(params string[] fileStructure)
        {
            string returnString = ThisAssemblyEntryPoint;
            foreach (var s in fileStructure)
            {
                returnString += $".{s}";
            }

            return returnString;
        }
    }
}
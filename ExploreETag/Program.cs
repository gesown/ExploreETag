using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Threading;

namespace ExploreETag
{
    class Program
    {
        private static HttpResponseMessage response;
        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    Console.WriteLine("Usage: ExploreETag PathToLocalFile (include trailing \\) -- Must have SASUri.txt file in folder");
                    return;
                }
                var currntLocalFilePath = args[0];
                if (!currntLocalFilePath.EndsWith("\\"))
                {
                    Console.WriteLine("Local path must end with \\");
                    return;
                }
                var ETagInFile = args[0] + "EtagIn.txt";
                var ETagIn = File.Exists(ETagInFile) ? File.ReadAllText(ETagInFile) : string.Empty;
            var     sasUriFile = args[0] + "SasUri.txt";

               var  SaSFileIn = File.Exists(sasUriFile) ? File.ReadAllText(sasUriFile) : throw new Exception("SasUri.txt File Missing.");
                Uri sasUri = new Uri(SaSFileIn);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, sasUri);
                var sasUriFileName = sasUri.Segments.Last();
                var newLocalFilePath = currntLocalFilePath + sasUriFileName;
                response = GetHeadResponse(request).Result;
                var ETag = response.Headers.ETag.ToString();
                ETag = ETag.Substring(1, ETag.Length - 2); // returned value bounded by delimited "
                if (ETag == ETagIn)
                {
                    Console.WriteLine("Local and Remote Files are Identical");
                    return;
                }
                if (File.Exists(newLocalFilePath))
                {
                    newLocalFilePath = Path.GetDirectoryName(newLocalFilePath) + "\\" + Path.GetFileNameWithoutExtension(newLocalFilePath) + "_" + ETag +
                                       Path.GetExtension(newLocalFilePath);
                }
                FileStream writer = new FileStream(newLocalFilePath, FileMode.Create);
                var fileResponse = GetFileResponse(sasUri).Result;
                var frStream = fileResponse.Content.ReadAsByteArrayAsync().Result;
                writer.Write(frStream, 0, frStream.Length);

                File.WriteAllText(ETagInFile, ETag);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static async Task<HttpResponseMessage> GetFileResponse(Uri sasUrl)
        {
            HttpClient client = new HttpClient();
            return await client.GetAsync(sasUrl);
        }

        private static async Task<HttpResponseMessage> GetHeadResponse(HttpRequestMessage request)
        {
            HttpClient client = new HttpClient();
            return await client.GetAsync(request.RequestUri.ToString(), HttpCompletionOption.ResponseHeadersRead);
        }
    }
}
// https://msstoragedemo.file.core.windows.net/msdemofileshare/WisdomContexts.vsdx
// share url msdemofileshare sas https://msstoragedemo.file.core.windows.net/msdemofileshare?st=2019-04-19T07%3A33%3A26Z&se=2019-04-20T07%3A33%3A26Z&sp=rl&sv=2018-03-28&sr=s&sig=trpGvkOwGO4EYn%2BvPCdrWW%2Bz5xoMgowpadB6xfPQnuY%3D
// share query ?st=2019-04-19T07%3A33%3A26Z&se=2019-04-20T07%3A33%3A26Z&sp=rl&sv=2018-03-28&sr=s&sig=trpGvkOwGO4EYn%2BvPCdrWW%2Bz5xoMgowpadB6xfPQnuY%3D
// file sas url https://msstoragedemo.file.core.windows.net/msdemofileshare/WisdomContexts.vsdx?st=2019-04-19T07%3A34%3A47Z&se=2019-04-20T07%3A34%3A47Z&sp=rl&sv=2018-03-28&sr=f&sig=JWzuvz8p2LCnmUwobufWEsN7FQaiEJVp80BetMr4CEc%3D
// file sas query ?st=2019-04-19T07%3A34%3A47Z&se=2019-04-20T07%3A34%3A47Z&sp=rl&sv=2018-03-28&sr=f&sig=JWzuvz8p2LCnmUwobufWEsN7FQaiEJVp80BetMr4CEc%3D
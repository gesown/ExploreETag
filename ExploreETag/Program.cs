using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;

namespace ExploreETag
{
    class Program
    {
        private static HttpResponseMessage response;

        static void Main(string[] args)
        {
            var currntLocalFilePath = args[0];
            var newLocalFilePath = currntLocalFilePath + "new.vsdx";
            var ETagIn = args[1];
            Uri sasUrl = new Uri(
    "https://msstoragedemo.file.core.windows.net/msdemofileshare/WisdomContexts.vsdx?st=2019-04-19T07%3A34%3A47Z&se=2019-04-20T07%3A34%3A47Z&sp=rl&sv=2018-03-28&sr=f&sig=JWzuvz8p2LCnmUwobufWEsN7FQaiEJVp80BetMr4CEc%3D");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, sasUrl);
            response = GetHeadResponse(request).Result;
            var ETag = response.Headers.ETag.ToString();
            if (ETag.Contains( ETagIn))
            {
                Console.WriteLine("Local and Remote Files are Identical");
                return;
            }
            FileStream writer=new FileStream(newLocalFilePath,FileMode.Create);
            var fileResponse = GetFileResponse(sasUrl).Result;
            var frstream = fileResponse.Content.ReadAsByteArrayAsync().Result;
            writer.Write(frstream,0,frstream.Length);
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
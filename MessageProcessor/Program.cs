using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace MessageProcessor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            S3Helper s3Helper = new S3Helper();
            Task[] tasks = new Task[1000];

            for (int index = 0; index < 1000; index++)
            {
                tasks[index] = NewMethod(s3Helper, index);
            }

            Task.WaitAll(tasks);
        }

        private static async Task NewMethod(S3Helper s3Helper, int index)
        {
            var fileName = "securefiles/test_" + index + ".txt";
            if (await s3Helper.Upload(new MemoryStream(Encoding.UTF8.GetBytes(new string('*', 80896))), fileName) == System.Net.HttpStatusCode.OK)
            {
                await s3Helper.CheckFileExists(fileName);
            }
        }
    }
}

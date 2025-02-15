using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace LiteInstaller
{
    public class FileDownload
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            MaxResponseContentBufferSize = 10 * 1024 * 1024
        };

        public async Task DownloadFileAsync(string url, string destinationPath, int parallelChunks = 8)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            using (HttpResponseMessage response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();
                long totalBytes = response.Content.Headers.ContentLength ?? -1;

                if (totalBytes <= 0)
                    throw new Exception("Не удалось определить размер файла.");

                long chunkSize = totalBytes / parallelChunks;
                List<Task> downloadTasks = new List<Task>();
                byte[][] fileChunks = new byte[parallelChunks][];

                for (int i = 0; i < parallelChunks; i++)
                {
                    long start = i * chunkSize;
                    long end = (i == parallelChunks - 1) ? totalBytes - 1 : (start + chunkSize - 1);
                    int index = i;

                    downloadTasks.Add(Task.Run(async () =>
                    {
                        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
                        {
                            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);

                            using (HttpResponseMessage chunkResponse = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                            {
                                chunkResponse.EnsureSuccessStatusCode();
                                fileChunks[index] = await chunkResponse.Content.ReadAsByteArrayAsync();
                            }
                        }
                    }));
                }

                await Task.WhenAll(downloadTasks);

                using (FileStream fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 65536, true))
                {
                    foreach (var chunk in fileChunks)
                    {
                        await fileStream.WriteAsync(chunk, 0, chunk.Length);
                    }
                }
            }

            stopwatch.Stop();
            Console.WriteLine($"Скачивание завершено за {stopwatch.Elapsed.TotalSeconds:F2} секунд.");
        }
    }
}


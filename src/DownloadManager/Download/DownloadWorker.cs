using System;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using PlexRipper.Application.Common.Interfaces.FileSystem;
using PlexRipper.Domain;
using PlexRipper.Domain.Entities;
using PlexRipper.Domain.Types;
using PlexRipper.DownloadManager.Common;

namespace PlexRipper.DownloadManager.Download
{
    public class DownloadWorker
    {
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        private readonly IFileSystem _fileSystem;
        private readonly Progress<long> _progress = new Progress<long>();
        private Task _copyTask;

        public Subject<DownloadWorkerProgress> DownloadWorkerProgressSubject { get; } = new Subject<DownloadWorkerProgress>();
        private Task _task;

        public DownloadWorker(int id, DownloadTask downloadTask, DownloadRange downloadRange, IFileSystem fileSystem)
        {
            if (downloadRange != null)
            {
                DownloadRange = downloadRange;
            }
            Id = id;
            DownloadTask = downloadTask;
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// The download worker id
        /// </summary>
        public int Id { get; }

        public DownloadTask DownloadTask { get; }
        public DownloadRange DownloadRange { get; }

        public void Start()
        {
            Log.Debug($"Download worker {Id} start for {DownloadTask.FileName}");

            var fileStreamResult = _fileSystem.SaveFile(DownloadTask.DownloadDirectory, $"{Id}-{DownloadTask.FileName}", DownloadRange.RangeSize);
            if (fileStreamResult.IsFailed)
            {
                return;
            }

            _task = Task.Factory.StartNew(() =>
            {
                using var httpClient = new HttpClient();
                using var requestMessage = new HttpRequestMessage(HttpMethod.Get, DownloadRange.Uri);

                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(DownloadRange.Uri);
                request.AddRange(DownloadRange.StartByte, DownloadRange.EndByte);
                var response = (HttpWebResponse) request.GetResponse();

                // var response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, _cancellationToken);
                using var _responseStream = response.GetResponseStream();
                using var fileStream = fileStreamResult.Value;

                var buffer = new byte[2048];

                while (true)
                {
                    int bytesRead = _responseStream.Read(buffer, 0, buffer.Length);

                    if (response.ContentLength > 0)
                    {
                        bytesRead = (int) Math.Min(DownloadRange.RangeSize - DownloadRange.BytesReceived, bytesRead);
                    }

                    if (bytesRead <= 0)
                    {
                        break;
                    }

                    fileStream.Write(buffer, 0, bytesRead);
                    fileStream.Flush();

                    DownloadRange.BytesReceived += bytesRead;
                    DownloadWorkerProgressSubject.OnNext(new DownloadWorkerProgress(Id, DownloadRange.BytesReceived, DownloadRange.RangeSize));

                    // _copyTask = _responseStream
                    //     .CopyToAsync(fileStream, _progress, 2048, _cancellationToken.Token);
                }
            }, TaskCreationOptions.LongRunning);
        }
    }
}
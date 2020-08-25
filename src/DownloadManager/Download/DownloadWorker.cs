using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using FluentResults;
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

        private readonly string _fileName;
        private readonly IFileSystem _fileSystem;

        private Task _task;

        public DownloadWorker(int id, DownloadTask downloadTask, DownloadRange downloadRange, IFileSystem fileSystem)
        {
            if (downloadRange != null)
            {
                DownloadRange = downloadRange;
            }
            _fileSystem = fileSystem;

            Id = id;
            DownloadTask = downloadTask;
            _fileName = $"{Id}-{DownloadTask.FileName}";
        }

        public Subject<DownloadWorkerProgress> DownloadWorkerProgressSubject { get; } = new Subject<DownloadWorkerProgress>();

        public long DownloadSpeed { get; internal set; }
        public TimeSpan ElapsedTime => DateTime.UtcNow.Subtract(DownloadStartAt);
        public DateTime DownloadStartAt { get; internal set; }

        /// <summary>
        /// The download worker id
        /// </summary>
        public int Id { get; }

        public DownloadTask DownloadTask { get; }
        public DownloadRange DownloadRange { get; }

        public string FileName => _fileName;
        public string FilePath => Path.Combine(DownloadTask.DownloadDirectory, _fileName);

        public Result<Task> Start()
        {
            Log.Debug($"Download worker {Id} start for {DownloadTask.FileName}");

            var fileStreamResult = _fileSystem.SaveFile(DownloadTask.DownloadDirectory, _fileName, DownloadRange.RangeSize);
            if (fileStreamResult.IsFailed)
            {
                return fileStreamResult.ToResult();
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

                    DownloadWorkerProgressSubject.OnNext(new DownloadWorkerProgress(Id, DownloadRange.BytesReceived, DownloadRange.RangeSize));
                }
            }, TaskCreationOptions.LongRunning);
            return Result.Ok(_task);
        }

        private void CalculateDownloadSpeed(long bytesReceived)
        {
            DownloadSpeed = Convert.ToInt64(Math.Round(bytesReceived / ElapsedTime.TotalSeconds, 2));
        }
    }
}
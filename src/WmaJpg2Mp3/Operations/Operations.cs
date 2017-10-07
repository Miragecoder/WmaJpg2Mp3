using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Lame;
using WmaJpg2Mp3;
using System.Diagnostics;

namespace WmaJpg2Mp3.Operations
{
    class OperationCompletedEventArgs : EventArgs
    {
        public OperationCompletedEventArgs(
            DateTime startTime,
            TimeSpan duration,
            int itemCount,
            Exception exception
            )
        {
            StartTime = startTime;
            Duration = duration;
            ItemCount = itemCount;
            Exception = exception;
        }
        public DateTime StartTime { get; }
        public TimeSpan Duration { get; }
        public int ItemCount { get; }
        public Exception Exception { get; }
    }

    class MessageEventArgs : EventArgs
    {
        public string Message { get; }
        public MessageEventArgs(string message) { Message = message; }
    }

    class OnProgressEventArgs : EventArgs
    {
        public int ProcessedItemCount { get; }
        public int TotalItemCount { get; }
        public OnProgressEventArgs(int processedItemCount, int totalItemCount)
        {
            ProcessedItemCount = processedItemCount;
            TotalItemCount = totalItemCount;
        }
    }

    class OperationManager
    {
        private readonly object _lock = new object();
        private Operation _currentOperation;

        public void Start(string sourcePath, string destinationPath)
        {
            lock (_lock)
            {
                CancelCurrent();
                _currentOperation = new Operation(sourcePath, destinationPath);
                _currentOperation.OnCompleted += (sender, e) =>
                {
                    lock (_lock)
                    {
                        _currentOperation = null;
                    }
                    OnCompleted(this, e);
                };
                _currentOperation.OnMessage += OnMessage;
                _currentOperation.OnStarted += OnStarted;
                _currentOperation.OnProgress += OnProgress;
                _currentOperation.Start();
            }
        }

        public void CancelCurrent()
        {
            lock (_lock)
            {
                _currentOperation?.Cancel();
            }
        }

        public event EventHandler<OperationCompletedEventArgs> OnCompleted;
        public event EventHandler OnStarted;
        public event EventHandler<MessageEventArgs> OnMessage;
        public event EventHandler<OnProgressEventArgs> OnProgress;
    }

    class Operation
    {
        private readonly Task _task;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private void RaiseMessage(string message) => OnMessage(this, new MessageEventArgs(message));

        public Operation(string sourcePath, string destinationPath)
        {
            _task = new Task(async () =>
            {
                var operationStart = DateTime.Now;
                var stopwatch = Stopwatch.StartNew();
                var itemCount = 0;
                Exception exception = null;

                OnProgress(
                    this,
                    new OnProgressEventArgs(
                        processedItemCount: 0,
                        totalItemCount: 1
                    )
                );
                try
                {
                    var sourceFiles = Directory
                        .EnumerateFiles(sourcePath, "*.wma", SearchOption.AllDirectories)
                        .Select(path => new FileInfo(path))
                        .Select(fileInfo => new FileConversionInfo(
                            sourceFolder: sourcePath,
                            destinationFolder: destinationPath,
                            sourceFile: fileInfo,
                            albumCoverFile: Directory
                                .EnumerateFiles(fileInfo.Directory.FullName, "*.jpg", SearchOption.TopDirectoryOnly)
                                .Select(candidateCover => new FileInfo(candidateCover))
                                .OrderByDescending(x => x.Length)
                                .FirstOrDefault()
                        ))
                        .ToList();

                    itemCount = sourceFiles.Count;
                    var processedFileCount = 0;
                    foreach (var entry in sourceFiles)
                    {
                        _cancellationTokenSource.Token.ThrowIfCancellationRequested();
                        RaiseMessage(entry.GetSummary());
                        if (!entry.TargetFileAlreadyExists)
                            await entry.ExecuteConversionAsync();

                        OnProgress(
                            this, 
                            new OnProgressEventArgs(
                                processedItemCount: ++processedFileCount, 
                                totalItemCount: sourceFiles.Count
                            )
                        );
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    OnProgress(
                        this,
                        new OnProgressEventArgs(
                            processedItemCount: 1,
                            totalItemCount: 1
                        )
                    );
                    OnCompleted(
                        this, 
                        new OperationCompletedEventArgs(
                            operationStart, 
                            stopwatch.Elapsed,
                            itemCount,
                            exception
                        )
                    );
                }
            }, _cancellationTokenSource.Token);
        }

        public event EventHandler<OperationCompletedEventArgs> OnCompleted;
        public event EventHandler OnStarted;
        public event EventHandler<MessageEventArgs> OnMessage;
        public event EventHandler<OnProgressEventArgs> OnProgress;

        public void Cancel() => _cancellationTokenSource.Cancel();

        public void Start()
        {
            _task.Start();
            OnStarted(this, new EventArgs());
        }

        private class FileConversionInfo
        {
            private readonly string _sourceFolder;
            private readonly string _destinationFolder;
            private readonly FileInfo _sourceFile;
            private readonly FileInfo _albumCoverFileInfo;

            public FileConversionInfo(
                string sourceFolder,
                string destinationFolder,
                FileInfo sourceFile,
                FileInfo albumCoverFile)
            {
                _sourceFolder = sourceFolder;
                _destinationFolder = destinationFolder;
                _sourceFile = sourceFile;
                _albumCoverFileInfo = albumCoverFile;
            }

            public bool HasCover => _albumCoverFileInfo != null;

            public string GenerateTargetFilePath() =>
                Path.Combine(
                    path1: _destinationFolder,
                    path2: new Uri(_sourceFolder)
                        .MakeRelativeUri(new Uri(_sourceFile.FullName))
                        .OriginalString
                        .Replace('/', '\\')
                    )
                    .Replace(".wma", ".mp3")
                    .Pipe(x => Uri.UnescapeDataString(x));

            public bool TargetFileAlreadyExists => File.Exists(GenerateTargetFilePath());

            public string GetSummary()
            {
                return $"Target path: '{GenerateTargetFilePath()}' - Cover ({_albumCoverFileInfo.FullName}) ";
            }

            public async Task ExecuteConversionAsync()
            {
                Directory.CreateDirectory(
                    new FileInfo(
                        GenerateTargetFilePath()
                    )
                    .Directory
                    .FullName
                );
                var albumCoverBytes = await _albumCoverFileInfo
                    .PipeAsync(async x =>
                    {
                        using (var fileStream = File.OpenRead(
                            _albumCoverFileInfo.FullName))
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await fileStream.CopyToAsync(memoryStream);
                                memoryStream.Position = 0;
                                return memoryStream.ToArray();
                            }
                        }
                    });

                var id3TagData = Scope.Pipe(() =>
                    {
                        using (var wmaStream = new NAudio.WindowsMediaFormat.WmaStream(_sourceFile.FullName))
                        {
                            return new ID3TagData()
                            {
                                AlbumArt = albumCoverBytes,
                                Album = wmaStream["WM/AlbumTitle"],
                                Title = wmaStream["Title"],
                                Artist = wmaStream["WM/AlbumArtist"],
                                Track = wmaStream["WM/TrackNumber"],
                                Year = wmaStream["WM/Year"]
                            };
                        }
                    });


                using (var reader = new AudioFileReader(_sourceFile.FullName))
                {
                    using (var writer = new LameMP3FileWriter(
                        GenerateTargetFilePath(),
                        reader.WaveFormat,
                        LAMEPreset.STANDARD, 
                        id3TagData
                    ))
                    {
                        await reader.CopyToAsync(writer);
                    }
                }
            }
        }
    }

}

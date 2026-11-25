using System.Collections.Concurrent;
using System.Threading;
using System.Windows;

namespace FactorioToolAssistedSpeedrun.Services
{
    /// <summary>
    /// Batches step writes and applies them on a background thread so the UI does not wait on the database.
    /// Any read through <see cref="ProjectDbContext"/> flushes pending writes first to keep results consistent.
    /// </summary>
    public static class DatabaseWriteQueue
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);
        private static readonly ConcurrentQueue<WriteRequest> Requests = new();
        private static readonly AutoResetEvent Wake = new(false);
        private static readonly Lock WorkerLock = new();
        private static volatile Thread? _worker;
        private static volatile bool _stopping;

        private sealed record WriteRequest(string ProjectDataFile, Action<ProjectDbContext>? Write, ManualResetEventSlim? Done);

        public static void Enqueue(string projectDataFile, Action<ProjectDbContext> write)
        {
            EnsureStarted();
            Requests.Enqueue(new WriteRequest(projectDataFile, write, null));
        }

        /// <summary>
        /// Blocks until every write enqueued before this call has been applied.
        /// </summary>
        public static void Flush()
        {
            if (_worker is null)
            {
                return;
            }

            using var done = new ManualResetEventSlim(false);
            Requests.Enqueue(new WriteRequest(string.Empty, null, done));
            Wake.Set();
            done.Wait();
        }

        /// <summary>
        /// Applies all remaining writes and stops the worker thread. Call on application exit.
        /// </summary>
        public static void Shutdown()
        {
            lock (WorkerLock)
            {
                var worker = _worker;
                if (worker is null)
                {
                    return;
                }

                _stopping = true;
                Wake.Set();
                worker.Join();
                _worker = null;
                _stopping = false;
            }
        }

        private static void EnsureStarted()
        {
            if (_worker is not null)
            {
                return;
            }

            lock (WorkerLock)
            {
                if (_worker is not null)
                {
                    return;
                }

                _worker = new Thread(Run) { IsBackground = true, Name = "DatabaseWriteQueue" };
                _worker.Start();
            }
        }

        private static void Run()
        {
            while (true)
            {
                Wake.WaitOne(Interval);

                var stopping = _stopping;
                var batch = new List<WriteRequest>();
                while (Requests.TryDequeue(out var request))
                {
                    batch.Add(request);
                }

                try
                {
                    Apply(batch);
                }
                finally
                {
                    foreach (var request in batch)
                    {
                        request.Done?.Set();
                    }
                }

                if (stopping)
                {
                    return;
                }
            }
        }

        private static void Apply(List<WriteRequest> batch)
        {
            // Group by file so each database gets one connection and one transaction, keeping order within each file.
            var groups = batch
                .Where(request => request.Write is not null)
                .GroupBy(request => request.ProjectDataFile);

            foreach (var group in groups)
            {
                try
                {
                    using var context = new ProjectDbContext(group.Key, waitForPendingWrites: false);
                    context.RunInTransaction(() =>
                    {
                        foreach (var request in group)
                        {
                            try
                            {
                                request.Write!(context);
                            }
                            catch (Exception ex)
                            {
                                ReportError(ex);
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    ReportError(ex);
                }
            }
        }

        private static void ReportError(Exception ex)
        {
            if (ex.InnerException is not null)
            {
                ex = ex.InnerException;
            }

            var message = $"An error occurred while writing to the database: {ex.Message}";
            // BeginInvoke avoids deadlocking when the UI thread is blocked in Flush().
            Application.Current?.Dispatcher.BeginInvoke(() =>
                MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error));
        }
    }
}
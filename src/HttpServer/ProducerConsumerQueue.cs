using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace HttpServer
{
    class ProducerConsumerQueue : IDisposable
    {
        BlockingCollection<Task> taskQueue = new BlockingCollection<Task>();

        public ProducerConsumerQueue(int workerCount)
        {
            for (int i = 0; i < workerCount; i++)
            {
                Task.Factory.StartNew(Consume, TaskCreationOptions.LongRunning);
            }
        }

        public Task Enqueue(Action action, CancellationToken cts = default(CancellationToken))
        { 
            Task task = new Task(action, cts);
            taskQueue.Add(task);
            return task;
        }

        public Task<TResult> Enqueue<TResult>(Func<TResult> func, CancellationToken cts = default(CancellationToken))
        {
            Task<TResult> task = new Task<TResult>(func, cts);
            taskQueue.Add(task);
            return task;
        }

        void Consume()
        {
            // Will block when no elements are available and will end when CompleteAdding is called
            foreach (Task task in taskQueue.GetConsumingEnumerable())
            {
                try
                {
                    if (!task.IsCanceled)
                    {
                        task.RunSynchronously();
                    }
                }
                catch (InvalidOperationException) { }
            }
        }

        public void Dispose()
        {
            taskQueue.CompleteAdding();
        }
    }
}

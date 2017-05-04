using System;
using System.Threading;

namespace _10Operations
{
    public static class OperationProcessor
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(2, 2);

        public static void ProcessOperations(int count = 10)
        {
            var events = new ManualResetEvent[count];

            for (int i = 0; i < count; i++)
            {
                ManualResetEvent resetEvent = new ManualResetEvent(false);
                events[i] = resetEvent;

                ThreadPool.QueueUserWorkItem(
                    x =>
                        {
                            try
                            {
                                _semaphore.Wait();
                                Operation();
                            }
                            finally
                            {
                                _semaphore.Release();
                                resetEvent.Set();
                            }
                        });

            }

            foreach (var e in events) e.WaitOne();
        }

        private static void Operation()
        {
            Console.WriteLine($"Thread #{Thread.CurrentThread.ManagedThreadId} in progress...");
            Thread.Sleep(1000);
            Console.WriteLine($"Thread #{Thread.CurrentThread.ManagedThreadId} finished operation");
        }
    }

}


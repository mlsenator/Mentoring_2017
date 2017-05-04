using System;
using System.Threading.Tasks;

namespace ContinuationTask
{
    public static class TaskCreator
    {
        public static void GetRegardlessOfParentResultTask()
        {
            var task = Task.Run(
                () =>
                    {
                        Console.WriteLine("Parent task in progress...");
                        Console.WriteLine("Throwing exception...");
                        throw new Exception();
                    });

            var continuationTask = task.ContinueWith(
                (antecedent) =>
                    {
                        Console.WriteLine("Continuation task in progress, regardless of the result of the parent task.");
                        Console.WriteLine($"Exception in parent task: {antecedent.Exception.InnerException.Message}");
                    });

            continuationTask.Wait();
        }

        public static void GetExecuteAfterFailedParentResultTask()
        {
            var task = Task.Run(
                () =>
                    {
                        Console.WriteLine("Parent task in progress...");
                        Console.WriteLine("Throwing exception...");
                        throw new Exception();
                    });

            var continuationTask = task.ContinueWith(
                (antecedent) =>
                    {
                        Console.WriteLine("Continuation task in progress, parent task finished without success..");
                        Console.WriteLine($"Exception in parent task: {antecedent.Exception.InnerException.Message}");
                    },
                TaskContinuationOptions.OnlyOnFaulted);
            continuationTask.Wait();
        }

        public static void GetExecuteAfterFailedParentResultReusedTask()
        {
            var task = Task.Run(
                () =>
                    {
                        Console.WriteLine("Parent task in progress...");
                        Console.WriteLine("Throwing exception...");
                        throw new Exception();
                    });

            var continuationTask = task.ContinueWith(
                (antecedent) =>
                    {
                        Console.WriteLine("Continuation task in progress, parent task finished without success, reusing parent task thread.");
                        Console.WriteLine($"Exception in parent task: {antecedent.Exception.InnerException.Message}");
                    },
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
            continuationTask.Wait();
        }

        public static void GetExecuteAfterCanceledParentOutsideOfTheThreadPoolTask()
        {
            var task = Task.Run(
                () =>
                    {
                        Console.WriteLine("Parent task in progress...");
                        Console.WriteLine("Throwing cancelation exception...");
                        throw new TaskCanceledException();
                    });

            var continuationTask = task.ContinueWith(
                (antecedent) =>
                    {
                        Console.WriteLine("Continuation task in progress, parent task was canceled.");
                    },
                TaskContinuationOptions.OnlyOnCanceled | TaskContinuationOptions.ExecuteSynchronously);

            continuationTask.Wait();
        }
    }
}
using System;

namespace ContinuationTask
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskCreator.GetRegardlessOfParentResultTask();
            Console.WriteLine();
            TaskCreator.GetExecuteAfterFailedParentResultTask();
            Console.WriteLine();
            TaskCreator.GetExecuteAfterFailedParentResultReusedTask();
            Console.WriteLine();
            TaskCreator.GetExecuteAfterCanceledParentOutsideOfTheThreadPoolTask();

            Console.ReadKey();
        }
    }
}

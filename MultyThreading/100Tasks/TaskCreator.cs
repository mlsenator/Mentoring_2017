using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace _100Tasks
{
    public static class TaskCreator
    {
        public static void CreateHotTasks(int count = 100)
        {
            var tasks = new List<Task>();

            for (int i = 1; i <= count; i++)
            {
                var taskId = i;
                tasks.Add(Task.Run(() => Do(taskId)));
            }

            Task.WaitAll(tasks.ToArray());
        }

        private static void Do(int taskId)
        {
            for (int i = 1; i <= 1000; i++)
            {
                Console.WriteLine($"Task #{taskId}: {i}");
            }
        }
    }
}

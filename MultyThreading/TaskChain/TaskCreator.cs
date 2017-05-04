using System;
using System.Linq;
using System.Threading.Tasks;

namespace TaskChain
{
    public static class TaskCreator
    {
        private static readonly Random _random = new Random();
        private const int ArraySize = 10;

        public static void CreateHotTaskChain()
        {

            Task.Run(() => CreateArray())
                .ContinueWith((arr) => ProcessArray(arr.Result))
                .ContinueWith((arr) => SortArray(arr.Result))
                .ContinueWith((arr) => GetAverage(arr.Result))
                .Wait();
        }

        private static int[] CreateArray()
        {
            var array = Enumerable.Repeat(0, ArraySize).Select(v => _random.Next(100)).ToArray();

            Console.WriteLine($"Created array: [{string.Join(", ", array)}]");

            return array;
        }

        private static int[] ProcessArray(int[] array)
        {
            array = array.Select(v => v * _random.Next(100)).ToArray();

            Console.WriteLine($"Multyplying finished: [{string.Join(", ", array)}]");

            return array;
        }

        private static int[] SortArray(int[] array)
        {
            array = array.OrderByDescending(v => v).ToArray();

            Console.WriteLine($"Array sorted: [{string.Join(", ", array)}]");

            return array;
        }

        private static void GetAverage(int[] array)
        {
            var average = array.Average();

            Console.WriteLine($"Average result: {average}");
        }
    }
}

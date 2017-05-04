using System;

namespace TaskChain
{
    class Program
    {
        static void Main(string[] args)
        {
            TaskCreator.CreateHotTaskChain();

            Console.ReadKey();
        }
    }
}

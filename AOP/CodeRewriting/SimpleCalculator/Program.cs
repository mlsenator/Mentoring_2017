using System;

namespace SimpleCalculator
{
	class Program
	{
		static void Main(string[] args)
		{
			var calculator = new Caluclator();

			while (true)
			{
				Console.WriteLine("Enter expression (e.g. 1 + 4; 5 - 2):");
				string input = Console.ReadLine();
				var output = calculator.Process(input);
				if (output.Success)
				{
					Console.WriteLine("Result: {0}", output.Result);
				}
				else
				{
					Console.WriteLine("Incorrect arguments");
				}
			}
		}
	}
}

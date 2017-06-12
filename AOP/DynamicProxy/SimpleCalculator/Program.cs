using System;
using Castle.DynamicProxy;
using Logger;
using Ninject;
using Ninject.Extensions.Interception;
using Ninject.Extensions.Interception.Infrastructure.Language;

namespace SimpleCalculator
{
	class Program
	{
		static void Main(string[] args)
		{
			var generator = new ProxyGenerator();
            ICalculator calculator = generator.CreateInterfaceProxyWithTarget<ICalculator>(new Caluclator(), new LoggerInterceptor());

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

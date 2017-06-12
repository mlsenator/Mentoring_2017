using System;
using System.IO;
using System.Text;
using Castle.DynamicProxy;
using Newtonsoft.Json;

namespace Logger
{
	public class LoggerInterceptor : IInterceptor
	{
		private readonly string _file = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt");

		public void Intercept(IInvocation invocation)
		{
			var method = invocation.Method;
			var methodParametes = method.GetParameters();
			var arguments = invocation.Arguments;
			var parameters = new StringBuilder();

			for (var i = 0; i < methodParametes.Length; i++)
			{
				var paramNane = methodParametes[i].Name;
				var paramValue = JsonConvert.SerializeObject(arguments[i]);
				parameters.AppendFormat("{0} = {1} ", paramNane, paramValue);
			}

			Log($"{DateTime.Now} call {method.Name} with params: {parameters}");

			invocation.Proceed();

			var result = JsonConvert.SerializeObject(invocation.ReturnValue);
			Log($"{DateTime.Now} {method.Name} returns: {result}");
		}

		private void Log(string data)
		{
			using (StreamWriter sw = File.AppendText(_file))
			{
				sw.WriteLine(data);
			}
		}
	}
}

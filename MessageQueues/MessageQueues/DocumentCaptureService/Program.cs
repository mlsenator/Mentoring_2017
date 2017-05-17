using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.IO;
using Topshelf;

namespace DocumentСaptureService
{
	class Program
	{
		static void Main(string[] args)
		{
			var currentDir = AppDomain.CurrentDomain.BaseDirectory;
			var inDir = Path.Combine(currentDir, "in");
			var tempDir = Path.Combine(currentDir, "temp");

			HostFactory.Run(
				hostConf =>
				{
					hostConf.Service<DocumentCaptureService>(
						s =>
						{
							s.ConstructUsing(() => new DocumentCaptureService(inDir, tempDir));
							s.WhenStarted(serv => serv.Start());
							s.WhenStopped(serv => serv.Stop());
						});
					hostConf.SetServiceName("DCService");
					hostConf.SetDisplayName("Doucument Capture Service");
					hostConf.StartAutomaticallyDelayed();
					hostConf.RunAsLocalService();
				});
		}
	}
}

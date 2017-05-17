using System;
using System.IO;
using Topshelf;

namespace CentralServer
{
	class Program
	{
		static void Main(string[] args)
		{
			var currentDir = AppDomain.CurrentDomain.BaseDirectory;
			var outDir = Path.Combine(currentDir, "out");

			HostFactory.Run(
				hostConf =>
				{
					hostConf.Service<CentaralServer>(
						s =>
						{
							s.ConstructUsing(() => new CentaralServer(outDir));
							s.WhenStarted(serv => serv.Start());
							s.WhenStopped(serv => serv.Stop());
						});
					hostConf.SetServiceName("CentaralServer");
					hostConf.SetDisplayName("Centaral Server");
					hostConf.StartAutomaticallyDelayed();
					hostConf.RunAsLocalService();
				});
		}
	}
}

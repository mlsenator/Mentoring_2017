using NLog;
using NLog.Config;
using NLog.Targets;
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

            var logConfig = new LoggingConfiguration();
            var target = new FileTarget()
            {
                Name = "Default",
                FileName = Path.Combine(currentDir, "log.txt"),
                Layout = "${date} ${message} ${onexception:inner=${exception:format=toString}}"
            };

            logConfig.AddTarget(target);
            logConfig.AddRuleForAllLevels(target);

            var logFactory = new LogFactory(logConfig);

            HostFactory.Run(
				hostConf =>
				{
					hostConf.Service<CentaralServer>(
						s =>
						{
							s.ConstructUsing(() => new CentaralServer(outDir));
							s.WhenStarted(serv => serv.Start());
							s.WhenStopped(serv => serv.Stop());
						}).UseNLog(logFactory);
                    hostConf.SetServiceName("CentaralServer");
					hostConf.SetDisplayName("Centaral Server");
					hostConf.StartAutomaticallyDelayed();
					hostConf.RunAsLocalService();
				});
		}
	}
}

﻿using ImageToPdfConverter;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Diagnostics;
using System.IO;
using Topshelf;

namespace ImageToPdfService
{

    class Program
    {
        static void Main(string[] args)
        {
            var currentDir = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var inDir = Path.Combine(currentDir, "in");
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
                    hostConf.Service<ImageToPdf>(
                        s =>
                        {
                            s.ConstructUsing(() => new ImageToPdf(inDir, outDir));
                            s.WhenStarted(serv => serv.Start());
                            s.WhenStopped(serv => serv.Stop());
                        }).UseNLog(logFactory);
                    hostConf.SetServiceName("KokokoService");
                    hostConf.SetDisplayName("Kokoko Service");
                    hostConf.StartAutomaticallyDelayed();
                    hostConf.RunAsLocalService();
                }
            );

        }
    }
}
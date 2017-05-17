using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContract
{
    public static class ServiceHelper
    {
        public static string ServerQueueName = @".\private$\CentralServerQueue";
        public static string MonitorQueueName = @".\private$\MonitorQueue";
        public static string ClientQueueName = @".\Private$\ClientQueue";

        public static int DefaultTimeOut = 5000;
        public static int ChunkSize = 1024;
    }
}

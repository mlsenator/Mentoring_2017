using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using IanaDomains;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IanaDomainsTest
{
    [TestClass]
    public class DomainWhoisRetrieverTest
    {
        private Stopwatch sw = new Stopwatch();
        
        [TestMethod]
        public void DomainWhoisRetrieverThreadPoolTest()
        {
            var retriever = new DomainWhoisRetrieverThreadPool();
            sw.Start();
            var domains = retriever.GetAll();
            sw.Stop();
            FormatOutput(domains, sw.Elapsed);
        }


        [TestMethod]
        public void DomainWhoisRetrieverTPLTest()
        {
            var retriever = new DomainWhoisRetrieverTPL();
            sw.Start();
            var domains = retriever.GetAll();
            sw.Stop();
            FormatOutput(domains, sw.Elapsed);
        }

        [TestMethod]
        public async Task DomainWhoisRetrieverAsyncTest()
        {
            var retriever = new DomainWhoisRetrieverAsync();
            sw.Start();
            var domains = await retriever.GetAllAsync();
            sw.Stop();
            FormatOutput(domains, sw.Elapsed);
        }

        private void FormatOutput(Dictionary<string, string> domains, TimeSpan processingTime)
        {
            foreach (var info in domains)
            {
                Console.WriteLine($"Domain: {info.Key,20} Whois: {(string.IsNullOrEmpty(info.Value) ? "N/A" : info.Value)}");
            }

            Console.WriteLine($"Time spent: {processingTime:hh\\:mm\\:ss}");
        }
    }
}

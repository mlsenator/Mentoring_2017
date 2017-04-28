using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace IanaDomains
{
    public class DomainWhoisRetrieverThreadPool : IDomainWhoisRetriever
    {
        private readonly IanaDomainHelper _helper = new IanaDomainHelper();
        private Dictionary<string, string> _result = new Dictionary<string, string>();

        public Dictionary<string, string> GetAll()
        {
            _result = new Dictionary<string, string>();

            var domains = _helper.GetAllDomains();

            var events = new ManualResetEvent[domains.Count()];
            var index = 0;

            foreach (var domain in domains.Take(100))
            {
                ManualResetEvent resetEvent = new ManualResetEvent(false);
                events[index++] = resetEvent;

                ThreadPool.QueueUserWorkItem(
                    x =>
                        {
                            try
                            {
                                GetWhoisByDomain(domain);
                            }
                            finally
                            {
                                resetEvent.Set();
                            }
                        });
            }

            foreach (var e in events.Take(100)) e.WaitOne();

            return _result;
        }

        private void GetWhoisByDomain(object domainString)
        {
            var domain = (string)domainString;
            var whois = _helper.GetWhoisServerByDomain(domain);
            _result.Add(domain, whois);
        }
    }
}

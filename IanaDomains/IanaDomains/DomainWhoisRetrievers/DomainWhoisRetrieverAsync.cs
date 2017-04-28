using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IanaDomains
{
    public class DomainWhoisRetrieverAsync : IDomainWhoisRetrieverAsync
    {
        private readonly IanaDomainHelper _helper = new IanaDomainHelper();
        private Dictionary<string, string> _result = new Dictionary<string, string>();

        public async Task<Dictionary<string, string>> GetAllAsync()
        {
            _result = new Dictionary<string, string>();
            var domains = await _helper.GetAllDomainsAsync();

            var tasks = new List<Task>();

            foreach (var domain in domains.Take(100))
            {
                var task = _helper.GetWhoisServerByDomainAsync(domain);
                tasks.Add(task);
                _result.Add(domain, await task);
            }

            await Task.WhenAll(tasks.ToArray());
            return _result;
        }
    }
}

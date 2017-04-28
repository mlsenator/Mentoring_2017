using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IanaDomains
{
    public class DomainWhoisRetrieverTPL : IDomainWhoisRetriever
    {
        private readonly IanaDomainHelper _helper = new IanaDomainHelper();
        private Dictionary<string, string> _result = new Dictionary<string, string>();

        public Dictionary<string, string> GetAll()
        {
            _result = new Dictionary<string, string>();
            var domains = _helper.GetAllDomains();

            var tasks = new List<Task>();

            foreach (var domain in domains.Take(100))
            {
                var task = Task.Factory.StartNew(() => _helper.GetWhoisServerByDomain(domain));
                tasks.Add(task);
                _result.Add(domain, task.Result);
            }

            Task.WaitAll(tasks.ToArray());
            return _result;
        }
    }
}

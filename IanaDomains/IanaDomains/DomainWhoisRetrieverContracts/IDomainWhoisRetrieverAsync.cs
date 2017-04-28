using System.Collections.Generic;
using System.Threading.Tasks;

namespace IanaDomains
{
    public interface IDomainWhoisRetrieverAsync
    {
        Task<Dictionary<string, string>> GetAllAsync();
    }
}

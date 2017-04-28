using System.Collections.Generic;

namespace IanaDomains
{
    public interface IDomainWhoisRetriever
    {
        Dictionary<string, string> GetAll();
    }
}

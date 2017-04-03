using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionAndIQueryble
{
    public interface IMapper<TSource, TDestination>
    {
        TDestination Map(TSource source);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionAndIQueryble
{
    public interface IMappingGenerator
    {
        IMapper<TSource, TDestination> Generate<TSource, TDestination>();
    }
}

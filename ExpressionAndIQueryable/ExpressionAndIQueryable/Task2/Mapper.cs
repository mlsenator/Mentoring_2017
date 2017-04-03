using System;

namespace ExpressionAndIQueryble
{
    public class Mapper<TSource, TDestination> : IMapper<TSource, TDestination>
    {
        private readonly Func<TSource, TDestination> _mapFunction;

        public Mapper(Func<TSource, TDestination> func)
        {
            this._mapFunction = func;
        }

        public TDestination Map(TSource source)
        {
            return _mapFunction(source);
        }
    }
}

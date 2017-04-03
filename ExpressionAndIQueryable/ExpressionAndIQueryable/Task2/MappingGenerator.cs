using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;

namespace ExpressionAndIQueryble
{
    public class MappingGenerator : IMappingGenerator
    {
        public IMapper<TSource, TDestination> Generate<TSource, TDestination>()
        {
            var sourceType = typeof (TSource);
            var destinationType = typeof (TDestination);
            var sourceParam = Expression.Parameter(sourceType);

            var bindings = ConfigurationBindings(sourceParam, sourceType, destinationType);

            var body = Expression.MemberInit(Expression.New(destinationType), bindings);
            var mapFunction = Expression.Lambda<Func<TSource, TDestination>>(body, sourceParam);

            return new Mapper<TSource, TDestination>(mapFunction.Compile());
        }

        private List<MemberBinding> ConfigurationBindings(ParameterExpression parameter, Type source, Type destination)
        {
            return new List<MemberBinding>()
                .MapProperties(parameter, source, destination)
                .MapFields(parameter, source, destination);
        }
    }
}

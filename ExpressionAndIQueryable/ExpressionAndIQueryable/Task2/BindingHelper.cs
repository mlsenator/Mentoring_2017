using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionAndIQueryble
{
    internal static class BindingHelper
    {
        internal static List<MemberBinding> MapProperties(this List<MemberBinding> bindings, ParameterExpression parameter, Type source, Type destination)
        {
            foreach (var sourceProperty in source.GetProperties())
            {
                var targetProperty = destination.GetProperty(sourceProperty.Name);
                if (targetProperty == null) continue;
                if (!targetProperty.CanWrite) continue;
                if (!targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType)) continue;
                bindings.Add(Expression.Bind(targetProperty, Expression.Property(parameter, sourceProperty)));
            }
            return bindings;
        }

        internal static List<MemberBinding> MapFields(this List<MemberBinding> bindings, ParameterExpression parameter, Type source, Type destination)
        { 
            foreach (var sourceField in source.GetFields())
            {
                var targetField = destination.GetField(sourceField.Name);
                if (targetField == null) continue;
                if (targetField.IsPrivate) continue;
                if (!targetField.FieldType.IsAssignableFrom(sourceField.FieldType)) continue;
                bindings.Add(Expression.Bind(targetField, Expression.Field(parameter, sourceField)));
            }
            return bindings;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressionAndIQueryable.Task1
{
    public class TransformExpressionVisitor : ExpressionVisitor
    {
        private Dictionary<string, object> MappingDictionary { get; set; }

        
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.Left is ParameterExpression && node.Right is ConstantExpression && node.Right.Type == typeof(int) &&
                (int)((ConstantExpression)node.Right).Value == 1)
            {
                switch (node.NodeType)
                {
                    case ExpressionType.Add: return Expression.Increment(node.Left);
                    case ExpressionType.Subtract: return Expression.Decrement(node.Left);
                    default: return base.VisitBinary(node);
                }
            }
            return base.VisitBinary(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (MappingDictionary != null && MappingDictionary.ContainsKey(node.Name))
            {
                return Expression.Constant(MappingDictionary[node.Name]);
            }

            return base.VisitParameter(node);

        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return Expression.Lambda(Visit(node.Body), node.Parameters.Where(p => !MappingDictionary.ContainsKey(p.Name)));
        }

        public Expression Transformation(Expression node, Dictionary<string, object> mappingDictionary)
        {
            MappingDictionary = mappingDictionary;

            return base.Visit(node);
        }
    }
}

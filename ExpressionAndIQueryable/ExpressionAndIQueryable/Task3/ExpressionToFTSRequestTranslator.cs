using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sample03
{
    public class ExpressionToFTSRequestTranslator : ExpressionVisitor
    {
        StringBuilder resultString;

        public string Translate(Expression exp)
        {
            resultString = new StringBuilder();
            Visit(exp);

            return resultString.ToString();
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable)
                && node.Method.Name == "Where")
            {
                var predicate = node.Arguments[1];
                Visit(predicate);

                return node;
            }

            if (node.Method.DeclaringType == typeof(string) &&
                new List<string> {"Contains", "StartsWith", "EndsWith"}.Contains(node.Method.Name))
            {
                var lambda = (Func<string>)CompileExtensionMethod(node);
                var binary = Expression.MakeBinary(ExpressionType.Equal,
                    Expression.Constant(lambda(), typeof(string)), node.Object);

                return VisitBinary(binary);
            }

            return base.VisitMethodCall(node);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Equal:
                    if (node.Left.NodeType == ExpressionType.MemberAccess && node.Right.NodeType == ExpressionType.Constant)
                    {
                        VisitWhereEqual(node.Left, node.Right);
                    }
                    else if (node.Left.NodeType == ExpressionType.Constant && node.Right.NodeType == ExpressionType.MemberAccess)
                    {
                        VisitWhereEqual(node.Right, node.Left);
                    }
                    else
                    {
                        throw new NotSupportedException(string.Format("Operands should be property or field or constant", node.NodeType));
                    }

                    break;
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    Visit(node.Left);
                    resultString.Append("&");
                    Visit(node.Right);
                    break;
                default:
                    throw new NotSupportedException(string.Format("Operation {0} is not supported", node.NodeType));
            };

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            resultString.Append(node.Member.Name).Append(":");

            return base.VisitMember(node);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            resultString.Append(node.Value);

            return node;
        }

        private void VisitWhereEqual(Expression first, Expression second)
        {
            Visit(first);
            resultString.Append("(");
            Visit(second);
            resultString.Append(")");
        }

        private Delegate CompileExtensionMethod(MethodCallExpression node)
        {
            var method = typeof(StringExtensionsHelper).GetMethod(node.Method.Name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var call = Expression.Call(null, method, node.Arguments[0], node.Arguments[0]);
            return Expression.Lambda(call).Compile();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ExpressionAndIQueryable.Task1;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressionAndIQueryableTest.Task1
{

    [TestClass]
    public class TransformExpressionVisitorTest
    {
        [TestMethod]
        public void TransformToIncrementTest()
        {
            Expression<Func<int, int>> source = (p) => p + 1;

            var actual = new TransformExpressionVisitor().VisitAndConvert(source, "");

            Assert.IsNotNull(actual);
            Assert.AreEqual("p => Increment(p)", actual.ToString());
        }

        [TestMethod]
        public void TransformToDecrementTest()
        {
            Expression<Func<int, int>> source = (p) => p - 1;

            var actual = new TransformExpressionVisitor().VisitAndConvert(source, "");

            Assert.IsNotNull(actual);
            Assert.AreEqual("p => Decrement(p)", actual.ToString());
        }

        [TestMethod]
        public void NoTransformationTest()
        {
            Expression<Func<int, int>> source = (p) => p - 2;

            var actual = new TransformExpressionVisitor().VisitAndConvert(source, "");

            Assert.IsNotNull(actual);
            Assert.AreEqual("p => (p - 2)", actual.ToString());
        }

        [TestMethod]
        public void ParameterIsExistExpression()
        {
            Expression<Func<int, int, int, int>> source = (a, b, c) => a + b + c;

            var actual = new TransformExpressionVisitor().Transformation(source, new Dictionary<string, object>
            {
                { "a", 21 },
                //{ "b", 23 },
                { "c", 4 }
            });

            Console.WriteLine(actual);
        }
    }
}
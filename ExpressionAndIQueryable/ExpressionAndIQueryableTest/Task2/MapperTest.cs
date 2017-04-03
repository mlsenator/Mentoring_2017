using ExpressionAndIQueryble;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpressionAndIQueryableTest
{
    [TestClass]
    public class MapperTest
    {
        private IMapper<Source, Destination> _mapper;
        [TestInitialize]
        public void Initialize()
        {
            var mapGenerator = new MappingGenerator();
            _mapper = mapGenerator.Generate<Source, Destination>();
        }

        [TestMethod]
        public void GetNewDestinationInstanceTest()
        {
            var result = _mapper.Map(new Source());
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Destination));
        }

        [TestMethod]
        public void MapPropertiesWithSameNameTest()
        {
            var source = new Source {A = "one"};
            var result = _mapper.Map(source);
            Assert.IsNotNull(result.A);
            Assert.AreEqual(source.A, result.A);
        }

        [TestMethod]
        public void MapPropertiesWithDifferentNameTest()
        {
            var source = new Source { A="one", B = "two", C = "three" };
            var result = _mapper.Map(source);
            Assert.AreEqual(source.A, result.A);
            Assert.AreEqual(source.B, result.B);
            Assert.IsNull(result.D);
            Assert.IsNotNull(source.C);
        }

        [TestMethod]
        public void MapPropertiesWithComplexTypesTest()
        {
            var source = new Source { Transfer = new Transfer() };
            var result = _mapper.Map(source);
            Assert.IsNotNull(result.Transfer);
            Assert.AreEqual(result.Transfer.Info, source.Transfer.Info);
        }

        [TestMethod]
        public void MapFieldsWithSameNameTest()
        {
            var source = new Source { E = "one"};
            var result = _mapper.Map(source);
            Assert.IsNotNull(result.E);
            Assert.AreEqual(source.E, result.E);
        }

        [TestMethod]
        public void MapFieldsWithDifferentNameTest()
        {
            var source = new Source { E = "one", F = "two", G = "three" };
            var result = _mapper.Map(source);
            Assert.AreEqual(source.E, result.E);
            Assert.AreEqual(source.F, result.F);
            Assert.IsNull(result.H);
            Assert.IsNotNull(source.G);
        }
    }

    public class Source
    {
        public string A { get; set; }

        public string B { get; set; }

        public string C { get; set; }

        public Transfer Transfer { get; set; }

        public string E;

        public string F;

        public string G;
    }

    public class Destination
    {
        public string A { get; set; }

        public string B { get; set; }

        public string D { get; set; }

        public Transfer Transfer { get; set; }

        public string E;

        public string F;

        public string H;
    }

    public class Transfer
    {
        public string Info { get; set; }
        public Transfer()
        {
            Info = "Info";
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using NeuralNetwork.Database;
using NeuralNetwork.DataPrep;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Tests
{
    [TestFixture]
    public class FindBestBuySell
    {
        [Test]
        public void LoadData()
        {
            var ds = SQLDatabase.Instance.ReadData("QTEC").GetData();

            var sut = new ProfitTester(ds);

            double sellParamert = sut.BestSell();
        }
    }

    public class ProfitTester
    {
        private readonly IList<DataElement> _elements;

        public ProfitTester(IList<DataElement> elements)
        {
            _elements = elements;
        }

        /// <summary>
        /// Looking for the optimal parameters for buy and sell
        /// </summary>
        /// <returns></returns>
        public double BestSell()
        {
            return 0;
        }
    }
}

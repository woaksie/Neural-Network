using System.Collections.Generic;
using System.Linq;
using NeuralNetwork.Database;
using NeuralNetwork.NetworkModels;

namespace NeuralNetwork.DataPrep
{
    public class TestDataPrep // todo: needs an iterface, tests, and constructor is wrong place for input values
    {
        private readonly IList<DataElement> _dataSets;

        public TestDataPrep(IList<DataElement> dataSets)
        {
            _dataSets = dataSets;
        }

        public IEnumerable<DataSet> SetupTargetValues()
        {
            for (var index = 0; index < _dataSets.Count - 1; index++)
            {
                DataElement set = _dataSets[index + 1];
                var source = set.Values.ToArray();

                var ds = new DataSet(_dataSets[index].Values, null)
                {
                    Targets = source[0] < source[1] // open < close
                        ? new[] {1D}
                        : new[] {0D}
                };

                yield return ds;
            }

            DataElement last = _dataSets.Last();
            yield return new DataSet(last.Values, new double[1]);
        }
    }
}
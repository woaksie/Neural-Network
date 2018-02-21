using System.Collections.Generic;
using System.Linq;
using NeuralNetwork.NetworkModels;

namespace NeuralNetwork
{
    public class TestDataPrep // todo: needs an iterface, tests, and constructor is wrong place for input values
    {
        private readonly IList<DataSet> _dataSets;

        public TestDataPrep(IList<DataSet> dataSets)
        {
            _dataSets = dataSets;
        }

        public IEnumerable<DataSet> SetupTargetValues()
        {
            for (var index = 0; index < _dataSets.Count - 1; index++)
            {
                var set = _dataSets[index + 1];
                _dataSets[index].Targets = set.Values[0] < set.Values[1] // open < close
                    ? new[] {1D}
                    : new[] {0D};
            }

            var last = _dataSets.Last();
            _dataSets.RemoveAt(_dataSets.Count - 1);
            yield return last;
        }
    }
}
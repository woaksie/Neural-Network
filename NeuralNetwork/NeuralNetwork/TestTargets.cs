using System.Collections.Generic;
using System.Linq;
using NeuralNetwork.NetworkModels;

namespace NeuralNetwork
{
    public class TestTargets
    {
        public static DataSet SetupTargetValues(IList<DataSet> dataSets)
        {
            for (var index = 0; index < dataSets.Count - 1; index++)
            {
                var set = dataSets[index + 1];
                dataSets[index].Targets = set.Values[0] < set.Values[1] // open < close
                    ? new[] {1D}
                    : new[] {0D};
            }

            var last = dataSets.Last();
            dataSets.RemoveAt(dataSets.Count - 1);
            return last;
        }
    }
}
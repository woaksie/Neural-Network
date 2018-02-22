using System.Collections.Generic;
using System.Linq;
using NeuralNetwork.NetworkModels;

namespace NeuralNetwork.Database
{
    public class PureData
    {
        private readonly List<DataElement> _result;

        public PureData(List<DataElement> result)
        {
            _result = result;
        }

        public IList<DataElement> GetData()
        {
            return _result.OrderBy(e => e.Key).ToArray();
        }
    }
}
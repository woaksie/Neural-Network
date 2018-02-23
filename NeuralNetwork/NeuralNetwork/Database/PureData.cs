using System.Collections.Generic;
using System.Linq;
using NeuralNetwork.NetworkModels;

namespace NeuralNetwork.Database
{
    public class PureData
    {
        private readonly IList<DataElement> _result;

        public PureData(IList<DataElement> result)
        {
            _result = result;
        }

        public IList<DataElement> GetData()
        {
            return _result.OrderBy(e => e.Key).ToArray();
        }
    }

    /// <summary>
    ///   {(double) open, (double) close, (double) high, (double) low, vol, (double) fdc, (double) mom});
    /// </summary>
    public enum StockIndex
    {
        Open,
        Close,
        High,
        Low,
        Volume,
        FiveDay,
        Momentum
    }
}
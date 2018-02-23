using System;
using System.Collections.Generic;
using System.Linq;
using NeuralNetwork.Database;

namespace NeuralNetwork.DataPrep
{
    public class BestProfit : ITargetGenerator
    {
        private readonly int _period;
        private readonly StockIndex _index;
        private readonly IList<DataElement> _elements;

        public BestProfit(int period, PureData pureData, StockIndex index)
        {
            _period = period;
            _index = index;
            _elements = pureData.GetData();
        }

        public IEnumerable<DataElement> CreateData()
        {
            for (int i = 0; i < _elements.Count - _period; i++)
            {
                var total = _elements
                    .Skip(i + 1)
                    .Take(_period)
                    .Max(d => d.Values[(int) _index]);

                yield return new DataElement(_elements[i].Key, new[] { total });
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using NeuralNetwork.Database;

namespace NeuralNetwork.DataPrep
{
    public class MovingAverage : IInputGenerator
    {
        private readonly int _periods;
        private readonly IList<DataElement> _elements;
        private StockIndex _stockIndex;

        public MovingAverage(int periods, PureData pureData, StockIndex stockIndex)
        {
            _periods = periods;
            _elements = pureData.GetData();
            _stockIndex = stockIndex;
        }

        public IEnumerable<DataElement> CreateData()
        {
            for (int i = _periods - 1; i < _elements.Count; i++)
            {
                var total = _elements
                    .Skip(1 + i - _periods)
                    .Take(_periods)
                    .Sum(d => d.Values[(int) _stockIndex]);
                var avg = total / _periods;

                yield return new DataElement(_elements[i].Key, new[] {avg});
            }
        }
    }
}
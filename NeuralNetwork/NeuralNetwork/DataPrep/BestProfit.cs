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
                //var total = _elements
                //    .Skip(i + 1)
                //    .Take(_period)
                //    .Max(d => d.Values[(int) _index]);

                yield return new DataElement(_elements[i].Key, new[] { GetBestValue(i) });
            }
        }

        /// <summary>
        /// Get the best return as a percent.
        /// </summary>
        /// <param name="i">start of period</param>
        /// <returns></returns>
        private double GetBestValue(int i)
        {
            var band = _elements
                .Skip(i + 1)
                .Take(_period)
                .ToArray();

            var buyAt = band[0].Values[(int) StockIndex.Open];
            var floor = buyAt * 0.985;
            var high = buyAt;
            double curO = 0;

            foreach (var dataElement in band)
            {
                var curr = dataElement.Values;

                var curL = curr[(int) StockIndex.Low];
                var curH = curr[(int)StockIndex.High];
                curO = curr[(int) StockIndex.Open];
                var exitAt = (high - buyAt) * 0.8D;

                if (curO < floor)
                    return (curO - buyAt) / buyAt;
                if (curL < floor)
                    return (floor - buyAt) / buyAt;

                if (exitAt / buyAt > 0.02D)
                {
                    if (curO < exitAt + buyAt)
                        return (curO - buyAt) / buyAt;
                    if (curL < exitAt + buyAt)
                        return exitAt / buyAt;
                }

                if (curH > high)
                    high = curH;
            }

            return (curO - buyAt) / buyAt;
        }
    }
}
using System;
using System.Collections.Generic;
using NeuralNetwork.Database;

namespace NeuralNetwork.DataPrep
{
    public class StockTargetsGeneratorFactory : ITargetsGeneratorFactory
    {
        private readonly TargetType _type;
        private readonly int _period;

        public StockTargetsGeneratorFactory(TargetType type, int period = 126)
        {
            _type = type;
            _period = period;
        }

        public IEnumerable<ITargetGenerator> GetTargetGenerators(PureData pureData)
        {
            switch (_type)
            {
                case TargetType.Profit:
                    return GetProfitList(pureData);
                default:
                    throw new ArgumentException($"{_type} is not supported");
            }
        }

        private IEnumerable<ITargetGenerator> GetProfitList(PureData pureData)
        {
            yield return new BestProfit(_period, pureData, StockIndex.High);
        }
    }
}
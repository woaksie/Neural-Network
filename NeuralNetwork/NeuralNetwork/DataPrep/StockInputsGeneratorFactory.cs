using System;
using System.Collections.Generic;
using System.Linq;
using NeuralNetwork.Database;

namespace NeuralNetwork.DataPrep
{
    public enum InputsType
    {
        Basic,
        Full
    }
    public class StockInputsGeneratorFactory : IInputsGeneratorFactory
    {
        private readonly InputsType _type;
        private readonly int _periods;

        public StockInputsGeneratorFactory(InputsType type, int periods = 20)
        {
            _type = type;
            _periods = periods;
        }
        public IEnumerable<IInputGenerator> GetInputGenerators(PureData pureData)
        {
            switch (_type)
            {
                case InputsType.Basic:
                    return GetBasicList(pureData);
                case InputsType.Full:
                    return Enumerable.Empty<IInputGenerator>();
                default:
                    throw new ArgumentException($"{_type} is not supported");
            }
        }

        private IEnumerable<IInputGenerator> GetBasicList(PureData pureData)
        {
            yield return new MovingAverage(_periods, pureData, StockIndex.Close);
        }
    }
}
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

        public StockInputsGeneratorFactory(InputsType type)
        {
            _type = type;
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
            yield return new MovingAverage(30, pureData);
        }
    }
}
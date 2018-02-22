using System.Collections.Generic;
using NeuralNetwork.Database;

namespace NeuralNetwork.DataPrep
{
    public interface IInputsGeneratorFactory
    {
        IEnumerable<IInputGenerator> GetInputGenerators(PureData pureData);
    }
}
using System.Collections.Generic;
using NeuralNetwork.Database;

namespace NeuralNetwork.DataPrep
{
    public interface ITargetsGeneratorFactory
    {
        IEnumerable<ITargetGenerator> GetTargetGenerators(PureData pureData);
    }
}
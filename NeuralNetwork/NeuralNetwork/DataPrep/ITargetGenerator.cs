using System.Collections.Generic;
using NeuralNetwork.Database;

namespace NeuralNetwork.DataPrep
{
    public interface ITargetGenerator
    {
        IEnumerable<DataElement> CreateData();
    }
}
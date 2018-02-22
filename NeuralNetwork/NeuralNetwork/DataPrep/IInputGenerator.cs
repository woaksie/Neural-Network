using System.Collections.Generic;
using NeuralNetwork.Database;

namespace NeuralNetwork.DataPrep
{
    public interface IInputGenerator
    {
        IEnumerable<DataElement> CreateData();
    }
}
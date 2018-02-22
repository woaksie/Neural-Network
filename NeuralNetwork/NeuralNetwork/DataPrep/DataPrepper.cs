using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using NeuralNetwork.Database;
using NeuralNetwork.NetworkModels;

namespace NeuralNetwork.DataPrep
{
    public class DataPrepper
    {
        private readonly PureData _pureData;
        private readonly IInputsGeneratorFactory _igf;
        private readonly ITargetsGeneratorFactory _tgf;
        private List<IEnumerable<DataElement>> _inputs;
        private List<IEnumerable<DataElement>> _targets;

        public DataPrepper(PureData pureData, IInputsGeneratorFactory igf, ITargetsGeneratorFactory tgf)
        {
            _pureData = pureData;
            _igf = igf;
            _tgf = tgf;
        }

        public IList<DataSet> GetTrainingInputData()
        {
            CalculateInputs();
            CalculateTargets();

            return CollateData();
        }

        private IList<DataSet> CollateData()
        {
            return Enumerable.Empty<DataSet>().ToArray();
        }

        private void CalculateInputs()
        {
            IEnumerable<IInputGenerator> generators = _igf.GetInputGenerators(_pureData);
            _inputs = new List<IEnumerable<DataElement>>();

            foreach (var generator in generators)
                _inputs.Add(generator.CreateData());
        }

        private void CalculateTargets()
        {
            IEnumerable<ITargetGenerator> generators = _tgf.GetTargetGenerators(_pureData);
            _targets = new List<IEnumerable<DataElement>>();

            foreach (var generator in generators)
                _targets.Add(generator.CreateData());
        }
    }

    public interface ITargetsGeneratorFactory
    {
        IEnumerable<ITargetGenerator> GetTargetGenerators(PureData pureData);
    }
}
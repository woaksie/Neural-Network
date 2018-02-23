using System;
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
        private IList<IList<DataElement>> _inputs;
        private IList<IList<DataElement>> _targets;
        private IList<DataSet> _unTargeted;

        public DataPrepper(PureData pureData, IInputsGeneratorFactory igf, ITargetsGeneratorFactory tgf)
        {
            _pureData = pureData;
            _igf = igf;
            _tgf = tgf;
        }

        public IList<DataSet> UnTargeted => _unTargeted;

        public IList<DataSet> GetTrainingInputData()
        {
            CalculateInputs();
            CalculateTargets();

            return CollateData().ToArray();
        }

        private void CalculateInputs()
        {
            IEnumerable<IInputGenerator> generators = _igf.GetInputGenerators(_pureData);
            _inputs = new List<IList<DataElement>>();

            foreach (var generator in generators)
                _inputs.Add(generator.CreateData().ToArray());
        }

        private void CalculateTargets()
        {
            IEnumerable<ITargetGenerator> generators = _tgf.GetTargetGenerators(_pureData);
            _targets = new List<IList<DataElement>>();

            foreach (var generator in generators)
                _targets.Add(generator.CreateData().ToArray());
        }

        private IEnumerable<DataSet> CollateData()
        {
            if (_inputs.Count == 0)
                return Enumerable.Empty<DataSet>();
            if (_targets.Count == 0)
                return Enumerable.Empty<DataSet>();

            IList<DataElement> source = _pureData.GetData();

            DateTime today = source.Last().Key.Date;
            DateTime first = _inputs.Select(i => i.Min(x => x.Key)).Max(y => y).Date;
            DateTime last = _targets.Select(t => t.Max(x => x.Key)).Min(y => y).Date;

            var targeted = new List<DataSet>();

            while (first <= last)
            {
                var sourceData = source.First(s => s.Key.Date == first);
                double close = sourceData.Values[(int) StockIndex.Close];

                IList<double> inputs = new List<double>();
                foreach (var input in _inputs)
                foreach (var d in input.First(i => i.Key.Date == first).Values)
                    inputs.Add(d/close);

                IList<double> targets = new List<double>();
                foreach (var target in _targets)
                foreach (var d in target.First(i => i.Key.Date == first).Values)
                    targets.Add(d/close);

                targeted.Add(new DataSet(inputs.ToArray(), targets.ToArray()));

                first = first.AddDays(1).Date;
                while (source.FirstOrDefault(d => d.Key.Date == first) == null && first <= last)
                    first = first.AddDays(1).Date;
            }

            _unTargeted = new List<DataSet>();

            while (first <= today)
            {
                var sourceData = source.First(s => s.Key.Date == first);
                double close = sourceData.Values[(int)StockIndex.Close];

                IList<double> inputs = new List<double>();
                foreach (var input in _inputs)
                foreach (var d in input.First(i => i.Key.Date == first).Values)
                    inputs.Add(d / close);

                UnTargeted.Add(new DataSet(inputs.ToArray(), Enumerable.Empty<double>().ToArray()));

                first = first.AddDays(1).Date;
                while (source.FirstOrDefault(d => d.Key.Date == first) == null && first <= today)
                    first = first.AddDays(1).Date;
            }

            return FixTargets(targeted);
        }

        private IEnumerable<DataSet> FixTargets(List<DataSet> data)
        {
            var num = data[0].Targets.Length;

            var results = new List<List<double>>();

            for (int i = 0; i < num; i++)
            {
                var mx = data.Max(d => d.Targets[i]);
                var mn = data.Min(d => d.Targets[i]);
                if (mn < 0)
                    mn = 0;
                var step = (mx - mn) / 3;

                for (int j = 0; j < data.Count; j++)
                {
                    if (i == 0)
                        results.Add(new List<double>());

                    double d = data[j].Targets[i];

                    if (d > (mx - step))
                    {
                        results[j].Add(1);
                        results[j].Add(0);
                        results[j].Add(0);
                    }
                    else if (d > (mx - (2*step)))
                    {
                        results[j].Add(0);
                        results[j].Add(1);
                        results[j].Add(0);
                    }
                    else if (d >= mn)
                    {
                        results[j].Add(0);
                        results[j].Add(0);
                        results[j].Add(1);
                    }
                    else
                    {
                        results[j].Add(0);
                        results[j].Add(0);
                        results[j].Add(0);
                    }
                }
            }

            for (int i = 0; i < data.Count; i++)
                data[i].Targets = results[i].ToArray();

            return data;
        }
    }
}
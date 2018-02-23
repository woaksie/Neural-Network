using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using NeuralNetwork.Database;
using NeuralNetwork.DataPrep;
using NeuralNetwork.NetworkModels;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class TopLevelTests
    {
        [Test]
        public void InitalCall()
        {
            PureData pureData = new PureData(new List<DataElement>());
            IInputsGeneratorFactory inputsFactory = A.Fake<IInputsGeneratorFactory>();
            ITargetsGeneratorFactory targetsFactory = A.Fake<ITargetsGeneratorFactory>();

            A.CallTo(() => inputsFactory.GetInputGenerators(pureData)).Returns(Enumerable.Empty<IInputGenerator>());
            A.CallTo(() => targetsFactory.GetTargetGenerators(pureData)).Returns(Enumerable.Empty<ITargetGenerator>());

            var sut = new DataPrepper(pureData, inputsFactory, targetsFactory);

            IList<DataSet> trainingData = sut.GetTrainingInputData();

            Assert.NotNull(trainingData);
            A.CallTo(() => inputsFactory.GetInputGenerators(pureData)).MustHaveHappened();
            A.CallTo(() => targetsFactory.GetTargetGenerators(pureData)).MustHaveHappened();
        }

        [Test]
        public void CallToInputsGeneratorFactory()
        {
            PureData pureData = new PureData(new List<DataElement>());
            IInputsGeneratorFactory sut = new StockInputsGeneratorFactory(InputsType.Basic, 30);

            var generatorList = sut.GetInputGenerators(pureData);

            Assert.NotNull(generatorList);
        }

        [Test]
        public void CallToTargetsGeneratorFactory()
        {
            PureData pureData = new PureData(new List<DataElement>());
            ITargetsGeneratorFactory sut = new StockTargetsGeneratorFactory(TargetType.Profit, 30);

            var generatorList = sut.GetTargetGenerators(pureData);

            Assert.NotNull(generatorList);
        }

        [Test]
        public void MovingAverageData2()
        {
            PureData pureData = new PureData(new List<DataElement>
            {
                new DataElement(new DateTime(2018, 2, 19), new[] {1.1D, 2.1D, 3.1D, 4.1D, 5.1D}),
                new DataElement(new DateTime(2018, 2, 20), new[] {1.2D, 2.2D, 3.2D, 4.2D, 5.2D}),
                new DataElement(new DateTime(2018, 2, 21), new[] {1.3D, 2.3D, 3.3D, 4.3D, 5.3D}),
                new DataElement(new DateTime(2018, 2, 21), new[] {1.4D, 2.4D, 3.4D, 4.4D, 5.4D})
            });

            var sut = new MovingAverage(2, pureData, StockIndex.Close);

            DataElement[] inputData = sut.CreateData().ToArray();

            Assert.NotNull(inputData);
            Assert.AreEqual(3, inputData.Length);
            Assert.IsTrue(Math.Abs(2.15D - inputData[0].Values[0]) < 0.00000001);
            Assert.IsTrue(Math.Abs(2.25D - inputData[1].Values[0]) < 0.00000001);
            Assert.AreEqual(pureData.GetData()[1].Key, inputData[0].Key);
            Assert.AreEqual(pureData.GetData()[2].Key, inputData[1].Key);
            Assert.AreEqual(pureData.GetData()[3].Key, inputData[2].Key);
        }

        [Test]
        public void MovingAverageData3()
        {
            PureData pureData = new PureData(new List<DataElement>
            {
                new DataElement(new DateTime(2018, 2, 19), new[] {1.1D, 2.1D, 3.1D, 4.1D, 5.1D}),
                new DataElement(new DateTime(2018, 2, 20), new[] {1.2D, 2.2D, 3.2D, 4.2D, 5.2D}),
                new DataElement(new DateTime(2018, 2, 21), new[] {1.3D, 2.3D, 3.3D, 4.3D, 5.3D}),
                new DataElement(new DateTime(2018, 2, 21), new[] {1.4D, 2.4D, 3.4D, 4.4D, 5.4D})
            });

            var sut = new MovingAverage(3, pureData, StockIndex.High);

            DataElement[] inputData = sut.CreateData().ToArray();

            Assert.NotNull(inputData);
            Assert.AreEqual(2, inputData.Length);
            Assert.IsTrue(Math.Abs(3.2D - inputData[0].Values[0]) < 0.00000001, $"{inputData[0].Values[0]}");
            Assert.IsTrue(Math.Abs(3.3D - inputData[1].Values[0]) < 0.00000001, $"{inputData[1].Values[0]}");
        }

        [Test]
        public void BestProfitPeriod2Data()
        {
            PureData pureData = new PureData(new List<DataElement>
            {
                new DataElement(new DateTime(2018, 2, 19), new[] {1.1D, 2.1D, 3.1D, 4.1D, 5.1D}),
                new DataElement(new DateTime(2018, 2, 20), new[] {1.2D, 2.2D, 3.2D, 4.2D, 5.2D}),
                new DataElement(new DateTime(2018, 2, 21), new[] {1.3D, 2.3D, 3.3D, 4.3D, 5.3D}),
                new DataElement(new DateTime(2018, 2, 21), new[] {1.4D, 2.4D, 3.4D, 4.4D, 5.4D})
            });

            var sut = new BestProfit(2, pureData, StockIndex.High);

            DataElement[] inputData = sut.CreateData().ToArray();

            Assert.NotNull(inputData);
            Assert.AreEqual(2, inputData.Length);
            Assert.IsTrue(Math.Abs(3.3D - inputData[0].Values[0]) < 0.00000001, $"{inputData[0].Values[0]}");
            Assert.IsTrue(Math.Abs(3.4D - inputData[1].Values[0]) < 0.00000001, $"{inputData[1].Values[0]}");
            Assert.AreEqual(pureData.GetData()[0].Key, inputData[0].Key);
            Assert.AreEqual(pureData.GetData()[1].Key, inputData[1].Key);
        }

        [Test]
        public void PuttingItAllTogether()
        {
            PureData pureData = new PureData(new List<DataElement>
            {
                new DataElement(new DateTime(2018, 2, 16), new[] {1.1D, 2.1D, 3.1D, 4.1D, 5.1D}),
                new DataElement(new DateTime(2018, 2, 20), new[] {1.2D, 2.2D, 3.2D, 4.2D, 5.2D}),
                new DataElement(new DateTime(2018, 2, 21), new[] {1.3D, 2.3D, 3.3D, 4.3D, 5.3D}),
                new DataElement(new DateTime(2018, 2, 22), new[] {1.4D, 2.4D, 3.4D, 4.4D, 5.4D}),
                new DataElement(new DateTime(2018, 2, 23), new[] {1.5D, 2.5D, 3.5D, 4.5D, 5.5D}),
                new DataElement(new DateTime(2018, 2, 26), new[] {1.6D, 2.6D, 3.6D, 4.6D, 5.6D}),
                new DataElement(new DateTime(2018, 2, 27), new[] {1.7D, 2.7D, 3.7D, 4.7D, 5.7D}),
                new DataElement(new DateTime(2018, 2, 28), new[] {1.8D, 2.8D, 3.8D, 4.8D, 5.8D}),
                new DataElement(new DateTime(2018, 3, 1), new[] {1.9D, 2.9D, 3.9D, 4.9D, 5.9D}),
            });

            var sut = new DataPrepper(pureData, new StockInputsGeneratorFactory(InputsType.Basic, 3), new StockTargetsGeneratorFactory(TargetType.Profit, 2));

            IList<DataSet> trainingData = sut.GetTrainingInputData();

            Assert.NotNull(trainingData);

            Assert.AreEqual(5, trainingData.Count);

            IList<DataSet> unTargeted = sut.UnTargeted;

            Assert.NotNull(unTargeted);

            Assert.AreEqual(2, unTargeted.Count);
        }
    }
}

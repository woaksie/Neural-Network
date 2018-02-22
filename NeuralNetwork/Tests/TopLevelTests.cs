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
            IInputsGeneratorFactory sut = new StockInputsGeneratorFactory(InputsType.Basic);

            var generatorList = sut.GetInputGenerators(pureData);

            Assert.NotNull(generatorList);
        }

        [Test]
        public void MovingAverageData()
        {
            PureData pureData = new PureData(new List<DataElement>
            {
                new DataElement(new DateTime(2018, 2, 19), new[] {1.1D, 2.1D, 3.1D, 4.1D, 5.1D}),
                new DataElement(new DateTime(2018, 2, 20), new[] {1.2D, 2.2D, 3.2D, 4.2D, 5.2D}),
                new DataElement(new DateTime(2018, 2, 21), new[] {1.3D, 2.3D, 3.3D, 4.3D, 5.3D}),
                new DataElement(new DateTime(2018, 2, 21), new[] {1.4D, 2.4D, 3.4D, 4.4D, 5.4D})
            });

            var sut = new MovingAverage(2, pureData);

            DataElement[] inputData = sut.CreateData().ToArray();

            Assert.NotNull(inputData);
            Assert.AreEqual(3, inputData.Length);
            Assert.IsTrue(Math.Abs(2.15D - inputData[0].Values[0]) < 0.00000001);
            Assert.IsTrue(Math.Abs(2.25D - inputData[1].Values[0]) < 0.00000001);
        }
    }
}

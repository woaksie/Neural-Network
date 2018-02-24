using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApprovalTests;
using ApprovalTests.Combinations;
using ApprovalTests.Reporters;
using FakeItEasy;
using NeuralNetwork.Database;
using NeuralNetwork.DataPrep;
using NeuralNetwork.NetworkModels;
using NUnit.Framework;

// this is clever, use it.
//             CombinationApprovals.VerifyAllCombinations(string.Join, new[] { ",", "-" }, new[] { new[] { "John", "Woakes" } });



namespace Tests
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
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
            var sut = new DataPrepper(HelperFunctions.GeneratePureData(),
                new StockInputsGeneratorFactory(InputsType.Basic, 3),
                new StockTargetsGeneratorFactory(TargetType.Profit, 2));

            IList<DataSet> trainingData = sut.GetTrainingInputData();

            Approvals.VerifyAll(trainingData, "DataSet" , a => string.Join(",", a.Targets) + " and " + string.Join(", ", a.SourceData));
        }

        [Test]
        public void PuttingItAllTogetherForTestData()
        {
            var sut = new DataPrepper(HelperFunctions.GeneratePureData(),
                new StockInputsGeneratorFactory(InputsType.Basic, 3),
                new StockTargetsGeneratorFactory(TargetType.Profit, 2));

            sut.GetTrainingInputData();

            IList<DataSet> unTargeted = sut.UnTargeted;

            Approvals.VerifyAll(unTargeted, "DataSet", a =>
                $" targets {string.Join(", ", a.Targets)} inputs {string.Join(", ", a.SourceData)}");
        }

        [Test]
        public void TestPureData()
        {
            Approvals.VerifyAll(HelperFunctions.GeneratePureData().GetData(),
                "Element",
                e => $" {e.Key.Date} values {string.Join(", ", e.Values)}");
        }

        [Test]
        public void TestBigPureData()
        {
            Approvals.VerifyAll(HelperFunctions.GenerateBigPureData().GetData(),
                "Element",
                e => $" {e.Key.Date} values {string.Join(", ", e.Values)}");
        }

        [Test]
        public void GenerateBigTestData()
        {
            var sut = new DataPrepper(HelperFunctions.GenerateBigPureData(),
                new StockInputsGeneratorFactory(InputsType.Full),
                new StockTargetsGeneratorFactory(TargetType.Profit, 50));

            IList<DataSet> trainingData = sut.GetTrainingInputData();

            Approvals.VerifyAll(trainingData, "DataSet", a => string.Join(",", a.Targets) + " and " + string.Join(", ", a.SourceData));
        }
    }
}

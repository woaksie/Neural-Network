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
    }
}

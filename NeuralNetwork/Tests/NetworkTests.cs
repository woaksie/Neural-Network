using System;
using System.Collections.Generic;
using NeuralNetwork.NetworkModels;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class NetworkTests
    {
        [Test]
        public void MakeANetwork()
        {
            var sut = GetNewNetwork();
            
            Assert.NotNull(sut);
        }

        [Test]
        public void TrainAndTestANetwork()
        {
            var sut = GetNewNetwork();
            sut.Train(GenerateData(), 0.00000001D);

            var testResults = sut.Compute(0D, 1D, 0D);

            foreach (var testResult in testResults)
            {
                Console.Write($" {testResult} ");
            }
        }

        private static Network GetNewNetwork()
        {
            return new Network(3, new[] {5, 5, 5}, 2);
        }

        private static List<DataSet> GenerateData()
        {
            var temp = new List<DataSet>();

            for (int i = 0; i < 8; i++)
                temp.Add(new DataSet(GetValues(i), GetTargets(i)));

            return temp;
        }

        private static double[] GetValues(int i)
        {
            switch (i)
            {
                case 0:
                    return new[] {0D, 0D, 0D};
                case 1:
                    return new[] {0D, 0D, 1D};
                case 2:
                    return new[] {0D, 1D, 0D};
                case 3:
                    return new[] {0D, 1D, 1D};
                case 4:
                    return new[] {1D, 0D, 0D};
                case 5:
                    return new[] {1D, 0D, 1D};
                case 6:
                    return new[] {1D, 1D, 0D};
                default:
                    return new[] {1D, 1D, 1D};
            }
        }

        private static double[] GetTargets(int i)
        {
            switch (i)
            {
                case 0:
                    return new[] { 0D, 1D };
                case 1:
                    return new[] { 0D, 1D };
                case 2:
                    return new[] { 0D, 1D };
                case 3:
                    return new[] { 1D, 0D };
                case 4:
                    return new[] { 0D, 1D };
                case 5:
                    return new[] { 1D, 0D };
                case 6:
                    return new[] { 1D, 0D };
                default:
                    return new[] { 1D, 0D };
            }
        }
    }
}

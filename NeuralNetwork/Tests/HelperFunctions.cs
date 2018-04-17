using System;
using System.Collections.Generic;
using NeuralNetwork.Database;

namespace Tests
{
    static internal class HelperFunctions
    {
        public static PureData GeneratePureData()
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
            return pureData;
        }

        public static PureData GenerateBigPureData()
        {
            var dataElements = new List<DataElement>();
            var startDate = new DateTime(2010,1,1).Date;

            rdm = new Random(5);

            for (int i = 0; i < 1000; i++)
            {
                dataElements.Add(new DataElement(startDate.AddDays(i).Date, GetInputValues(i)));
            }

            PureData pureData = new PureData(dataElements);
            return pureData;
        }

        private static Random rdm;

        private static double[] GetInputValues(int i)
        {
            var baseValue = GetBaseValue(i);

            var high = baseValue + rdm.NextDouble();
            var low = baseValue - rdm.NextDouble();
            var close = low + ((high - low) * rdm.NextDouble());
            var open = low + ((high - low) * rdm.NextDouble());

            return new[] { Truncate(open), Truncate(close), Truncate(high), Truncate(low)};
        }

        private static double Truncate(double i)
        {
            return Math.Truncate(i * 100D) / 100D;
        }

        private static double GetBaseValue(int i)
        {
            var v = i % 200;
            return (Math.Pow(v, 2) + (-200 * v) + 11111) / 100;
        }
    }
}
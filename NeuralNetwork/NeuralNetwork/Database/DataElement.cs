using System;

namespace NeuralNetwork.Database
{
    public class DataElement
    {
        private DateTime _key;
        private double[] _values;

        public DataElement(DateTime key, double[] values)
        {
            Key = key;
            _values = values;
        }

        public double[] Values => _values;

        public DateTime Key
        {
            get { return _key; }
            set { _key = value.Date; }
        }
    }
}
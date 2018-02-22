using System;

namespace NeuralNetwork.Database
{
    public class DataElement
    {
        private DateTime _key;
        private double[] _values;

        public DataElement(DateTime key, double[] values)
        {
            this.Key = key;
            this._values = values;
        }

        public double[] Values
        {
            get { return _values; }
        }

        public DateTime Key
        {
            get { return _key; }
            set { _key = value; }
        }
    }
}
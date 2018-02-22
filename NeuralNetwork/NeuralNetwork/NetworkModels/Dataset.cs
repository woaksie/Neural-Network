using System.Collections.Generic;

namespace NeuralNetwork.NetworkModels
{
	public class DataSet
	{
	    private double[] Values { get; set; }
		public double[] Targets { get; set; }

	    public IEnumerable<double> SourceData
	    {
	        get
	        {
	            foreach (var value in Values)
	                yield return value;
	        }
	    }

	    public DataSet(double[] values, double[] targets)
		{
			Values = values;
			Targets = targets;
		}
	}
}
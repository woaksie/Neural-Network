using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using NeuralNetwork.Database;
using NeuralNetwork.DataPrep;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Tests
{
    [TestFixture]
    public class FindBestBuySell
    {
        [Test]
        public void LoadData()
        {
            var ds = SQLDatabase.Instance.ReadData("QTEC").GetData();

            var sut = new ProfitTester(ds);

            double sellParamert = sut.BestSell();
        }
    }

    public class ProfitTester
    {
        private readonly IList<DataElement> _elements;
        private readonly int _lookAheadBars = (int) Math.Round(252D / 2D);


        public ProfitTester(IList<DataElement> elements)
        {
            _elements = elements;
        }

        /// <summary>
        /// Looking for the optimal parameters for buy and sell
        /// </summary>
        /// <returns></returns>
        public double BestSell()
        {
            double cutLossPercent = 0.01D;
            double sellPercent = 0.001D;
            double buyPercent = 0.001D;

            while (cutLossPercent < 0.08D)
            {
                while (sellPercent < 0.08D)
                {
                    while (buyPercent < 0.08D)
                    {
                        Results x = CalculateProfit(cutLossPercent, sellPercent, buyPercent);
                        StoreResults(x);

                        buyPercent += 0.001D;
                    }
                    sellPercent += 0.001D;
                }
                cutLossPercent += 0.001;
            }



        }

        // todo: logic is not done - check each variable and each conditional
        private Results CalculateProfit(double cutLossPercent, double sellPercent, double buyPercent)
        {
            var shares = 1D;
            var txn = 0;
            var cash = 0D;
            var holdShares = true;

            var day1 = _elements[0].Values;

            var open = day1[(int) StockIndex.Open];
            var high = day1[(int) StockIndex.High];
            var low = day1[(int) StockIndex.Low];

            var max = high;
            var min = low;

            var floor = open * (1D - cutLossPercent);
            var sell = open * (1D - sellPercent);
            var buy = open * (1D + buyPercent);

            foreach (var ele in _elements)
            {
                open = ele.Values[(int)StockIndex.Open];
                high = ele.Values[(int) StockIndex.High];
                low = ele.Values[(int)StockIndex.Low];

                if (holdShares)        // looking for sell signal
                    if (low < floor)   // cap loss
                    {
                        holdShares = false;
                        shares = 0;
                        txn += 1;
                        
                        if (open > floor)  // got a crack at floor value
                            cash = floor * shares;
                        else               // take open price 
                            cash = open * shares;
                    }
                    else  // check sell level
                    {
                        if (low < sell)   // time to sell
                        {
                            holdShares = false;
                            shares = 0;
                            txn += 1;

                            if (open > sell)   // got sell price
                                cash = sell * shares;
                            else               // have to make do with open price
                                cash = open * shares;
                        }
                        else  // still looking good to hold
                        {
                            if (high > max)
                                sell = high * (1D - sellPercent);
                        }
                    }
                else      // looking for buy signal
                {
                    if (high > buy)  // time to buy
                    {
                        holdShares = true;
                        if (open > buy)   // have to go with open price
                            shares = open * cash;
                        else              // can buy at buy price
                            shares = buy * cash;
                    }
                    else   // stay out of the market
                    {
                        if (low < min) // new low so adjust buy price
                        {
                            min = low;
                            buy = low * (1D + buyPercent);
                        }
                        else  // bide your time
                        {
                            // wait
                        }
                    }
                }
            }



        }

        private void StoreResults(Results results)
        {
            throw new NotImplementedException();
        }
    }

    public class Results
    {
    }
}

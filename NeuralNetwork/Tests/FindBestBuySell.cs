using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using ApprovalTests;
using ApprovalTests.Core;
using ApprovalTests.Reporters;
using NeuralNetwork.Database;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    [UseReporter(typeof(DiffReporter))]
    public class FindBestBuySell
    {
        [Test]
        public void LoadData()
        {
            var ds = SQLDatabase.Instance.ReadData("QTEC").GetData();

            var sut = new ProfitTester(ds);

            IList<Result> results = sut.BestSell();

            var result = results.OrderByDescending(r => r.Cash).ToArray();

            Approvals.Verify(result.First().Audit);
        }

        [Test]
        public void Top50ResultsAndParameters()
        {
            var ds = SQLDatabase.Instance.ReadData("QTEC").GetData();

            var sut = new ProfitTester(ds);

            IList<Result> results = sut.BestSell();

            var result = results.OrderByDescending(r => r.Cash).Take(50).ToArray();

            Approvals.Verify(Expand(result));
        }

        private string Expand(Result[] result)
        {
            StringBuilder res = new StringBuilder();
            res.AppendLine("Floor,Scrapy,Sell,Buy,Cash");

            foreach (var val in result)
                res.AppendLine($"{val.Floor},{val.Scrapy},{val.Sell},{val.Buy},{val.Cash}");

            return res.ToString();
        }
    }

    public class ProfitTester
    {
        private readonly IList<DataElement> _elements;
        private readonly int _lookAheadBars = (int) Math.Round(252D / 2D);
        private readonly List<Result> _results;
        private StringBuilder _audit;
        private readonly double _startCash;


        public ProfitTester(IList<DataElement> elements)
        {
            _elements = elements;
            _results = new List<Result>();
            _startCash = 2000D;
        }

        /// <summary>
        /// Looking for the optimal parameters for buy and sell
        /// </summary>
        /// <param name="audit"></param>
        /// <returns></returns>
        public IList<Result> BestSell()
        {
            _audit = new StringBuilder();

            double cutLossPercent = 0.0005D;
            while (cutLossPercent < 0.15D)
            {
                double sellPercent = 0.0005D;
                while (sellPercent < 0.1D)
                {
                    double buyPercent = 0.0005D;
                    while (buyPercent < 0.1D)
                    {
                        double notWorthItProfit = 0.0005D;
                        while (notWorthItProfit < 0.15D)
                        {
                            var result = CalculateProfit(cutLossPercent, sellPercent, buyPercent, notWorthItProfit);
                            StoreResults(result);

                            notWorthItProfit += 0.005D;
                        }
                        buyPercent += 0.005D;
                    }
                    sellPercent += 0.005D;
                }
                cutLossPercent += 0.005D;
            }

            return _results;
        }

        // todo: logic is not done - check each variable and each conditional
        private Result CalculateProfit(double cutLossPercent, double sellPercent, double buyPercent, double notWorthItProfit)
        {
            _audit.Clear();
            _audit.AppendLine("cutLossPercent,sellPercent,buyPercent,notWorthItProfit");
            _audit.AppendLine($"{cutLossPercent},{sellPercent},{buyPercent},{notWorthItProfit}");

            var cash = _startCash;
            var shares = 0D;
            var comm = 4.99D;

            var txn = 0;
            var holdShares = true;
            var daysIn = 0;
            var daysOut = 0;

            var day1 = _elements[0].Values;

            var open = day1[(int) StockIndex.Open];
            var high = day1[(int) StockIndex.High];
            var low = day1[(int) StockIndex.Low];

            var max = high;
            var min = low;

            var floor = open * (1D - cutLossPercent);
            var scrapy = open * (1D + notWorthItProfit);
            var sell = open * (-1D);                      // until past scrapy
            var buy = open * (1D + buyPercent);

            var temp = BuyShares(cash, open);
            shares = temp.Shares;
            cash = temp.Change;

            _audit.AppendLine("Date,open,high,low,holdShares,cash,shares,max,min,floor,scrapy,sell,buy");

            foreach (var ele in _elements)
            {
                open = ele.Values[(int)StockIndex.Open];
                high = ele.Values[(int) StockIndex.High];
                low = ele.Values[(int)StockIndex.Low];

                _audit.AppendLine($"{ele.Key.ToShortDateString()},{open},{high},{low},{holdShares},{cash},{shares},{max},{min},{floor},{scrapy},{sell},{buy}");

                if (holdShares)
                    daysIn++;
                else
                    daysOut++;

                if (holdShares)        // looking for sell signal
                    if (low < floor)   // cap loss
                    {
                        if (open > floor) // got a crack at floor value
                            cash = RoundDown((floor * shares) - comm) + cash;
                        else               // take open price 
                            cash = RoundDown((open * shares) - comm) + cash;

                        holdShares = false;
                        shares = 0;
                        txn += 1;
                        buy = low * (1D + buyPercent);
                        min = low;
                    }
                    else  // check sell level
                    {
                        if (low < sell)   // time to sell
                        {
                            if (open > sell)   // got sell price
                                cash = RoundDown((sell * shares) - comm) + cash;
                            else               // have to make do with open price
                                cash = RoundDown((open * shares) - comm) + cash;

                            holdShares = false;
                            shares = 0;
                            txn += 1;
                            buy = low * (1D + buyPercent);
                            min = low;
                        }
                        else  // still looking good to hold
                        {
                            if (high > scrapy)
                            {
                                if (high >= max)
                                {
                                    sell = high * (1D - sellPercent);
                                    max = high;
                                }
                            }
                        }
                    }
                else      // looking for buy signal
                {
                    if (high > buy)  // time to buy
                    {
                        if (open > buy)   // have to go with open price
                        {
                            temp = BuyShares(cash, open);
                            shares = temp.Shares;
                            cash = temp.Change;

                            scrapy = open * (1D + notWorthItProfit);
                            floor = open * (1D - cutLossPercent);
                            max = open;
                        }
                        else              // can buy at buy price
                        {
                            temp = BuyShares(cash, buy);
                            shares = temp.Shares;
                            cash = temp.Change;

                            scrapy = buy * (1D + notWorthItProfit);
                            floor = buy * (1D - cutLossPercent);
                            max = buy;
                        }

                        holdShares = true;
                        sell = -1D;
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

            if (holdShares)
            {
                cash = RoundDown((open * shares) - comm) + cash;
                txn += 1;
            }

            _audit.AppendLine($"Percent return per month {DoAnnualPercentage(_startCash, cash, _elements[0].Key, _elements.Last().Key)}%");

            return new Result(cash, txn, cutLossPercent, sellPercent, buyPercent, notWorthItProfit, daysIn, daysOut, _audit);
        }

        private double DoAnnualPercentage(double startCash, double cash, DateTime start, DateTime end)
        {
            double period = end.Date.Subtract(start.Date).TotalDays;
            double ratio = cash / startCash;
            return (Math.Pow(ratio, (365.25 / period)) - 1) * 100;
        }

        private Sale BuyShares(double cash, double price)
        {
            double temp = cash / price;
            var shares = (int) Math.Truncate(temp);
            var change = cash - (shares * price);
            return new Sale(shares, change);
        }

        private double RoundDown(double val)
        {
            double temp = val * 100D;
            temp = Math.Truncate(temp);
            return temp / 100D;
        }

        private void StoreResults(Result result)
        {
            _results.Add(result);
        }
    }

    internal class Sale
    {
        public int Shares { get; }
        public double Change { get; }

        public Sale(int shares, double change)
        {
            this.Shares = shares;
            this.Change = change;
        }
    }

    public class Result
    {
        private int txn;
        private double cutLossPercent;
        private double sellPercent;
        private double buyPercent;
        private double notWorthItProfit;
        private readonly int _daysIn;
        private readonly int _daysOut;
        private string _audit;

        private Result(double cash, int txn)
        {
            Cash = cash;
            this.txn = txn;
        }

        public Result(double cash, int txn, double cutLossPercent, double sellPercent, double buyPercent,
            double notWorthItProfit, int daysIn, int daysOut, StringBuilder audit) : this(cash, txn)
        {
            _audit = audit.ToString();
            this.cutLossPercent = cutLossPercent;
            this.sellPercent = sellPercent;
            this.buyPercent = buyPercent;
            this.notWorthItProfit = notWorthItProfit;
            _daysIn = daysIn;
            _daysOut = daysOut;
        }

        public double Cash { get; }
        public string Audit => _audit;
        public double Floor => cutLossPercent;
        public double Scrapy => notWorthItProfit;
        public double Sell => sellPercent;
        public double Buy => buyPercent;
    }
}

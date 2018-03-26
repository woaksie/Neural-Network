using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApprovalTests;
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
        private readonly decimal _startCash;


        public ProfitTester(IList<DataElement> elements)
        {
            _elements = elements;
            _results = new List<Result>();
            _startCash = 2000m;
        }

        /// <summary>
        /// Looking for the optimal parameters for buy and sell
        /// </summary>
        /// <param name="audit"></param>
        /// <returns></returns>
        public IList<Result> BestSell()
        {
            _audit = new StringBuilder();

            decimal cutLossPercent = 0.0005m;
            while (cutLossPercent < 0.15m)
            {
                decimal sellPercent = 0.0005m;
                while (sellPercent < 0.1m)
                {
                    decimal buyPercent = 0.0005m;
                    while (buyPercent < 0.1m)
                    {
                        decimal notWorthItProfit = 0.0005m;
                        while (notWorthItProfit < 0.15m)
                        {
                            var result = CalculateProfit(cutLossPercent, sellPercent, buyPercent, notWorthItProfit);
                            StoreResults(result);

                            notWorthItProfit += 0.005m;
                        }
                        buyPercent += 0.005m;
                    }
                    sellPercent += 0.005m;
                }
                cutLossPercent += 0.005m;
            }

            return _results;
        }

        // todo: logic is not done - check each variable and each conditional
        private Result CalculateProfit(decimal cutLossPercent, decimal sellPercent, decimal buyPercent, decimal notWorthItProfit)
        {
            _audit.Clear();
            _audit.AppendLine("cutLossPercent,sellPercent,buyPercent,notWorthItProfit");
            _audit.AppendLine($"{cutLossPercent},{sellPercent},{buyPercent},{notWorthItProfit}");

            var cash = _startCash;
            var shares = 0;
            decimal comm = 4.99m;

            var txn = 0;
            var holdShares = true;
            var daysIn = 0;
            var daysOut = 0;

            var day1 = _elements[0].Values;

            decimal open = (decimal) day1[(int) StockIndex.Open];
            decimal high = (decimal) day1[(int) StockIndex.High];
            decimal low = (decimal) day1[(int) StockIndex.Low];

            var max = high;
            var min = low;

            var floor = open * (1m - cutLossPercent);
            var scrapy = open * (1m + notWorthItProfit);
            var sell = open * (-1m);                      // until past scrapy
            var buy = open * (1m + buyPercent);

            var temp = BuyShares(cash, open);
            shares = temp.Shares;
            cash = temp.Change;

            _audit.AppendLine("Date,open,high,low,holdShares,cash,shares,max,min,floor,scrapy,sell,buy");

            foreach (var ele in _elements)
            {
                open = (decimal) ele.Values[(int)StockIndex.Open];
                high = (decimal) ele.Values[(int) StockIndex.High];
                low = (decimal) ele.Values[(int)StockIndex.Low];

                _audit.AppendLine($"{ele.Key.ToShortDateString()},{open},{high},{low},{holdShares},{cash},{shares},{max},{min},{floor},{scrapy},{sell},{buy}");

                if (holdShares)
                    daysIn++;
                else
                    daysOut++;

                if (holdShares)        // looking for sell signal
                    if (low < floor)   // cap loss
                    {
                        if (open > floor) // got a crack at floor value
                            cash = SellShares(floor, shares, comm, cash);
                        else // take open price 
                            cash = SellShares(open, shares, comm, cash);

                        holdShares = false;
                        shares = 0;
                        txn += 1;
                        buy = low * (1m + buyPercent);
                        min = low;
                    }
                    else  // check sell level
                    {
                        if (low < sell)   // time to sell
                        {
                            if (open > sell) // got sell price
                                cash = SellShares(sell, shares, comm, cash);
                            else // have to make do with open price
                                cash = SellShares(open, shares, comm, cash);

                            holdShares = false;
                            shares = 0;
                            txn += 1;
                            buy = low * (1m + buyPercent);
                            min = low;
                        }
                        else  // still looking good to hold
                        {
                            if (high > scrapy)
                            {
                                if (high >= max)
                                {
                                    sell = high * (1m - sellPercent);
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

                            scrapy = open * (1m + notWorthItProfit);
                            floor = open * (1m - cutLossPercent);
                            max = open;
                        }
                        else              // can buy at buy price
                        {
                            temp = BuyShares(cash, buy);
                            shares = temp.Shares;
                            cash = temp.Change;

                            scrapy = buy * (1m + notWorthItProfit);
                            floor = buy * (1m - cutLossPercent);
                            max = buy;
                        }

                        holdShares = true;
                        sell = -1m;
                    }
                    else   // stay out of the market
                    {
                        if (low < min) // new low so adjust buy price
                        {
                            min = low;
                            buy = low * (1m + buyPercent);
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
                cash = SellShares(open, shares, comm, cash);
                txn += 1;
            }

            _audit.AppendLine($"Percent return per month {DoAnnualPercentage(_startCash, cash, _elements[0].Key, _elements.Last().Key)}%");

            return new Result(cash, txn, cutLossPercent, sellPercent, buyPercent, notWorthItProfit, daysIn, daysOut, _audit);
        }

        private double DoAnnualPercentage(decimal startCash, decimal cash, DateTime start, DateTime end)
        {
            double period = end.Date.Subtract(start.Date).TotalDays;
            double ratio = (double) (cash / startCash);
            return (Math.Pow(ratio, (365.25D / period)) - 1D) * 100D;
        }

        private Sale BuyShares(decimal cash, decimal price)
        {
            var tPrice = price * 100m;
            tPrice = Math.Ceiling(tPrice);
            tPrice = tPrice / 100m;

            var tShares = cash / tPrice;
            var shares = (int) Math.Truncate(tShares);
            var change = cash - (shares * tPrice);

            return new Sale(shares, change);
        }

        private decimal SellShares(decimal price, int shares, decimal comm, decimal cash)
        {
            decimal temp = price * 100m;
            temp = Math.Truncate(temp);
            temp = temp / 100m;
            temp = temp * shares;
            return temp + cash - comm;
        }

        private void StoreResults(Result result)
        {
            _results.Add(result);
        }
    }

    internal class Sale
    {
        public int Shares { get; }
        public decimal Change { get; }

        public Sale(int shares, decimal change)
        {
            Shares = shares;

            var temp = change * 100m;
            temp = Math.Round(temp);
            temp = temp / 100m;
            Change = temp;
        }
    }

    public class Result
    {
        private int txn;
        private decimal cutLossPercent;
        private decimal sellPercent;
        private decimal buyPercent;
        private decimal notWorthItProfit;
        private readonly int _daysIn;
        private readonly int _daysOut;
        private string _audit;

        private Result(decimal cash, int txn)
        {
            Cash = cash;
            this.txn = txn;
        }

        public Result(decimal cash, int txn, decimal cutLossPercent, decimal sellPercent, decimal buyPercent,
            decimal notWorthItProfit, int daysIn, int daysOut, StringBuilder audit) : this(cash, txn)
        {
            _audit = audit.ToString();
            this.cutLossPercent = cutLossPercent;
            this.sellPercent = sellPercent;
            this.buyPercent = buyPercent;
            this.notWorthItProfit = notWorthItProfit;
            _daysIn = daysIn;
            _daysOut = daysOut;
        }

        public decimal Cash { get; }
        public string Audit => _audit;
        public decimal Floor => cutLossPercent;
        public decimal Scrapy => notWorthItProfit;
        public decimal Sell => sellPercent;
        public decimal Buy => buyPercent;
    }
}

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
        private static readonly string Symbol = "QTEC";  // "IAU";  "QTEC"
        private static IList<Result> _results = null;

        [Test]
        public void LoadData()
        {
            IList<Result> results = Results();

            var result = results.OrderByDescending(r => r.Cash).ToArray();

            Approvals.Verify(result.First().Audit);
        }

        [Test]
        public void Top50ResultsAndParameters()
        {
            IList<Result> results = Results();

            var result = results.OrderByDescending(r => r.Cash).Take(50).ToArray();

            Approvals.Verify(Expand(result));
        }

        private static IList<Result> Results()
        {
            if (_results != null)
                return _results;

            var sut = new ProfitTester(GetData());
            _results = sut.BestSell();

            return _results;
        }

        private static IList<DataElement> GetData()
        {
            return SQLDatabase.Instance.ReadData(Symbol).GetData();
        }

        private string Expand(Result[] result)
        {
            StringBuilder res = new StringBuilder();

            var toots = KNearest(result, res);

            res.AppendLine("Floor,Sell,Buy,Scrapy,Txns,Cash");

            foreach (var val in result)
            {
                var line = $"{val.Floor},{val.Sell},{val.Buy},{val.Scrapy},{val.Txn},{val.Cash}";
                if (line.Contains(toots))
                    line = $"{line} ****";
                res.AppendLine(line);
            }

            return res.ToString();
        }

        private string KNearest(Result[] result, StringBuilder res)
        {
            var lst = result.ToList();
            var value = KNN(result);

            for (int i = 0; i < 10; i++)
            {
                RemoveOutLier(lst, value);
                value = KNN(lst.ToArray());
            }

            var best = $"{value.Floor.ToString($"0.####")},{value.Sell.ToString($"0.####")},{value.Buy.ToString($"0.####")},{value.Scrapy.ToString($"0.####")}";
            res.AppendLine("Floor,Sell,Buy,Scrapy" + Environment.NewLine + best);
            return best;
        }

        private void RemoveOutLier(List<Result> lst, Result value)
        {
            double maxDistance = 0D;
            Result bestDetails = null;
            foreach (var res in lst)
            {
                var distance = CalcDistance(value, res.Floor, res.Scrapy, res.Sell, res.Buy);

                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    bestDetails = res;
                }
            }

            lst.Remove(bestDetails);
        }

        private Result KNN(Result[] result)
        {
            var mxFloor = result.Max(r => r.Floor) + 0.000001m;
            var mxScrapy = result.Max(r => r.Scrapy) + 0.000001m;
            var mxSell = result.Max(r => r.Sell) + 0.000001m;
            var mxBuy = result.Max(r => r.Buy) + 0.000001m;

            var mnFloor = result.Min(r => r.Floor);
            var mnScrapy = result.Min(r => r.Scrapy);
            var mnSell = result.Min(r => r.Sell);
            var mnBuy = result.Min(r => r.Buy);

            double minDistance = double.MaxValue;
            Result minDetails = null;

            for (decimal flr = mnFloor; flr <= mxFloor; flr = flr + 0.005m)
            {
                for (decimal spy = mnScrapy; spy < mxScrapy; spy = spy + 0.005m)
                {
                    for (decimal sel = mnSell; sel < mxSell; sel = sel + 0.005m)
                    {
                        for (decimal buy = mnBuy; buy < mxBuy; buy = buy + 0.005m)
                        {
                            double distance = 0;
                            double step = 1.5 / (result.Length - 1);
                            double factor = 2D;
                            foreach (var res in result)
                            {
                                distance += CalcDistance(res, flr, spy, sel, buy) * factor;
                                factor -= step;
                            }

                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                minDetails = new Result(0, 0, flr, sel, buy, spy, 0, 0, new StringBuilder());
                            }
                        }
                    }
                }
            }

            return minDetails;
        }

        private static double CalcDistance(Result res, decimal flr, decimal spy, decimal sel, decimal buy)
        {
            double temp;
            temp = Math.Pow((double) (flr - res.Floor), 2);
            temp += Math.Pow((double) (spy - res.Scrapy), 2);
            temp += Math.Pow((double) (sel - res.Sell), 2);
            temp += Math.Pow((double) (buy - res.Buy), 2);
            temp = Math.Sqrt(temp);
            return temp;
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

            decimal floor = 0.0005m;
            while (floor < 0.15m)
            {
                decimal sell = 0.0005m;
                while (sell < 0.09m)
                {
                    decimal buy = 0.0005m;
                    while (buy < 0.09m)
                    {
                        decimal scrapy = 0.0005m;
                        while (scrapy < 0.25m)
                        {
                            var result = CalculateProfit(floor, sell, buy, scrapy);
                            StoreResults(result);

                            scrapy += 0.005m;
                        }
                        buy += 0.005m;
                    }
                    sell += 0.005m;
                }
                floor += 0.005m;
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
            decimal close = (decimal) day1[(int) StockIndex.Close];

            var max = high;
            var min = low;

            var floor = open * (1m - cutLossPercent);
            var scrapy = open * (1m + notWorthItProfit);
            var sell = open * (-1m);                      // until past scrapy
            var buy = open * (1m + buyPercent);

            var temp = BuyShares(cash, open);
            shares = temp.Shares;
            cash = temp.Change;

            var heading = "Date,open,high,low,close,holdShares,cash,shares,max,min,floor,scrapy,sell,buy";
            _audit.AppendLine(heading);

            foreach (var ele in _elements)
            {
                open = (decimal) ele.Values[(int) StockIndex.Open];
                high = (decimal) ele.Values[(int) StockIndex.High];
                low = (decimal) ele.Values[(int) StockIndex.Low];
                close = (decimal) ele.Values[(int) StockIndex.Close];

                _audit.AppendLine($"{ele.Key.ToShortDateString()},{open},{high},{low},{close},{holdShares},{cash},{shares},{max},{min},{floor},{scrapy},{sell},{buy}");

                if (holdShares)
                    daysIn++;
                else
                    daysOut++;

                if (holdShares)        // looking for sell signal
                    if (low < floor)   // cap loss
                    {
                        var price = open > floor 
                            ? floor 
                            : open;
                        cash = SellShares(price, shares, comm, cash);
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
                            var price = open > sell 
                                ? sell 
                                : open;
                            cash = SellShares(price, shares, comm, cash);
                            holdShares = false;
                            shares = 0;
                            txn += 1;
                            buy = low * (1m + buyPercent);
                            min = low;
                        }
                        else  // still looking good to hold
                        {
                            if (high > scrapy)
                                if (sell < 0)  // first time over scrapy
                                {
                                    sell = high * (1m - sellPercent);
                                    max = high;
                                }
                                else if (high >= max)
                                {
                                    sell = high * (1m - sellPercent);
                                    max = high;
                                }
                        }
                    }
                else      // looking for buy signal
                {
                    if (high > buy)  // time to buy
                    {
                        var price = open > buy 
                            ? open 
                            : buy;
                        temp = BuyShares(cash, price);
                        shares = temp.Shares;
                        cash = temp.Change;

                        scrapy = price * (1m + notWorthItProfit);
                        floor = price * (1m - cutLossPercent);
                        max = high;
                        sell = -1m;
                        holdShares = true;
                    }
                    else   // stay out of the market
                    {
                        if (low < min) // new low so adjust buy price
                        {
                            min = low;
                            buy = low * (1m + buyPercent);
                        }
                        else  // bide your time
                        {}
                    }
                }
            }

            if (holdShares)
            {
                cash = SellShares(open, shares, comm, cash);
                txn += 1;
            }

            _audit.AppendLine(heading);
            _audit.AppendLine($"Percent return per month {DoAnnualPercentage(_startCash, cash, _elements[0].Key, _elements.Last().Key)}%");
            _audit.AppendLine($"Days in {daysIn}");
            _audit.AppendLine($"Days out {daysOut}");

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
            if (_results.Count > 100)
            {
                var temp = _results.OrderBy(r => r.Cash).First();
                if (temp.Cash > result.Cash)
                    return;
                _results.Remove(temp);
            }
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
        public int Txn => txn;
    }
}

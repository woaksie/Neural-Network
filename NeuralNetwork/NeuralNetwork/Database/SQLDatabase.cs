using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using NeuralNetwork.Database.Config;

namespace NeuralNetwork.Database
{
    public class SQLDatabase
    {
        private readonly string _connectionString;

        public static SQLDatabase Instance
        {
            get
            {
                var settings = new AppSettingsConfigurationSource();

                var provider = settings.Build();
                provider.Load();

                if (provider.TryGet(ConfigKeys.GetKey(ConfigKeys.KeyNames.DatabaseConnectionString), out var conStr))
                    return new SQLDatabase(conStr);

                throw new ConfigurationErrorsException("No database configuration");
            }
        }

        private SQLDatabase(string connectionString)
        {
            _connectionString = connectionString;
        }

        public PureData ReadData(string symbol)
        {
            var queryString =
                $@"SELECT DailyPrices.Date, 
                          DailyPrices.OpenPrice, 
                          DailyPrices.ClosePrice, 
                          DailyPrices.High, 
                          DailyPrices.Low, 
                          DailyPrices.Volume, 
                          DailyPrices.FiveDayClose, 
                          DailyPrices.Momentum
                   FROM Company
                       INNER JOIN DailyPrices
                           ON Company.Id = DailyPrices.CompanyId
                   WHERE (Company.Symbol = N'{symbol}')
                   order by Date;";

            var result = new List<DataElement>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                // Call Read before accessing data.
                while (reader.Read())
                {
                    result.Add(ReadSingleRow(reader));
                }

                // Call Close when done reading.
                reader.Close();
            }

            return new PureData(result);
        }

        private DataElement ReadSingleRow(SqlDataReader reader)
        {
            var key = reader.GetDateTime(0).Date;
            var open = reader.GetDecimal(1);
            var close = reader.GetDecimal(2);
            var high = reader.GetDecimal(3);
            var low = reader.GetDecimal(4);
            var vol = reader.GetInt64(5);
            var fdc = reader.GetDecimal(6);
            var mom = reader.GetDecimal(7);

            return new DataElement(key,
                new double[]
                    {(double) open, (double) close, (double) high, (double) low, vol, (double) fdc, (double) mom});
        }
    }
}

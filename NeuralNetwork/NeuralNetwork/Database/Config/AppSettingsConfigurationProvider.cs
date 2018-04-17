using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace NeuralNetwork.Database.Config
{
    public class AppSettingsConfigurationProvider : FileConfigurationProvider
    {
        public AppSettingsConfigurationProvider(AppSettingsConfigurationSource source)
            : base(source)
        {}

        public override void Load(Stream stream)
        {
            try
            {
                if (Data == null)
                    Data = new SortedDictionary<string, string>();

                var txt = new StreamReader(stream).ReadToEnd();
                if (!string.IsNullOrWhiteSpace(txt))
                    Data[ConfigKeys.GetKey(ConfigKeys.KeyNames.DatabaseConnectionString)] =
                        txt.Replace(Environment.NewLine, string.Empty);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
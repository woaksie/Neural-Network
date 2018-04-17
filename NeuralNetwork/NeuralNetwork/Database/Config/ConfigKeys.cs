using System;

namespace NeuralNetwork.Database.Config
{
    public static class ConfigKeys
    {
        public enum KeyNames
        {
            DatabaseConnectionString
        }

        public static string GetKey(KeyNames name)
        {
            switch (name)
            {
                case KeyNames.DatabaseConnectionString:
                    return "DbConStr";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
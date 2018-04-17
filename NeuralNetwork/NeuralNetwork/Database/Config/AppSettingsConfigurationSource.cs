using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace NeuralNetwork.Database.Config
{
    public class AppSettingsConfigurationSource : FileConfigurationSource
    {
        public AppSettingsConfigurationSource()
        {
            Path = @"SQL.txt";
            ReloadOnChange = true;
            Optional = false;
            FileProvider = new PhysicalFileProvider(@"D:\Projects\");
        }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new AppSettingsConfigurationProvider(this);
        }

        public IConfigurationProvider Build()
        {
            return new AppSettingsConfigurationProvider(this);
        }
    }
}
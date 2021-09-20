using Microsoft.Extensions.Configuration;

namespace NaviLogger.Extensions
{
    public static class NaviConfigurationExtension
    {
        public static string GetKafkaStrings(this IConfiguration configuration, string name)
        {
            return configuration?.GetSection("KafkaStrings")?[name];
        }
    }
}
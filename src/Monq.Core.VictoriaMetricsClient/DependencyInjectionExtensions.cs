using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Monq.Core.VictoriaMetricsClient;

public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds Victoria Metrics Http Cient. 
    /// Use <see cref="IVictoriaClientRead"/> or <see cref="IVictoriaClientWrite"/> 
    /// or <see cref="IVictoriaProxyClient"/> in services.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddVictoriaMetricsHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<VictoriaOptions>(configuration);

        var readHttpClientConfiguration = BuildConfiguration(ClusterNodeTypes.Read);
        var writeHttpClientConfiguration = BuildConfiguration(ClusterNodeTypes.Write);
        services.AddHttpClient<IVictoriaClientRead, VictoriaClientRead>(readHttpClientConfiguration);
        services.AddHttpClient<IVictoriaProxyClient, VictoriaProxyClient>(readHttpClientConfiguration);
        services.AddHttpClient<IVictoriaClientWrite, VictoriaClientWrite>(writeHttpClientConfiguration);

        return services;
    }

    static Action<IServiceProvider, HttpClient> BuildConfiguration(ClusterNodeTypes clusterNodeType)
    {
        Action<IServiceProvider, HttpClient> configurationFunc = (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<VictoriaOptions>>();
            var victoriaOptions = options.Value;

            if (victoriaOptions is null)
                throw new StorageConfigurationException("There is not configuration found for the VictoriaMetrics.");

            if (victoriaOptions.IsCluster)
                client.ConfigureVictoriaMetricsAsCluster(victoriaOptions, clusterNodeType);
            else
                client.ConfigureVictoriaMetricsAsSingle(victoriaOptions);

            if (victoriaOptions.AuthenticationType == AuthenticationTypes.BasicAuth)
                client.ConfigureVictoriaMetricsWithBasicAuth(victoriaOptions);

            if (victoriaOptions.UseHttpV2)
                client.DefaultRequestVersion = new Version(2, 0);

            if (clusterNodeType == ClusterNodeTypes.Write)
            {
                client.DefaultRequestHeaders.Add("X-Prometheus-Remote-Write-Version", "0.1.0");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-protobuf"));
            }
        };
        return configurationFunc;
    }

    static void ConfigureVictoriaMetricsAsCluster(this HttpClient client, VictoriaOptions options, ClusterNodeTypes clusterNodeType)
    {
        if (string.IsNullOrEmpty(options.ClusterInsertUri))
            throw new StorageConfigurationException("""You must specify the "clusterInsertUri" configuration property.""");
        if (string.IsNullOrEmpty(options.ClusterSelectUri))
            throw new StorageConfigurationException("""You must specify the "clusterSelectUri" configuration property.""");

        const string multitenant = "multitenant";
        string accountId = string.Empty;

        if (options.ClusterAccountId == multitenant)
            accountId = multitenant;
        else
            accountId = $"{options.ClusterAccountId ?? "0"}";

        switch (clusterNodeType)
        {
            case ClusterNodeTypes.Read:
                client.BaseAddress = new Uri(new Uri(options.ClusterSelectUri),
                    $"select/{accountId}/prometheus/api/v1/");
                break;
            case ClusterNodeTypes.Write:
                client.BaseAddress = new Uri(new Uri(options.ClusterInsertUri),
                    $"insert/{accountId}/prometheus/api/v1/");
                break;
        }
    }

    static void ConfigureVictoriaMetricsAsSingle(this HttpClient client, VictoriaOptions options)
    {
        if (string.IsNullOrEmpty(options.Uri))
            throw new StorageConfigurationException("""You must specify the "uri" configuration property.""");

        client.BaseAddress = new Uri(new Uri(options.Uri), "prometheus/api/v1/");
    }

    static void ConfigureVictoriaMetricsWithBasicAuth(this HttpClient client, VictoriaOptions options)
    {
        if (string.IsNullOrEmpty(options.BasicAuthUsername))
            throw new StorageConfigurationException("""You must specify the "basicAuthUsername" configuration property.""");
        if (string.IsNullOrEmpty(options.BasicAuthPassword))
            throw new StorageConfigurationException("""You must specify the "basicAuthPassword" configuration property.""");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes($"{options.BasicAuthUsername}:{options.BasicAuthPassword}")));
    }

    enum ClusterNodeTypes
    {
        Read,
        Write
    }
}

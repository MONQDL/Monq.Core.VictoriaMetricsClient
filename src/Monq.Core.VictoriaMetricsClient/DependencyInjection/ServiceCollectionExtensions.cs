using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Monq.Core.VictoriaMetricsClient;
using Monq.Core.VictoriaMetricsClient.Exceptions;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Victoria Metrics Http Client.
    /// Use <see cref="IVictoriaClientRead"/> or <see cref="IVictoriaClientWrite"/> 
    /// or <see cref="IVictoriaProxyClient"/> in services.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("Configuration binding requires unreferenced code")]
    public static IServiceCollection AddVictoriaMetricsHttpClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<VictoriaOptions>(configuration);

        var readHttpClientConfiguration = BuildConfiguration(ClusterNodeTypes.Read, _ => { });
        var writeHttpClientConfiguration = BuildConfiguration(ClusterNodeTypes.Write, _ => { });
        services.AddHttpClient<IVictoriaClientRead, VictoriaClientRead>(readHttpClientConfiguration);
        services.AddHttpClient<IVictoriaProxyClient, VictoriaProxyClient>(readHttpClientConfiguration);
        services.AddHttpClient<IVictoriaClientWrite, VictoriaClientWrite>(writeHttpClientConfiguration);

        return services;
    }

    /// <summary>
    /// Adds Victoria Metrics Http Client.
    /// Use <see cref="IVictoriaClientRead"/> or <see cref="IVictoriaClientWrite"/> 
    /// or <see cref="IVictoriaProxyClient"/> in services.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="configureHttpClient"></param>
    /// <returns></returns>
    [RequiresUnreferencedCode("Configuration binding requires unreferenced code")]
    public static IServiceCollection AddVictoriaMetricsHttpClient(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<HttpClient> configureHttpClient)
    {
        services.Configure<VictoriaOptions>(configuration);

        var readHttpClientConfiguration = BuildConfiguration(ClusterNodeTypes.Read, configureHttpClient);
        var writeHttpClientConfiguration = BuildConfiguration(ClusterNodeTypes.Write, configureHttpClient);
        services.AddHttpClient<IVictoriaClientRead, VictoriaClientRead>(readHttpClientConfiguration);
        services.AddHttpClient<IVictoriaProxyClient, VictoriaProxyClient>(readHttpClientConfiguration);
        services.AddHttpClient<IVictoriaClientWrite, VictoriaClientWrite>(writeHttpClientConfiguration);

        return services;
    }

    static Action<IServiceProvider, HttpClient> BuildConfiguration(
        ClusterNodeTypes clusterNodeType,
        Action<HttpClient> configureHttpClient)
        => (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<VictoriaOptions>>();
            var victoriaOptions = options.Value ?? throw new StorageConfigurationException("There is not configuration found for the VictoriaMetrics.");

            configureHttpClient(client);

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

    static void ConfigureVictoriaMetricsAsCluster(this HttpClient client, VictoriaOptions options, ClusterNodeTypes clusterNodeType)
    {
        if (string.IsNullOrEmpty(options.ClusterInsertUri))
            throw new StorageConfigurationException("""You must specify the "clusterInsertUri" configuration property.""");
        if (string.IsNullOrEmpty(options.ClusterSelectUri))
            throw new StorageConfigurationException("""You must specify the "clusterSelectUri" configuration property.""");

        const string multitenant = "multitenant";
        string? accountId;

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

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.UTF8.GetBytes($"{options.BasicAuthUsername}:{options.BasicAuthPassword}")));
    }

    enum ClusterNodeTypes
    {
        Read,
        Write
    }
}

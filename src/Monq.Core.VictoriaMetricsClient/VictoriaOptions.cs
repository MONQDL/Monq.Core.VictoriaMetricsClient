namespace Monq.Core.VictoriaMetricsClient;

/// <summary>
/// VictoriaMetrics client connection options.
/// </summary>
public class VictoriaOptions
{
    /// <summary>
    /// VictoriaMetrics instance URI. This property will be used if <see cref="IsCluster"/> is false.
    /// </summary>
    public string? Uri { get; set; }

    /// <summary>
    /// If true - then you must specify the <see cref="ClusterSelectUri"/> and <see cref="ClusterInsertUri"/> properties.
    /// If false - then you must specify the <see cref="Uri"/> property.
    /// </summary>
    public bool IsCluster { get; set; } = false;

    /// <summary>
    /// Is an arbitrary 32-bit integer identifying namespace for data ingestion (aka tenant). 
    /// It is possible to set it as accountID:projectID, where projectID is also arbitrary 32-bit integer. 
    /// If projectID isn't set, then it equals to 0. 
    /// See multitenancy docs for more details. 
    /// The "accountID" can be set to multitenant string, e.g. http://{vminsert}:8480/insert/multitenant/{suffix}.
    /// </summary>
    public string? ClusterAccountId { get; set; } = "0";

    /// <summary>
    /// The cluster "select" node URI. This property will be used if <see cref="IsCluster"/> is true.
    /// </summary>
    public string? ClusterSelectUri { get; set; }

    /// <summary>
    /// The cluster "insert" node URI. This property will be used if <see cref="IsCluster"/> is true.
    /// </summary>
    public string? ClusterInsertUri { get; set; }

    /// <summary>
    /// The authentication auth type.
    /// </summary>
    public AuthenticationTypes AuthenticationType { get; set; } = AuthenticationTypes.BasicAuth;

    /// <summary>
    /// The username of BasicAuth authentication mechanism.
    /// This property will be used if <see cref="AuthenticationType"/> is <see cref="AuthenticationTypes.BasicAuth"/>.
    /// </summary>
    public string? BasicAuthUsername { get; set; }

    /// <summary>
    /// The password of BasicAuth authentication mechanism.
    /// This property will be used if <see cref="AuthenticationType"/> is <see cref="AuthenticationTypes.BasicAuth"/>.
    /// </summary>
    public string? BasicAuthPassword { get; set; }

    /// <summary>
    /// If true, then use the http v2 connection protocol.
    /// </summary>
    public bool UseHttpV2 { get; set; } = true;

    /// <summary>
    /// Prefix for system labels.
    /// </summary>
    public string SystemLabelPrefix { get; set; } = "monq_";
}

/// <summary>
/// Authentication type.
/// </summary>
public enum AuthenticationTypes
{
    /// <summary>
    /// None.
    /// </summary>
    None,
    /// <summary>
    /// Basic authentication.
    /// </summary>
    BasicAuth
}

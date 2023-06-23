namespace CommonGIS
{
    /// <summary>Tunnel categories as specified in ADR convention.</summary>
    /// The "A" category is missing as this type is used to mark frobidden tunnel classes for transported goods
    /// and an "A" tunnel has no restrictions.
    public enum TunnelCategory
    {   /// <summary>The route can pass any tunnels.</summary>
        NoRestriction = 0,
        E = 1,
        D = 2,
        C = 3,
        B = 4
    }
}

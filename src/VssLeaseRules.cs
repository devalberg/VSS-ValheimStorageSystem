namespace VSS;
internal static class VssLeaseRules
{
    public static bool Active(long until, long now) => until > now;
    public static bool CanCommit(bool owner, string storedToken, string requestedToken, long until, long now) =>
        owner && !string.IsNullOrEmpty(requestedToken) && storedToken == requestedToken && Active(until, now);
}

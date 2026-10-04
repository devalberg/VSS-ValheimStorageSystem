using UnityEngine;

namespace VSS;
internal static class VssSettings
{
    private static ZRoutedRpc _registered;
    private static float _radius = 20f;
    private static float _nextRequest;
    private static bool _versionRejected;
    public static bool Ready { get; private set; }
    public static float Radius => ZNet.instance != null && ZNet.instance.IsServer() ? VssPlugin.ModConfig.Radius.Value : _radius;
    public static void Initialize()
    {
        Ready = ZNet.instance != null && ZNet.instance.IsServer();
        _versionRejected = false;
        _nextRequest = 0;
        var rpc = ZRoutedRpc.instance;
        if (rpc == null) return;
        if (_registered != rpc)
        {
            _registered = rpc;
            rpc.Register("VSS_RequestSettings", Request);
            rpc.Register<float, string>("VSS_Settings", Receive);
        }
        Tick();
    }
    public static void Tick()
    {
        if (ZNet.instance == null || ZNetScene.instance == null || _registered == null || _registered != ZRoutedRpc.instance) return;
        if (ZNet.instance.IsServer()) { Ready = true; return; }
        if (Ready || _versionRejected || Time.unscaledTime < _nextRequest) return;
        var peer = ZNet.instance.GetServerPeer();
        if (peer == null) return;
        _nextRequest = Time.unscaledTime + 3f;
        _registered.InvokeRoutedRPC(peer.m_uid, "VSS_RequestSettings");
    }
    private static void Request(long sender)
    {
        if (ZNet.instance != null && ZNet.instance.IsServer())
            ZRoutedRpc.instance.InvokeRoutedRPC(sender, "VSS_Settings", Radius, VssPlugin.ModVersion);
    }
    private static void Receive(long sender, float radius, string version)
    {
        if (ZNet.instance == null || ZNet.instance.IsServer() || ZNet.instance.GetServerPeer() == null || sender != ZNet.instance.GetServerPeer().m_uid) return;
        Ready = version == VssPlugin.ModVersion && !float.IsNaN(radius) && radius >= 1 && radius <= 100;
        if (Ready) _radius = radius;
        else { _versionRejected = true; VssPlugin.Log.LogWarning("VSS requires the same version on the server and every player."); }
    }
    public static void Broadcast()
    {
        if (ZNet.instance != null && ZNet.instance.IsServer() && ZRoutedRpc.instance != null)
            ZRoutedRpc.instance.InvokeRoutedRPC(ZNetView.Everybody, "VSS_Settings", Radius, VssPlugin.ModVersion);
    }
}

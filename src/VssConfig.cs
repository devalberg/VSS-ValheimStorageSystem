using BepInEx.Configuration;

namespace VSS;
public sealed class VssConfig
{
    public ConfigEntry<bool> RegisterStand { get; }
    public ConfigEntry<float> Radius { get; }
    public ConfigEntry<float> RefreshSeconds { get; }
    public VssConfig(ConfigFile config)
    {
        RegisterStand = config.Bind("General", "RegisterStand", true, "Register the Storage Inventory pedestal in the hammer Furniture tab.");
        Radius = config.Bind("Storage", "RadiusMeters", 20f, new ConfigDescription(
            "Chest radius in metres. In multiplayer the host/server value is used by everyone.", new AcceptableValueRange<float>(1f, 100f)));
        RefreshSeconds = config.Bind("UI", "RefreshSeconds", 0.75f, new ConfigDescription(
            "Storage display refresh interval.", new AcceptableValueRange<float>(0.25f, 5f)));
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace VSS;
public sealed class VssAccessStand : MonoBehaviour, Interactable, Hoverable
{
    internal static readonly HashSet<VssAccessStand> Instances = new();
    private void Awake()
    {
        var view = GetComponent<ZNetView>();
        if (view != null && view.IsValid()) Instances.Add(this);
    }
    private void OnDestroy() => Instances.Remove(this);
    private void Start()
    {
        var view = GetComponent<ZNetView>();
        if (view != null && view.IsValid()) Instances.Add(this);
    }
    public string GetHoverText() => Localization.instance.Localize("Storage Inventory\n[<color=yellow><b>$KEY_Use</b></color>] Open storage");
    public string GetHoverName() => "Storage Inventory";
    public float GetHoverOffset() => 0.75f;
    public bool Interact(Humanoid user, bool hold, bool alt)
    {
        if (hold || !(user is Player player) || player != Player.m_localPlayer) return false;
        if (!PrivateArea.CheckAccess(transform.position, 0f, true, false)) return true;
        VssUi.Open(player, this);
        return true;
    }
    public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;
    internal static bool HasWardAccess(long playerId, Vector3 point)
    {
        // PrivateArea.CheckAccess uses the LOCAL player. On an owner peer, check the requesting ID explicitly.
        var areas = UnityEngine.Object.FindObjectsByType<PrivateArea>(FindObjectsSortMode.None);
        var inside = false;
        foreach (var area in areas)
        {
            if (!VssGame.WardEnabled(area) || !VssGame.WardInside(area, point, 0f)) continue;
            inside = true;
            var piece = area.GetComponent<Piece>();
            if ((piece != null && piece.GetCreator() == playerId) || VssGame.WardPermitted(area, playerId)) return true;
        }
        return !inside;
    }
}

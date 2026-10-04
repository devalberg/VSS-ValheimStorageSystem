using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace VSS;
internal static class VssPieceRegistry
{
    public const string PrefabName = "vss_access_stand";
    // $piece_stool is a localization key; the vanilla prefab is named piece_chair.
    internal const string BasePrefabName = "piece_chair";
    private static readonly AccessTools.FieldRef<ZNetScene, Dictionary<int, GameObject>> Named =
        AccessTools.FieldRefAccess<ZNetScene, Dictionary<int, GameObject>>("m_namedPrefabs");
    private static readonly AccessTools.FieldRef<ObjectDB, List<Piece>> BuildCache =
        AccessTools.FieldRefAccess<ObjectDB, List<Piece>>("m_buildPieces");
    private static GameObject _holder;
    private static GameObject _prefab;
    public static void Register(ZNetScene scene)
    {
        VssSettings.Initialize();
        if (!VssPlugin.ModConfig.RegisterStand.Value) return;
        EnsurePrefab(ObjectDB.instance, scene);
        if (_prefab == null) { VssPlugin.Log.LogError($"Vanilla {BasePrefabName} prefab missing; storage stand registration failed."); return; }
        if (!scene.m_prefabs.Contains(_prefab)) scene.m_prefabs.Add(_prefab);
        Named(scene)[PrefabName.GetStableHashCode()] = _prefab;
        RegisterRecipe(ObjectDB.instance);
        VssPlugin.Log.LogInfo("Registered Storage Inventory network prefab.");
    }
    private static PieceTable HammerTable(ObjectDB db) => db?.GetItemPrefab("Hammer")?.GetComponent<ItemDrop>()?.m_itemData.m_shared.m_buildPieces;
    private static void EnsurePrefab(ObjectDB db, ZNetScene scene)
    {
        if (!VssPlugin.ModConfig.RegisterStand.Value) return;
        if (_prefab == null)
        {
            // ObjectDB is ready before ZNetScene and before player recipe discovery.
            var stool = scene?.GetPrefab(BasePrefabName) ?? HammerTable(db)?.m_pieces.Find(p => p != null && p.name == BasePrefabName);
            if (stool == null) return;
            _holder = new GameObject("VSS Prefabs");
            _holder.SetActive(false);
            Object.DontDestroyOnLoad(_holder);
            // Keep activeSelf TRUE under an inactive parent: world instances must activate and initialize ZNetView.
            _prefab = Object.Instantiate(stool, _holder.transform, false);
            _prefab.name = PrefabName;
            foreach (var chair in _prefab.GetComponentsInChildren<Chair>(true)) Object.DestroyImmediate(chair);
            if (VssAssets.HasStandModel)
            {
                // The bundle is the complete pedestal; keep vanilla health/network/effects, replace its stool geometry.
                foreach (var renderer in _prefab.GetComponentsInChildren<Renderer>(true)) Object.DestroyImmediate(renderer);
                foreach (var filter in _prefab.GetComponentsInChildren<MeshFilter>(true)) Object.DestroyImmediate(filter);
                foreach (var collider in _prefab.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
            }
            var model = VssAssets.GetStandModel();
            model.transform.SetParent(_prefab.transform, false);
            foreach (var child in model.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = _prefab.layer;
            if (VssAssets.HasStandModel)
            {
                var renderers = model.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0) throw new System.InvalidOperationException("VSS pedestal bundle contains no renderers.");
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                var collider = _prefab.AddComponent<BoxCollider>();
                collider.center = _prefab.transform.InverseTransformPoint(bounds.center);
                collider.size = bounds.size;
            }
            var piece = _prefab.GetComponent<Piece>();
            piece.m_name = "Storage Inventory";
            piece.m_description = "Browse nearby chests. Drag items between storage and your inventory.";
            piece.m_icon = VssSprites.StandIcon;
            piece.m_category = Piece.PieceCategory.Furniture;
            piece.m_usage = Piece.UsageTagFlags.Furniture | Piece.UsageTagFlags.Storage;
            piece.m_craftingStation = null;
            piece.m_dlc = "";
            piece.m_comfort = 0;
            piece.m_enabled = true;
            piece.m_canBeRemoved = true;
            _prefab.AddComponent<VssAccessStand>();
            _prefab.SetActive(true);
        }
    }
    public static void RegisterRecipe(ObjectDB db)
    {
        if (db == null || !VssPlugin.ModConfig.RegisterStand.Value) return;
        var wood = db.GetItemPrefab("Wood");
        var table = HammerTable(db);
        if (wood == null || table == null) return;
        EnsurePrefab(db, null);
        if (_prefab == null) return;
        _prefab.GetComponent<Piece>().m_resources = new[]
        { new Piece.Requirement { m_resItem = wood.GetComponent<ItemDrop>(), m_amount = 5, m_recover = true } };
        if (!table.m_pieces.Contains(_prefab))
        {
            table.m_pieces.Add(_prefab);
            // Valheim's current build UI caches all pieces in ObjectDB.
            BuildCache(db)?.Clear();
            VssPlugin.Log.LogInfo("Registered Storage Inventory in Hammer table (5 wood; Furniture/Storage).");
        }
    }
}
[HarmonyPatch(typeof(ZNetScene), "Awake")]
internal static class VssZNetScenePatch
{
    private static void Postfix(ZNetScene __instance)
    {
        try { VssPieceRegistry.Register(__instance); }
        catch (System.Exception ex) { VssPlugin.Log.LogError($"VSS prefab registration failed: {ex}"); }
    }
}
[HarmonyPatch(typeof(ObjectDB), "Awake")]
internal static class VssObjectDbPatch
{
    private static void Postfix(ObjectDB __instance) => VssPieceRegistry.RegisterRecipe(__instance);
}
[HarmonyPatch(typeof(ObjectDB), "CopyOtherDB")]
internal static class VssCopyDbPatch
{
    private static void Postfix(ObjectDB __instance) => VssPieceRegistry.RegisterRecipe(__instance);
}
[HarmonyPatch(typeof(Player), "UpdateKnownRecipesList")]
internal static class VssDiscoverPiecePatch
{
    private static void Prefix() => VssPieceRegistry.RegisterRecipe(ObjectDB.instance);
}

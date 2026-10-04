using UnityEngine;

namespace VSS;
internal static class VssProceduralStand
{
    // The vanilla stool provides the networked building base. Replace this cube with a book/quill bundle later.
    public static GameObject CreateModel()
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "vss_placeholder_ledger";
        cube.transform.localPosition = new Vector3(0f, 0.7f, 0f);
        cube.transform.localScale = new Vector3(0.5f, 0.25f, 0.4f);
        cube.layer = LayerMask.NameToLayer("piece");
        var shader = Shader.Find("Standard") ?? Shader.Find("Custom/Vegetation") ?? Shader.Find("Sprites/Default");
        if (shader != null) cube.GetComponent<Renderer>().sharedMaterial = new Material(shader) { color = new Color(0.43f, 0.29f, 0.13f) };
        return cube;
    }
}

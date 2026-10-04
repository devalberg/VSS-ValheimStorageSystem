using System;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace VSS;

internal static class VssLayoutExport
{
    internal static void Save(RectTransform panel, string path)
    {
        var json = new StringBuilder("{\n  \"coordinateSystem\": \"logical pixels relative to panel top-left\",\n  \"panelWidth\": 1040, \"panelHeight\": 950,\n  \"elements\": [\n");
        var first = true;
        foreach (var rect in panel.GetComponentsInChildren<RectTransform>(true))
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            var min = new Vector2(float.MaxValue, float.MaxValue); var max = new Vector2(float.MinValue, float.MinValue);
            foreach (var corner in corners)
            {
                var local = (Vector2)panel.InverseTransformPoint(corner);
                min = Vector2.Min(min, local); max = Vector2.Max(max, local);
            }
            if (!first) json.Append(",\n"); first = false;
            var image = rect.GetComponent<Image>(); var text = rect.GetComponent<TMPro.TMP_Text>();
            var name = rect.name; var parent = rect.parent;
            while (parent != null && parent != panel) { name = parent.name + "/" + name; parent = parent.parent; }
            json.Append("    {\"path\": ").Append(Quote(name))
                .Append(", \"x\": ").Append(Number(min.x - panel.rect.xMin))
                .Append(", \"y\": ").Append(Number(panel.rect.yMax - max.y))
                .Append(", \"width\": ").Append(Number(max.x - min.x)).Append(", \"height\": ").Append(Number(max.y - min.y))
                .Append(", \"active\": ").Append(rect.gameObject.activeInHierarchy ? "true" : "false");
            if (image != null) json.Append(", \"sprite\": ").Append(Quote(image.sprite != null ? image.sprite.name : ""))
                .Append(", \"imageType\": ").Append(Quote(image.type.ToString())).Append(", \"raycastTarget\": ").Append(image.raycastTarget ? "true" : "false");
            if (text != null) json.Append(", \"text\": ").Append(Quote(text.text));
            json.Append('}');
        }
        File.WriteAllText(path, json.Append("\n  ]\n}\n").ToString());
    }
    private static string Number(float value) => Math.Round(value, 3).ToString(CultureInfo.InvariantCulture);
    private static string Quote(string value) => "\"" + (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t") + "\"";
}

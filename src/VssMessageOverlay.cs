using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VSS;

// Keep the actual vanilla messages and their animations; only lift their graphics over VSS.
internal static class VssMessageOverlay
{
    private sealed class State
    {
        internal Canvas Canvas;
        internal bool OverrideSorting;
        internal int SortingOrder, SortingLayer;
    }
    private static readonly List<State> States = new();

    internal static void Raise(MessageHud hud, Canvas storageCanvas)
    {
        if (hud == null || storageCanvas == null) return;
        RaiseGraphic(hud.m_messageText, storageCanvas);
        RaiseGraphic(hud.m_messageCenterText, storageCanvas);
        RaiseGraphic(hud.m_messageIcon, storageCanvas);
    }
    private static void RaiseGraphic(Graphic graphic, Canvas storageCanvas)
    {
        if (graphic == null) return;
        var canvas = graphic.GetComponent<Canvas>();
        if (canvas == null) canvas = graphic.gameObject.AddComponent<Canvas>();
        if (States.Exists(s => s.Canvas == canvas)) return;
        States.Add(new State { Canvas = canvas, OverrideSorting = canvas.overrideSorting,
            SortingOrder = canvas.sortingOrder, SortingLayer = canvas.sortingLayerID });
        canvas.overrideSorting = true; canvas.sortingLayerID = storageCanvas.sortingLayerID;
        canvas.sortingOrder = storageCanvas.sortingOrder + 10;
    }
    internal static void Restore()
    {
        foreach (var state in States)
        {
            if (state.Canvas == null) continue;
            state.Canvas.overrideSorting = state.OverrideSorting;
            state.Canvas.sortingLayerID = state.SortingLayer; state.Canvas.sortingOrder = state.SortingOrder;
        }
        States.Clear();
        // Added nested canvases now inherit vanilla sorting. Leave them in place so an immediate
        // close/reopen cannot reuse a component scheduled for destruction at the end of the frame.
    }
}

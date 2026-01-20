using UnityEditor;
using UnityEngine;

public class AnchorSizer
{
    // %[Ctrl], &[Alt], a[A] -> Ctrl + Alt + A
    [MenuItem("Tools/Anchors to Corners %&a")]
    static void AnchorsToCorners()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            RectTransform t = go.GetComponent<RectTransform>();
            if (t == null) continue;

            RectTransform p = t.parent.GetComponent<RectTransform>();
            if (p == null) continue;

            Undo.RecordObject(t, "Anchors to Corners");

            Vector2 newMin = new Vector2(t.anchorMin.x + t.offsetMin.x / p.rect.width,
                                         t.anchorMin.y + t.offsetMin.y / p.rect.height);
            Vector2 newMax = new Vector2(t.anchorMax.x + t.offsetMax.x / p.rect.width,
                                         t.anchorMax.y + t.offsetMax.y / p.rect.height);

            t.anchorMin = newMin;
            t.anchorMax = newMax;
            t.offsetMin = t.offsetMax = Vector2.zero;
        }
    }
}
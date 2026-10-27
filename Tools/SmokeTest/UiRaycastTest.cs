using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class UiRaycastTest
{
    private const BindingFlags All = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static Type T(string name) => Type.GetType("PayWithPixels." + name + ", Assembly-CSharp");

    [UnityTest, Timeout(120000)]
    public IEnumerator ShopButtonsAreClickable()
    {
        yield return EditorSceneManager.LoadSceneInPlayMode("Assets/_Project/Scenes/Game/Game.unity", new LoadSceneParameters(LoadSceneMode.Single));
        yield return null;
        if (GameObject.Find("Game") == null) T("Game").GetMethod("Create", All).Invoke(null, null);
        yield return null;
        var game = GameObject.Find("Game").GetComponent(T("Game"));
        T("Game").GetMethod("StartRun", All).Invoke(game, null);
        yield return null;
        // wymus sklep tak jak F5
        T("Game").GetMethod("OpenShop", All).Invoke(game, null);
        yield return null;
        yield return null;

        var es = EventSystem.current;
        Assert.NotNull(es, "no EventSystem");
        Debug.Log("[ui] EventSystem=" + es.name + " module=" + es.currentInputModule);
        var buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
        Debug.Log("[ui] buttons=" + buttons.Length + " screen=" + Screen.width + "x" + Screen.height);
        foreach (var b in buttons)
        {
            if (!b.gameObject.activeInHierarchy) continue;
            var rt = (RectTransform)b.transform;
            Vector3 center = rt.TransformPoint(rt.rect.center);
            var pd = new PointerEventData(es) { position = center };
            var hits = new List<RaycastResult>();
            es.RaycastAll(pd, hits);
            string top = hits.Count > 0 ? hits[0].gameObject.name + "<" + (hits[0].gameObject.transform.parent != null ? hits[0].gameObject.transform.parent.name : "") : "NONE";
            Debug.Log("[ui] button " + b.name + " interactable=" + b.interactable + " at " + center + " top hit: " + top + " (hits=" + hits.Count + ")");
        }
    }
}

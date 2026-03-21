using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ClickDebugger : MonoBehaviour
{
    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        var pointer = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        if (results.Count == 0)
        {
            Debug.Log("Клик никуда не попал — нет Raycast");
            return;
        }

        // Первый элемент — тот кто перехватывает клик
        Debug.Log($"<color=red>ПЕРЕХВАТИЛ: {results[0].gameObject.name}</color>");

        foreach (var r in results)
            Debug.Log($"  └ {r.gameObject.name} (depth: {r.depth})");
    }
}
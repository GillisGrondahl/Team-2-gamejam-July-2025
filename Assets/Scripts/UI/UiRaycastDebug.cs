using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UiRaycastDebug : MonoBehaviour
{
    private PointerEventData _ped;
    private readonly List<RaycastResult> _results = new();

    void Update()
    {
        if (EventSystem.current == null) return;
        _results.Clear();
        _ped ??= new PointerEventData(EventSystem.current);
        _ped.position = Input.mousePosition;
        EventSystem.current.RaycastAll(_ped, _results);

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log($"UI hits: {_results.Count}");
            foreach (var r in _results)
                Debug.Log($" → {r.gameObject.name} (canvas: {r.module?.ToString()})");
        }
    }
}
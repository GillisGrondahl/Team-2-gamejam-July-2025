using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PositiveSoftware.Utils.Tools
{
    [InitializeOnLoad]
    static class HierarchyIconActivation
    {
        static readonly Dictionary<int, string> _sceneMap = new Dictionary<int, string>();

        static HierarchyIconActivation()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGameObjectGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchySceneGUI;
            EditorApplication.hierarchyChanged += RefreshSceneMap;
        }

        static void RefreshSceneMap()
        {
            _sceneMap.Clear();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                _sceneMap[scene.handle] = scene.path;
            }
        }

        private static void OnHierarchySceneGUI(int instanceID, Rect selectionRect)
        {
            if (!_sceneMap.TryGetValue(instanceID, out var path))
                return;

            Scene scene = SceneManager.GetSceneByPath(path);
            bool isLoaded = scene.isLoaded;

            Rect rect = new Rect(selectionRect.x, selectionRect.y, 15f, selectionRect.height);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                if (scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, false);
                }
                else
                {
                    EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                }
                Event.current.Use();
            }

        }

        private static void OnHierarchyGameObjectGUI(int instanceID, Rect selectionRect)
        {

            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null)
                return;

            Rect rect = new Rect(selectionRect.x, selectionRect.y, 15f, selectionRect.height);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                if (!Application.isPlaying)
                {
                    Undo.RecordObject(obj, "Toggle GameObject");
                }
                obj.SetActive(!obj.activeSelf);
                if (!Application.isPlaying)
                {
                    EditorSceneManager.MarkSceneDirty(obj.scene);
                }
                Event.current.Use();
            }
        }
    }
}

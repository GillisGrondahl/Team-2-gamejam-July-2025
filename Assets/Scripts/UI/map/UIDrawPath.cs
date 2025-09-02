using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

public class PathDrawerTool : EditorWindow
{
    [SerializeField] private GameObject imageStepPrefab; // UI Image prefab to spawn
    [SerializeField] private Canvas targetCanvas; // Canvas to spawn images on
    [SerializeField] private float stepDistance = 50f; // Distance between step instances
    [SerializeField] private bool useCurvedPath = true; // Toggle between straight lines and curves
    [SerializeField] private bool isDrawing = false;

    private List<Vector2> pathPoints = new List<Vector2>(); // All control points for the path
    private int calculatedStepCount = 0; // Calculated number of steps based on distance

    [MenuItem("Tools/Path Drawer")]
    public static void ShowWindow()
    {
        GetWindow<PathDrawerTool>("Path Drawer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Advanced Path Drawer Tool", EditorStyles.boldLabel);

        // Prefab field
        imageStepPrefab = (GameObject)EditorGUILayout.ObjectField(
            "UI Image Step Prefab",
            imageStepPrefab,
            typeof(GameObject),
            false
        );

        // Canvas field
        targetCanvas = (Canvas)EditorGUILayout.ObjectField(
            "Target Canvas",
            targetCanvas,
            typeof(Canvas),
            true
        );

        // Step distance
        stepDistance = EditorGUILayout.FloatField("Step Distance", stepDistance);
        stepDistance = Mathf.Max(1f, stepDistance); // Ensure positive distance

        // Curve toggle
        useCurvedPath = EditorGUILayout.Toggle("Use Curved Path", useCurvedPath);

        EditorGUILayout.Space();

        // Path info
        EditorGUILayout.LabelField($"Control Points: {pathPoints.Count}");
        if (pathPoints.Count >= 2)
        {
            float pathLength = CalculatePathLength();
            calculatedStepCount = Mathf.FloorToInt(pathLength / stepDistance);
            EditorGUILayout.LabelField($"Path Length: {pathLength:F1}");
            EditorGUILayout.LabelField($"Calculated Steps: {calculatedStepCount}");
        }

        EditorGUILayout.Space();

        // Drawing toggle
        Color originalColor = GUI.backgroundColor;
        if (isDrawing)
        {
            GUI.backgroundColor = Color.green;
        }

        if (GUILayout.Button(isDrawing ? "Finish Path (F)" : "Start Drawing"))
        {
            ToggleDrawing();
        }
        GUI.backgroundColor = originalColor;

        // Control buttons
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Undo Last Point") && pathPoints.Count > 0)
        {
            UndoLastPoint();
        }
        if (GUILayout.Button("Clear Points"))
        {
            ClearPoints();
        }
        EditorGUILayout.EndHorizontal();

        // Create path button (only if we have enough points)
        GUI.enabled = pathPoints.Count >= 2;
        if (GUILayout.Button($"Create Path ({calculatedStepCount} steps)"))
        {
            CreatePath();
        }
        GUI.enabled = true;

        EditorGUILayout.Space();

        // Instructions
        string instructions = useCurvedPath ?
            "CURVED PATH MODE:\n" +
            "• Click to add control points\n" +
            "• Minimum 2 points needed\n" +
            "• More points = smoother curves\n" +
            "• Press F or click 'Finish Path' when done\n" +
            "• Press ESC to cancel"
            :
            "STRAIGHT PATH MODE:\n" +
            "• Click to add waypoints\n" +
            "• Path connects points with straight lines\n" +
            "• Press F or click 'Finish Path' when done\n" +
            "• Press ESC to cancel";

        EditorGUILayout.HelpBox(instructions, MessageType.Info);

        // Status
        if (isDrawing)
        {
            string status = pathPoints.Count == 0 ? "Click to place first point" :
                           $"Click to add point {pathPoints.Count + 1}";
            EditorGUILayout.LabelField("Status:", status);
        }

        // Clear all button
        EditorGUILayout.Space();
        if (GUILayout.Button("Clear All Step Images"))
        {
            ClearAllStepImages();
        }
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        isDrawing = false;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isDrawing) return;

        Event e = Event.current;

        // Handle keyboard shortcuts
        if (e.type == EventType.KeyDown)
        {
            if (e.keyCode == KeyCode.Escape)
            {
                CancelDrawing();
                e.Use();
                return;
            }
            else if (e.keyCode == KeyCode.F && pathPoints.Count >= 2)
            {
                CreatePath();
                ToggleDrawing();
                e.Use();
                return;
            }
        }

        // Handle mouse clicks to add points
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (targetCanvas != null)
            {
                Vector2 localPoint = GetCanvasLocalPosition(e.mousePosition);
                pathPoints.Add(localPoint);
                Debug.Log($"Added point {pathPoints.Count} at: {localPoint}");
            }

            e.Use();
            Repaint();
        }

        // Draw the path preview
        DrawPathPreview();
    }

    private void DrawPathPreview()
    {
        if (pathPoints.Count == 0 || targetCanvas == null) return;

        RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();

        // Draw existing points
        Handles.color = Color.red;
        for (int i = 0; i < pathPoints.Count; i++)
        {
            Vector3 worldPos = canvasRect.TransformPoint(pathPoints[i]);
            Handles.DrawWireCube(worldPos, Vector3.one * 8f);
            Handles.Label(worldPos + Vector3.up * 15f, $"P{i + 1}");
        }

        // Draw path preview
        if (pathPoints.Count >= 2)
        {
            Handles.color = Color.yellow;

            if (useCurvedPath)
            {
                DrawCurvedPathPreview(canvasRect);
            }
            else
            {
                DrawStraightPathPreview(canvasRect);
            }
        }

        // Draw line to mouse cursor from last point
        if (pathPoints.Count > 0)
        {
            Vector2 mouseLocalPos = GetCanvasLocalPosition(Event.current.mousePosition);
            Vector3 lastPointWorld = canvasRect.TransformPoint(pathPoints[pathPoints.Count - 1]);
            Vector3 mouseWorld = canvasRect.TransformPoint(mouseLocalPos);

            Handles.color = Color.gray;
            Handles.DrawDottedLine(lastPointWorld, mouseWorld, 5f);
        }
    }

    private void DrawStraightPathPreview(RectTransform canvasRect)
    {
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Vector3 startWorld = canvasRect.TransformPoint(pathPoints[i]);
            Vector3 endWorld = canvasRect.TransformPoint(pathPoints[i + 1]);
            Handles.DrawLine(startWorld, endWorld);
        }
    }

    private void DrawCurvedPathPreview(RectTransform canvasRect)
    {
        List<Vector2> curvePoints = GenerateCurvePoints(100); // High resolution for preview

        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            Vector3 startWorld = canvasRect.TransformPoint(curvePoints[i]);
            Vector3 endWorld = canvasRect.TransformPoint(curvePoints[i + 1]);
            Handles.DrawLine(startWorld, endWorld);
        }
    }

    private List<Vector2> GenerateCurvePoints(int resolution)
    {
        List<Vector2> curvePoints = new List<Vector2>();

        if (pathPoints.Count < 2) return curvePoints;

        if (pathPoints.Count == 2)
        {
            // Straight line for only 2 points
            for (int i = 0; i <= resolution; i++)
            {
                float t = i / (float)resolution;
                curvePoints.Add(Vector2.Lerp(pathPoints[0], pathPoints[1], t));
            }
        }
        else if (pathPoints.Count == 3)
        {
            // Quadratic Bezier curve
            for (int i = 0; i <= resolution; i++)
            {
                float t = i / (float)resolution;
                Vector2 point = CalculateQuadraticBezier(pathPoints[0], pathPoints[1], pathPoints[2], t);
                curvePoints.Add(point);
            }
        }
        else
        {
            // Catmull-Rom spline for 4+ points
            curvePoints = GenerateCatmullRomSpline(resolution);
        }

        return curvePoints;
    }

    private Vector2 CalculateQuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector2 point = uu * p0; // (1-t)^2 * P0
        point += 2 * u * t * p1; // 2(1-t)t * P1
        point += tt * p2; // t^2 * P2

        return point;
    }

    private List<Vector2> GenerateCatmullRomSpline(int resolution)
    {
        List<Vector2> splinePoints = new List<Vector2>();

        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Vector2 p0 = i > 0 ? pathPoints[i - 1] : pathPoints[i];
            Vector2 p1 = pathPoints[i];
            Vector2 p2 = pathPoints[i + 1];
            Vector2 p3 = i + 2 < pathPoints.Count ? pathPoints[i + 2] : pathPoints[i + 1];

            int segmentResolution = resolution / (pathPoints.Count - 1);
            for (int j = 0; j <= segmentResolution; j++)
            {
                if (i == pathPoints.Count - 2 && j == segmentResolution) break; // Avoid duplicate end point

                float t = j / (float)segmentResolution;
                Vector2 point = CalculateCatmullRom(p0, p1, p2, p3, t);
                splinePoints.Add(point);
            }
        }

        return splinePoints;
    }

    private Vector2 CalculateCatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
    {
        float tt = t * t;
        float ttt = tt * t;

        float q0 = -ttt + 2 * tt - t;
        float q1 = 3 * ttt - 5 * tt + 2;
        float q2 = -3 * ttt + 4 * tt + t;
        float q3 = ttt - tt;

        return 0.5f * (p0 * q0 + p1 * q1 + p2 * q2 + p3 * q3);
    }

    private float CalculatePathLength()
    {
        if (pathPoints.Count < 2) return 0f;

        List<Vector2> curvePoints = GenerateCurvePoints(200); // High resolution for accurate length

        float length = 0f;
        for (int i = 0; i < curvePoints.Count - 1; i++)
        {
            length += Vector2.Distance(curvePoints[i], curvePoints[i + 1]);
        }

        return length;
    }

    private void ToggleDrawing()
    {
        isDrawing = !isDrawing;
        if (!isDrawing && pathPoints.Count >= 2)
        {
            CreatePath();
        }
        else if (!isDrawing)
        {
            ClearPoints();
        }

        if (isDrawing && SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.Focus();
        }
    }

    private void CancelDrawing()
    {
        isDrawing = false;
        ClearPoints();
        Debug.Log("Path drawing cancelled");
    }

    private void UndoLastPoint()
    {
        if (pathPoints.Count > 0)
        {
            pathPoints.RemoveAt(pathPoints.Count - 1);
            Debug.Log($"Removed last point. Points remaining: {pathPoints.Count}");
            Repaint();
        }
    }

    private void ClearPoints()
    {
        pathPoints.Clear();
        Repaint();
    }

    private void CreatePath()
    {
        if (imageStepPrefab == null || targetCanvas == null || pathPoints.Count < 2)
        {
            Debug.LogError("Missing prefab, canvas reference, or insufficient points!");
            return;
        }

        if (imageStepPrefab.GetComponent<Image>() == null)
        {
            Debug.LogError("Prefab must have an Image component!");
            return;
        }

        Debug.Log($"Creating curved path with {pathPoints.Count} control points");

        // Generate curve points based on step distance
        List<Vector2> curvePoints = GenerateCurvePoints(1000); // High resolution for placement accuracy
        List<Vector2> stepPositions = new List<Vector2>();

        // Sample points along curve at specified distance intervals
        float accumulatedDistance = 0f;
        stepPositions.Add(curvePoints[0]); // Always include start point

        for (int i = 1; i < curvePoints.Count; i++)
        {
            float segmentLength = Vector2.Distance(curvePoints[i - 1], curvePoints[i]);
            accumulatedDistance += segmentLength;

            if (accumulatedDistance >= stepDistance)
            {
                stepPositions.Add(curvePoints[i]);
                accumulatedDistance = 0f;
            }
        }

        // Create parent object
        string pathType = useCurvedPath ? "CurvedPath" : "StraightPath";
        GameObject pathParent = new GameObject($"{pathType}_{System.DateTime.Now:HHmmss}");
        pathParent.transform.SetParent(targetCanvas.transform, false);

        // Spawn step images (skip first position to avoid placing on start point)
        for (int i = 1; i < stepPositions.Count; i++)
        {
            GameObject stepInstance = PrefabUtility.InstantiatePrefab(imageStepPrefab) as GameObject;
            stepInstance.transform.SetParent(pathParent.transform, false);
            stepInstance.name = $"Step_{i:00}";

            RectTransform stepRect = stepInstance.GetComponent<RectTransform>();
            stepRect.anchoredPosition = stepPositions[i];

            // Calculate rotation to face path direction
            if (i > 0 && i < stepPositions.Count)
            {
                Vector2 direction = (stepPositions[i] - stepPositions[i - 1]).normalized;
                if (direction != Vector2.zero)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    stepRect.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );

        Debug.Log($"Created {stepPositions.Count - 1} step images along {pathType.ToLower()}");

        // Clear points after creation
        ClearPoints();
    }

    private Vector2 GetCanvasLocalPosition(Vector2 sceneViewMousePosition)
    {
        if (targetCanvas == null) return Vector2.zero;

        // Get world position from scene view mouse position
        Ray ray = HandleUtility.GUIPointToWorldRay(sceneViewMousePosition);

        RectTransform canvasRect = targetCanvas.GetComponent<RectTransform>();
        Canvas canvas = targetCanvas.GetComponent<Canvas>();

        Vector2 localPoint = Vector2.zero;

        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            // For world space canvas, intersect ray with canvas plane
            Plane canvasPlane = new Plane(-canvasRect.forward, canvasRect.position);
            float distance;
            if (canvasPlane.Raycast(ray, out distance))
            {
                Vector3 worldHitPoint = ray.GetPoint(distance);
                localPoint = canvasRect.InverseTransformPoint(worldHitPoint);
            }
        }
        else
        {
            // For screen space canvases, project world position to canvas
            Vector3 worldPosition = ray.origin;

            // If canvas has a camera assigned, use it
            Camera canvasCamera = canvas.worldCamera;
            if (canvasCamera == null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                canvasCamera = Camera.main;
            }

            if (canvasCamera != null)
            {
                Vector2 screenPoint = canvasCamera.WorldToScreenPoint(worldPosition);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPoint,
                    canvasCamera,
                    out localPoint
                );
            }
            else
            {
                // For overlay mode, use a different approach
                Vector3 canvasWorldPos = canvasRect.TransformPoint(Vector3.zero);
                Vector3 offset = worldPosition - canvasWorldPos;
                localPoint = canvasRect.InverseTransformVector(offset);
            }
        }

        return localPoint;
    }

    private void ClearAllStepImages()
    {
        if (targetCanvas == null) return;

        Transform[] children = targetCanvas.GetComponentsInChildren<Transform>();
        int clearedCount = 0;

        for (int i = children.Length - 1; i >= 0; i--)
        {
            if (children[i].name.Contains("Path_") || children[i].name.Contains("CurvedPath_") || children[i].name.Contains("StraightPath_"))
            {
                DestroyImmediate(children[i].gameObject);
                clearedCount++;
            }
        }

        Debug.Log($"Cleared {clearedCount} path objects");

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
        );
    }
}
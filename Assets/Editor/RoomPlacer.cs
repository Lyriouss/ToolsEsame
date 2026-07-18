using UnityEditor;
using UnityEngine;

public class RoomPlacer : EditorWindow
{
    bool rangeVisible = true;
    private float range = 20f;
    
    private static Vector3 previewPosition;
    
    [MenuItem("Tools/Room Placer")]
    public static void ShowWindow()
    {
        GetWindow<RoomPlacer>("Room Placer");
    }
    
    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI()
    {
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.black;
        titleStyle.fontSize = 20;

        GUILayout.Label("", GUI.skin.horizontalSlider);
        GUILayout.Space(10);
        GUILayout.Label("Set snap range", titleStyle);
        GUILayout.Label("", GUI.skin.horizontalSlider);
        GUILayout.Space(15);

        // using (new GUILayout.VerticalScope())
        // {
        // }

        rangeVisible = EditorGUILayout.ToggleLeft("Range Visible", rangeVisible);

        range = EditorGUILayout.FloatField("Range", range);

        GUILayout.Space(15);
        GUILayout.Label("", GUI.skin.horizontalSlider);
        GUILayout.Space(10);
        GUILayout.Label("Select rooms by the number of doors", titleStyle);
        GUILayout.Label("", GUI.skin.horizontalSlider);
        GUILayout.Space(15);

        if (GUILayout.Button("One Door", GUILayout.Height(50)))
        {
            
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Two Doors", GUILayout.Height(50)))
        {
            
        }
        
        GUILayout.Space(10);

        if (GUILayout.Button("Three Doors", GUILayout.Height(50)))
        {
            
        }
        
        GUILayout.Space(10);

        if (GUILayout.Button("Four Doors", GUILayout.Height(50)))
        {
            
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!rangeVisible)
            return;
        
        Event e = Event.current;
        
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        
        if (groundPlane.Raycast(ray, out float distance))
        {
            previewPosition = ray.GetPoint(distance);
            
            Handles.color = Color.black;
            Handles.DrawWireDisc(previewPosition, Vector3.up, range);
            SceneView.RepaintAll();
        }
    }
}

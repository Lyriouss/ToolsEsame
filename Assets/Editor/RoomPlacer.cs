using System.IO;
using UnityEditor;
using UnityEngine;

public enum RoomDoors
{
    One,
    Two,
    Three,
    Four
}

public class RoomPlacer : EditorWindow
{
    bool rangeVisible = true;
    private float range = 20f;
    private RoomDoors doors = RoomDoors.One;

    private GameObject selectedPrefab;
    private GameObject lastSelectedPrefab;
    // private Mesh previewMesh;
    // private Material[] previewMaterials;
    // private Vector3 previewScale;
    
    private GameObject previewPrefab;
    private static Vector3 previewPosition;

    private Vector2 scrollPos;
    
    [MenuItem("Tools/Room Placer")]
    public static void ShowWindow()
    {
        GetWindow<RoomPlacer>("Room Placer");
    }
    
    private void OnEnable()
    {
        SceneView.duringSceneGui += OnPreviewPrefab;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnPreviewPrefab;
        SceneView.duringSceneGui -= OnSceneGUI;

        ClearPreview();
        // ClearSelection();
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
            doors = RoomDoors.One;
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Two Doors", GUILayout.Height(50)))
        {
            doors = RoomDoors.Two;
        }
        
        GUILayout.Space(10);

        if (GUILayout.Button("Three Doors", GUILayout.Height(50)))
        {
            doors = RoomDoors.Three;
        }
        
        GUILayout.Space(10);

        if (GUILayout.Button("Four Doors", GUILayout.Height(50)))
        {
            doors = RoomDoors.Four;
        }
    }

    private void OnPreviewPrefab(SceneView sceneView)
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        
        if (previewPrefab == null && selectedPrefab != null)
        {
            previewPrefab = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            lastSelectedPrefab = selectedPrefab;
        }
        else if (lastSelectedPrefab != selectedPrefab)
        {
            DestroyImmediate(previewPrefab);
            previewPrefab = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            lastSelectedPrefab = selectedPrefab;
        }
        
        Event e = Event.current;
        
        if (e.type == EventType.KeyDown && e.shift && e.keyCode == KeyCode.E)
        {
            if (previewPrefab != null)
            {
                previewPrefab.transform.Rotate(0f, 90f, 0f, Space.World);
                SceneView.RepaintAll();
            }

            e.Use();
        }
        else if (e.type == EventType.KeyDown && e.shift && e.keyCode == KeyCode.Q)
        {
            if (previewPrefab != null)
            {
                previewPrefab.transform.Rotate(0f, -90f, 0f, Space.World);
                SceneView.RepaintAll();
            }
            
            e.Use();
        }
        
        
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        
        if (groundPlane.Raycast(ray, out float distance))
        {
            previewPosition = ray.GetPoint(distance);

            if (rangeVisible)
            {
                Handles.color = Color.black;
                Handles.DrawWireDisc(previewPosition, Vector3.up, range);
                SceneView.RepaintAll();
            }

            if (previewPrefab != null)
            {
                previewPrefab.transform.position = previewPosition;
            }
            
            // Matrix4x4 matrix = Matrix4x4.TRS(previewPosition, Quaternion.identity, previewScale);
            //
            // for (int i = 0; i < previewMaterials.Length; i++)
            // {
            //     Graphics.DrawMesh(previewMesh, matrix, previewMaterials[i], 0, sceneView.camera, i);
            // }
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        GUIStyle infoStyle = new GUIStyle();
        infoStyle.alignment = TextAnchor.MiddleCenter;
        infoStyle.normal.textColor = Color.black;
        infoStyle.fontSize = 16;
        infoStyle.fontStyle = FontStyle.Bold;
        
        Handles.BeginGUI();

        GUILayout.Label("Click on scene view and rotate rooms with Shift + Q and Shift + E", infoStyle);
        
        GUILayout.BeginArea(new Rect(10, 10, position.width, position.height));

        GUILayout.Space(15);

        string folderPath;
        
        switch (doors)
        {
            case RoomDoors.One:
                folderPath = "Assets/Prefabs/OneDoor";

                if (!Directory.Exists(folderPath))
                {
                    EditorGUILayout.HelpBox("Prefabs folder does not exist.", MessageType.Warning);
                    return;
                }
                
                PrefabFolderPreview(folderPath);
                
                break;
            
            case RoomDoors.Two:
                folderPath = "Assets/Prefabs/TwoDoors";

                if (!Directory.Exists(folderPath))
                {
                    EditorGUILayout.HelpBox("Prefabs folder does not exist.", MessageType.Warning);
                    return;
                }
                
                PrefabFolderPreview(folderPath);

                break;
            
            case RoomDoors.Three:
                folderPath = "Assets/Prefabs/ThreeDoors";

                if (!Directory.Exists(folderPath))
                {
                    EditorGUILayout.HelpBox("Prefabs folder does not exist.", MessageType.Warning);
                    return;
                }
                
                PrefabFolderPreview(folderPath);

                break;
            
            case RoomDoors.Four:
                folderPath = "Assets/Prefabs/FourDoors";

                if (!Directory.Exists(folderPath))
                {
                    EditorGUILayout.HelpBox("Prefabs folder does not exist.", MessageType.Warning);
                    return;
                }
                
                PrefabFolderPreview(folderPath);

                break;
        }
        
        GUILayout.EndArea();
        
        Handles.EndGUI();
    }
    
    private void PrefabFolderPreview(string path)
    {
        DirectoryInfo dir = new DirectoryInfo(path);
        FileInfo[] info = dir.GetFiles("*.prefab");
                
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        
        foreach (FileInfo f in info)
        {
            //Converts absolute path into Unity's relative path
            string relPath = path + "/" + f.Name;
            
            GameObject filePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(relPath);
            Texture2D texturePreview = AssetPreview.GetAssetPreview(filePrefab);
            
            if (texturePreview != null)
            {
                if (GUILayout.Button(texturePreview, GUILayout.Width(160), GUILayout.Height(80)))
                {
                    selectedPrefab = filePrefab;
                    // SelectPrefab(filePrefab);
                }
            }
            else
            {
                if (GUILayout.Button("Generating Preview...", GUILayout.Width(160), GUILayout.Height(80)))
                {
                    selectedPrefab = filePrefab;
                    // SelectPrefab(filePrefab);
                }
            }

        }
        
        GUILayout.Space(10);

        if (GUILayout.Button("Undo", GUILayout.Height(80), GUILayout.Width(160)))
        {
            
        }
        
        GUILayout.EndScrollView();
    }

    // private void SelectPrefab(GameObject prefab)
    // {
    //     selectedPrefab = prefab;
    //
    //     var meshFilter = prefab.GetComponentInChildren<MeshFilter>();
    //     var meshRenderer = prefab.GetComponentInChildren<MeshRenderer>();
    //
    //     if (meshFilter != null && meshRenderer != null)
    //     {
    //         previewMesh = meshFilter.sharedMesh;
    //         previewMaterials = meshRenderer.sharedMaterials;
    //         previewScale = prefab.transform.localScale;
    //     }
    //     else
    //     {
    //         ClearSelection();
    //     }
    // }
    
    private void ClearPreview()
    {
        if (previewPrefab != null)
            DestroyImmediate(previewPrefab);
    }
    
    // private void ClearSelection()
    // {
    //     selectedPrefab = null;
    //     previewMesh = null;
    //     previewMaterials = null;
    // }
}

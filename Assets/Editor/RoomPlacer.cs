using System.Collections.Generic;
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
    private Vector3 previewPosition;

    private GameObject container;
    
    private List<GameObject> previewDoors = new List<GameObject>();
    private List<GameObject> worldDoors = new List<GameObject>();

    private bool isSnapped = false;

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
            
            previewDoors.Clear();
            previewDoors = FindGameObjectsWithTagInChildren(previewPrefab, "Door");
            
            lastSelectedPrefab = selectedPrefab;
        }
        else if (lastSelectedPrefab != selectedPrefab)
        {
            DestroyImmediate(previewPrefab);
            previewPrefab = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            
            previewDoors.Clear();
            previewDoors = FindGameObjectsWithTagInChildren(previewPrefab, "Door");
            
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
                Physics.SyncTransforms();
                TryToSnapDoors();
            }
        }

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            if (previewPrefab == null || !isSnapped)
                return;

            PlaceRoom(previewPrefab.transform.position, previewPrefab.transform.rotation);
        
            e.Use();
        }
        
        sceneView.Repaint();
    }

    private void TryToSnapDoors()
    {
        if (previewDoors.Count <= 0)
            return;

        List<GameObject> hitWorldDoors = new List<GameObject>();
        
        LayerMask doorMask = LayerMask.GetMask("Door");

        Collider[] hitDoors = Physics.OverlapSphere(previewPosition, range, doorMask);

        foreach (Collider hitDoor in hitDoors)
        {
            if (!hitDoor.transform.IsChildOf(previewPrefab.transform))
            {
                hitWorldDoors.Add(hitDoor.gameObject);
            }
        }

        if (hitWorldDoors.Count <= 0)
        {
            isSnapped = false;
            return;
        }
        
        GameObject bestPreviewDoor = null;
        GameObject bestWorldDoor = null;
        float bestDistance = float.MaxValue;
        
        foreach (GameObject previewDoor in previewDoors)
        {
            foreach (GameObject worldDoor in hitWorldDoors)
            {
                if (Physics.OverlapSphere(worldDoor.transform.position, 0.001f).Length >= 2)
                    continue;
                
                float facingDot = Vector3.Dot(previewDoor.transform.forward, -worldDoor.transform.forward);

                Vector3 directionToWorldDoor = (previewPosition - worldDoor.transform.position).normalized;
                float directionalDot = Vector3.Dot(worldDoor.transform.forward, directionToWorldDoor);
                
                if (facingDot > 0.5f && directionalDot > 0f)
                {
                    float distance = Vector3.Distance(previewPosition, worldDoor.transform.position);
                    
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestPreviewDoor = previewDoor;
                        bestWorldDoor = worldDoor;
                    }
                }
            }
        }
        
        if (bestPreviewDoor != null)
        {
            isSnapped = true;
            SnapDoors(bestPreviewDoor, bestWorldDoor);
        }
        else
            isSnapped = false;
    }

    private void SnapDoors(GameObject previewDoor, GameObject worldDoor)
    {
        Vector3 offset = previewDoor.transform.position - previewPrefab.transform.position;
        Vector3 targetPos = worldDoor.transform.position - offset;

        previewPrefab.transform.position = targetPos;
    }

    private void PlaceRoom(Vector3 position, Quaternion rotation)
    {
        ContainerCheck();

        GameObject room = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab, container.transform);

        Undo.RegisterCreatedObjectUndo(room, "Place Room");
        
        room.transform.position = position;
        room.transform.rotation = rotation;
    }
    
    private void ContainerCheck()
    {
        if (container == null)
            container = GameObject.Find("Placed Rooms");
        
        if (container == null)
            container = new GameObject("Placed Rooms");
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

            GUILayout.Space(5);
        }
        
        //GUILayout.Space(5);

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
    
    private List<GameObject> FindGameObjectsWithTagInChildren(GameObject parent, string tag)
    {
        List<GameObject> childrenWithLayer = new List<GameObject>();
        foreach (Transform child in parent.transform)
        {
            if (child.gameObject.CompareTag(tag))
                childrenWithLayer.Add(child.gameObject);
        }
        return childrenWithLayer;
    }

    // private List<GameObject> UpdateWorldDoors()
    // {
    //     worldDoors.Clear();
    //     DestroyImmediate(previewPrefab);
    //     
    //     List<GameObject> newWorldDoors = new List<GameObject>();
    //     
    //     GameObject[] allDoors = GameObject.FindGameObjectsWithTag("Door");
    //
    //     LayerMask layerMask = LayerMask.GetMask("Door");
    //     
    //     foreach (GameObject door in allDoors)
    //     {
    //         if (previewPrefab != null)
    //             if (door.transform.IsChildOf(previewPrefab.transform))
    //                 continue;
    //
    //         if (Physics.OverlapSphere(door.transform.position, 0.001f, layerMask).Length >= 2)
    //             continue;
    //         
    //         newWorldDoors.Add(door);
    //
    //         //Debug.LogWarning(worldDoor);
    //     }
    //
    //     foreach (GameObject door in newWorldDoors)
    //     {
    //         Debug.LogWarning(door);
    //     }
    //
    //     Debug.LogWarning(newWorldDoors.Count);
    //     
    //     return newWorldDoors;
    // }
    
    private void ClearPreview()
    {
        if (previewPrefab != null)
            DestroyImmediate(previewPrefab);

        previewDoors.Clear();
    }
    
    // private void ClearSelection()
    // {
    //     selectedPrefab = null;
    //     previewMesh = null;
    //     previewMaterials = null;
    // }
}

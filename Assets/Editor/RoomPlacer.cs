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
    private static Vector3 previewPosition;
    
    private List<GameObject> previewDoors = new List<GameObject>();
    private List<GameObject> worldDoors = new List<GameObject>();

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

        worldDoors = UpdateWorldDoors();
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
                TryToSnapDoors();
            }
            
            // Matrix4x4 matrix = Matrix4x4.TRS(previewPosition, Quaternion.identity, previewScale);
            //
            // for (int i = 0; i < previewMaterials.Length; i++)
            // {
            //     Graphics.DrawMesh(previewMesh, matrix, previewMaterials[i], 0, sceneView.camera, i);
            // }
        }
    }

    private void TryToSnapDoors()
    {
        if (previewDoors.Count <= 0)
            return;
        
        // int doorMask = LayerMask.NameToLayer("Door");
        //
        // Collider[] results = new Collider[20];
        //
        // PhysicsScene physicsScene = SceneManager.GetActiveScene().GetPhysicsScene();
        // int hits = physicsScene.OverlapSphere(previewPosition, range, results, doorMask, QueryTriggerInteraction.UseGlobal);
        //
        // if (hits <= 0)
        //     return;
        
        GameObject bestPreviewDoor = null;
        GameObject bestWorldDoor = null;
        float bestDistance = float.MaxValue;
        
        foreach (GameObject previewDoor in previewDoors)
        {
            // foreach (Collider hit in results)
            // {
            //     if (hit.transform.IsChildOf(previewPrefab.transform))
            //         continue;
            //     
            //     float facingDot = Vector3.Dot(previewDoor.transform.forward, -hit.transform.forward);
            //
            //     if (facingDot > 0.5)
            //     {
            //         float distance = Vector3.Distance(previewDoor.transform.position, hit.transform.position);
            //         if (distance < bestDistance)
            //         {
            //             bestDistance = distance;
            //             bestPreviewDoor = previewDoor;
            //             bestWorldDoor = hit.gameObject;
            //         }
            //     }
            // }

            foreach (GameObject worldDoor in worldDoors)
            {
                float distanceToWorldDoor = Vector3.Distance(previewPosition, worldDoor.transform.position);
                if (distanceToWorldDoor > range)
                    continue;
                
                float facingDot = Vector3.Dot(previewDoor.transform.forward, -worldDoor.transform.forward);
                
                if (facingDot > 0.5)
                {
                    float distance = Vector3.Distance(previewDoor.transform.position, worldDoor.transform.position);
                    Debug.LogWarning(worldDoor.name + " distance: " + distance);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestPreviewDoor = previewDoor;
                        bestWorldDoor = worldDoor;
                    }
                }
            }

            if (bestPreviewDoor != null)
                SnapDoors(bestPreviewDoor, bestWorldDoor);
        }
    }

    private void SnapDoors(GameObject previewDoor, GameObject worldDoor)
    {
        Vector3 offset = previewDoor.transform.position - previewPrefab.transform.position;
        Vector3 targetPos = worldDoor.transform.position - offset;

        previewPrefab.transform.position = targetPos;
    }

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

    private List<GameObject> UpdateWorldDoors()
    {
        worldDoors.Clear();
        
        List<GameObject> newWorldDoors = new List<GameObject>();
        
        GameObject[] allWorldDoors = GameObject.FindGameObjectsWithTag("Door");
        foreach (GameObject worldDoor in allWorldDoors)
        {
            newWorldDoors.Add(worldDoor);
        }
        
        return newWorldDoors;
    }
    
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

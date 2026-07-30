using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

//Enum used for selection of rooms to place of that category
public enum RoomDoors
{
    One,
    Two,
    Three,
    Four
}

public class RoomPlacer : EditorWindow
{
    //Changeable values from tool window
    bool rangeVisible = true;
    private float range = 10f;
    private RoomDoors doors = RoomDoors.One;

    //Prefab reference obtained from Asset folders
    private GameObject selectedPrefab;
    private GameObject lastSelectedPrefab;
    
    //References for preview of room in scene
    private GameObject previewPrefab;
    private Vector3 previewPosition;

    //Transform to place all placed rooms
    private GameObject container;
    
    //List to store current preview doors of previewPrefab
    private List<GameObject> previewDoors = new List<GameObject>();

    //Bool to prevent room from being placed without a snap
    private bool isSnapped = false;

    //Used in PrefabPreviewFolder in case prefabs available exceed scene view area
    private Vector2 scrollPos;
    
    //Creates a window for this tool
    [MenuItem("Tools/Room Placer")]
    public static void ShowWindow()
    {
        GetWindow<RoomPlacer>("Room Placer");
    }
    
    private void OnEnable()
    {
        //Instantiates scene view function when opening tool window
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        //Stops scene view function when closing tool window
        SceneView.duringSceneGui -= OnSceneGUI;

        //Removes room preview from scene when closing tool window
        ClearPreview();
    }

    //OnGUI functions run automatically when opening tool window
    private void OnGUI()
    {
        //Created a custom style for GUI texts
        GUIStyle titleStyle = new GUIStyle();
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.black;
        titleStyle.fontSize = 20;

        //From top to bottom, creates customized visual elements of Layout in tool window
        GUILayout.Label("", GUI.skin.horizontalSlider);
        GUILayout.Space(10);
        GUILayout.Label("Set snap range", titleStyle);
        GUILayout.Label("", GUI.skin.horizontalSlider);
        GUILayout.Space(15);

        //Creates a toggle button to change rangeVisible bool from window GUI
        rangeVisible = EditorGUILayout.Toggle("Range Visible", rangeVisible);
        //Same here but to change range float
        range = EditorGUILayout.FloatField("Range", range);
        
        //Creates another customized Layout
        GUILayout.Space(15);
        GUILayout.Label("", GUI.skin.horizontalSlider);
        GUILayout.Space(10);
        GUILayout.Label("Select rooms by the number of doors", titleStyle);
        GUILayout.Label("", GUI.skin.horizontalSlider);
        GUILayout.Space(15);

        //Creates interactable buttons in tool window to change which room category is shown in scene view GUI
        if (GUILayout.Button("One Door", GUILayout.Height(50)))
        {
            doors = RoomDoors.One;
            //When selecting a new room category, sets selectedPrefab to null so the first prefab of that category is automatically selected
            selectedPrefab = null;
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Two Doors", GUILayout.Height(50)))
        {
            doors = RoomDoors.Two;
            selectedPrefab = null;
        }
        
        GUILayout.Space(10);

        if (GUILayout.Button("Three Doors", GUILayout.Height(50)))
        {
            doors = RoomDoors.Three;
            selectedPrefab = null;
        }
        
        GUILayout.Space(10);

        if (GUILayout.Button("Four Doors", GUILayout.Height(50)))
        {
            doors = RoomDoors.Four;
            selectedPrefab = null;
        }
    }
    
    private void OnSceneGUI(SceneView sceneView)
    {
        //Prevents the user to interact with scene objects
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        //Creates a custom style for text
        GUIStyle infoStyle = new GUIStyle();
        infoStyle.alignment = TextAnchor.MiddleCenter;
        infoStyle.normal.textColor = Color.black;
        infoStyle.fontSize = 16;
        infoStyle.fontStyle = FontStyle.Bold;

        //Creates 2D GUI block inside of scene view
        Handles.BeginGUI();

        //Shows text in scene view
        GUILayout.Label("Click on scene view and rotate rooms with Shift + Q and Shift + E", infoStyle);
        
        //Based on the current enum of room category selected, returns a path to Asset folder
        string folderPath;
        switch (doors)
        {
            case RoomDoors.One:
                folderPath = "Assets/Prefabs/OneDoor";
                break;

            case RoomDoors.Two:
                folderPath = "Assets/Prefabs/TwoDoors";
                break;

            case RoomDoors.Three:
                folderPath = "Assets/Prefabs/ThreeDoors";
                break;

            case RoomDoors.Four:
                folderPath = "Assets/Prefabs/FourDoors";
                break;

            default:
                folderPath = null;
                break;
        }

        //If the folderPath exists in project
        if (Directory.Exists(folderPath))
            //Shows the prefabs in that folder in scene view.
            PrefabFolderPreview(folderPath);
        else
            //Else shows a message in console that the path doesn't exist
            EditorGUILayout.HelpBox("Prefabs folder does not exist.", MessageType.Warning);
        
        //Ends 2D block in scene view
        Handles.EndGUI();

        //Shows preview of the prefab selected in scene
        PreviewPrefab();
    }
    
    private void PrefabFolderPreview(string path)
    {
        //Gets the info of directory using the path
        DirectoryInfo dirInfo = new DirectoryInfo(path);
        //Hashes out only the elements within the directory that are prefabs
        FileInfo[] prefabs = dirInfo.GetFiles("*.prefab");

        //Starts a box on the left side of scene view to show the prefabs selectable
        Rect layoutRect = new Rect(10f, 10f, 180f, Screen.height - 100f);
        GUI.BeginGroup(layoutRect);
        
        GUILayout.Space(15);
        
        //Starts a scroll view in case the prefabs selectable exceed scene view area
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        
        //For every prefab found in directory
        foreach (FileInfo prefab in prefabs)
        {
            //Converts absolute path into Unity's relative path
            string relPath = path + "/" + prefab.Name;
            
            //Sets reference of prefab in path
            GameObject filePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(relPath);
            //Gets the 2D texture (image) of prefab to preview in GUI
            Texture2D texturePreview = AssetPreview.GetAssetPreview(filePrefab);

            //If no selectedPrefab is present (usually when opening tool window), sets the first prefab found as selected
            if (selectedPrefab == null)
                selectedPrefab = filePrefab;
            
            //If the 2D texture is present
            if (texturePreview != null)
            {
                //Creates a button with the image of prefab that sets the selectedPrefab as the prefab shown
                if (GUILayout.Button(texturePreview, GUILayout.Width(160), GUILayout.Height(80)))
                {
                    selectedPrefab = filePrefab;
                }
            }
            else
            {
                //Else does the same above but without the image of prefab
                if (GUILayout.Button("Generating Preview...", GUILayout.Width(160), GUILayout.Height(80)))
                {
                    selectedPrefab = filePrefab;
                }
            }

            GUILayout.Space(5);
        }

        //Creates a button that performs Unity's built in Undo method (ctrl + z)
        if (GUILayout.Button("Undo", GUILayout.Height(80), GUILayout.Width(160)))
        {
            Undo.PerformUndo();
        }
        
        //Ends the scroll slider
        GUILayout.EndScrollView();

        //Closes box created above
        GUI.EndGroup();
    }

    private void PreviewPrefab()
    {
        #region Prefab Spawn
        //If there is no previewPrefab present and user has selected a prefab
        if (previewPrefab == null && selectedPrefab != null)
        {
            //Instantiates in scene that selected prefab
            previewPrefab = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            
            //Clears previewDoors List in case there are already elements inside
            previewDoors.Clear();
            //Gets from List constructor all children game object in the previewPrefab with tag "Door"
            previewDoors = FindGameObjectsWithTagInChildren(previewPrefab, "Door");
            
            //Saves this selectedPrefab as a reference in case user changes selectedPrefab
            lastSelectedPrefab = selectedPrefab;
        }
        //If the selectedPrefab saved and current selectedPrefab are different from each other when preview prefab is in scene
        else if (previewPrefab != null && lastSelectedPrefab != selectedPrefab)
        {
            //Destroys the previewPrefab in scene
            DestroyImmediate(previewPrefab);
            //and instantiates the selectedPrefab to update the preview
            previewPrefab = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
            
            //Clears all elements within previewDoors List
            previewDoors.Clear();
            //Gets all children game objects with tag "Door" within
            previewDoors = FindGameObjectsWithTagInChildren(previewPrefab, "Door");
            
            //Saves this selectedPrefab as a reference
            lastSelectedPrefab = selectedPrefab;
        }
        #endregion
        
        #region Prefab Movement
        //Creates reference of the current event being processed in editor
        Event e = Event.current;
        
        //Creates a Ray at the position of cursor on scene view
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        
        //Creates a plane at ground level (0y)
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        
        //Casts a raycast onto the plane using cursor position, returns a float value that indicates cursor position at ground level in scene
        if (groundPlane.Raycast(ray, out float distance))
        {
            //Sets previewPosition as the cursor position at ground level in scene
            previewPosition = ray.GetPoint(distance);

            //As long toggle in range visibility toggle is set to true in tool window
            if (rangeVisible)
            {
                //Shows a visible circle around cursor position in scene view
                Handles.color = Color.black;
                Handles.DrawWireDisc(previewPosition, Vector3.up, range);
                SceneView.RepaintAll();
            }

            //If the previewPrefab is present
            if (previewPrefab != null)
            {
                //Moves the previewPrefab to cursor position
                previewPrefab.transform.position = previewPosition;
                //Flushes transform changes to physics engine manually to avoid jittering when snapping doors
                Physics.SyncTransforms();
                //Starts the check to possibly snap doors
                TryToSnapDoors();
            }
        }
        #endregion
        
        #region User Commands
        //Runs the contents inside when shift and E key are pressed together
        if (e.type == EventType.KeyDown && e.shift && e.keyCode == KeyCode.E)
        {
            //Skips if the previewPrefab is not present
            if (previewPrefab == null)
                return;
            
            //Rotates the previewed room 90 degrees clockwise
            previewPrefab.transform.Rotate(0f, 90f, 0f, Space.World);

            //Uses this method so this event action doesn't run multiple times
            e.Use();
        }
        //Runs the contents inside when shift and Q key are pressed together
        else if (e.type == EventType.KeyDown && e.shift && e.keyCode == KeyCode.Q)
        {
            //Skips if the previewPrefab is not present
            if (previewPrefab == null)
                return;
            
            //Rotates the previewed room 90 degrees counter-clockwise
            previewPrefab.transform.Rotate(0f, -90f, 0f, Space.World);
            
            //Uses this method so this event action doesn't run multiple times
            e.Use();
        }

        //Runs when the event is a mouse left click
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            //Executes the lower function only when the previewPrefab is present and is snapped to a door
            if (previewPrefab == null || !isSnapped)
                return;

            //Instantiates the room in scene at snapped position
            PlaceRoom(previewPrefab.transform.position, previewPrefab.transform.rotation);
        
            //Uses this method so this event action doesn't run multiple times
            e.Use();
        }
        #endregion
    }

    private void TryToSnapDoors()
    {
        //Creates a reference to store world doors hit
        List<GameObject> hitWorldDoors = new List<GameObject>();
        
        //Creates a layer mask that get the layer with name "Door"
        LayerMask doorMask = LayerMask.GetMask("Door");

        //Casts an overlap sphere at cursor point that detects doors
        Collider[] hitDoors = Physics.OverlapSphere(previewPosition, range, doorMask);

        //For each door hit in overlap sphere
        foreach (Collider hitDoor in hitDoors)
        {
            //Checks if that door has previewPrefab as it's parent
            if (hitDoor.transform.IsChildOf(previewPrefab.transform))
                continue;       //if so, checks the next door in for loop
            
            //Adds the hitDoor as an element of hitWorldDoors List
            hitWorldDoors.Add(hitDoor.gameObject);
        }

        //If no world doors were found
        if (hitWorldDoors.Count <= 0)
        {
            //isSnapped is set to false and stops the function
            isSnapped = false;
            return;
        }
        
        //Creates references for best distance, preview and world door
        GameObject bestPreviewDoor = null;
        GameObject bestWorldDoor = null;
        float bestDistance = float.MaxValue;
        
        //Checks every preview door available
        foreach (GameObject previewDoor in previewDoors)
        {
            //and every world door detected for possible snap
            foreach (GameObject worldDoor in hitWorldDoors)
            {
                //First casts an overlap sphere on top of world door to check if there are overlapping doors (already snapped)
                if (Physics.OverlapSphere(worldDoor.transform.position, 0.001f, doorMask).Length >= 2)
                    continue;       //If there are overlapping doors, goes to next world door check in for loop
                
                //Uses Dot to see if the door forwards are facing opposite direction (returns positive value if they are)
                float facingDot = Vector3.Dot(previewDoor.transform.forward, -worldDoor.transform.forward);

                //Calculates the direction from cursor position and worldDoor position
                Vector3 directionToWorldDoor = (previewPosition - worldDoor.transform.position).normalized;
                //By using the worldDoor's forward and direction above, calculates another Dot to see if cursor is anywhere in front of worldDoor
                float directionalDot = Vector3.Dot(worldDoor.transform.forward, directionToWorldDoor);
                
                //Checks if the Dot calculation meet the requirements
                if (facingDot > 0.9f && directionalDot > 0f)
                {
                    //If so, Calculates the distance from cursor position and worldDoor
                    float distance = Vector3.Distance(previewPosition, worldDoor.transform.position);
                    
                    //If the distance calculated is less than current best distance
                    if (distance < bestDistance)
                    {
                        //Sets this as the best distance
                        bestDistance = distance;
                        //and sets the preview and world door in this check as best doors
                        bestPreviewDoor = previewDoor;
                        bestWorldDoor = worldDoor;
                    }
                }
            }
        }
        
        //If a snap candidate was found in the for loops
        if (bestPreviewDoor != null && bestWorldDoor != null)
        {
            //Sets isSnapped to true
            isSnapped = true;
            //and snaps the best preview and world doors together
            SnapDoors(bestPreviewDoor, bestWorldDoor);
        }
        //If no snap is possible to make, isSnapped is set to false
        else
            isSnapped = false; 
    }

    private void SnapDoors(GameObject previewDoor, GameObject worldDoor)
    {
        //Calculates the distance and direction from previewDoor snapped to center of previewPrefab
        Vector3 offset = previewDoor.transform.position - previewPrefab.transform.position;
        //Then with that offset, calculates to desired position of room by using the worldDoor position so the two doors are positioned together
        Vector3 targetPos = worldDoor.transform.position - offset;

        //Changes position of previewPrefab based on targetPos calculated
        previewPrefab.transform.position = targetPos;
    }

    private void PlaceRoom(Vector3 position, Quaternion rotation)
    {
        //Checks for the container to place placed rooms in
        ContainerCheck();

        //Instantiates the selected room in scene
        GameObject room = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab, container.transform);

        //Pushes this room creation action in Unity's built in Undo method (ctrl + z)
        Undo.RegisterCreatedObjectUndo(room, "Place Room");
        
        //Changes room position and rotation based on parameter values
        room.transform.position = position;
        room.transform.rotation = rotation;
    }
    
    private void ContainerCheck()
    {
        //If container is null, tries to find the game object with name in parameter
        if (container == null)
            container = GameObject.Find("Placed Rooms");
        
        //If container is still null, creates game object with that name
        if (container == null)
            container = new GameObject("Placed Rooms");
    }
    
    private List<GameObject> FindGameObjectsWithTagInChildren(GameObject parent, string tag)
    {
        //Creates a list to store all doors found in parent
        List<GameObject> childrenWithTag = new List<GameObject>();
        //Cycles through every child transform in parent
        foreach (Transform child in parent.transform)
        {
            //If a child object has the tag passed through parameter tag
            if (child.gameObject.CompareTag(tag))
                //Adds the game object to childrenWithTag List
                childrenWithTag.Add(child.gameObject);
        }
        //After cycling through all children, returns the modified List
        return childrenWithTag;
    }

    private void ClearPreview()
    {
        //Destroys the room preview in scene
        if (previewPrefab != null)
            DestroyImmediate(previewPrefab);

        //Removes all elements from previewDoors List
        previewDoors.Clear();
    }
}
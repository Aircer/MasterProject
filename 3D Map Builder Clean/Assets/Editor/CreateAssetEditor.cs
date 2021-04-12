using UnityEditor;
using UnityEngine;
using System.IO;
using MapTileGridCreator.Utilities;

[CanEditMultipleObjects]
public class CreateAssetEditor : EditorWindow
{
    private string _path_palletAsset = "Assets/Cells/NewCell/default.prefab";
    private string _path_coordinatesAsset = "Assets/Cells/NewCell/coordinates.prefab";
    private string _path_defaultMeshAsset = "Assets/Cells/NewCell/defaultMesh.prefab";
    private string _path_palletSaveAsset = "Assets/Cells/Pallets/";
    private CellInformation newCellInformation;
    private GameObject newCell;
    Editor newCellEditor;
    private Texture2D bgTexture;
    private bool meshBoxesShow;
    private Vector3 scaleMesh;
    private Vector3 positionMesh;
    private Vector3 rotationMesh;
    private GameObject meshCell;
    private Vector2 scrollPos;

    [MenuItem("3D Map/CreateAssetEditor")]
    static void ShowWindow()
    {
        EditorWindow window = GetWindow(typeof(CreateAssetEditor));
        window.Show();
    }

    private void OnDisable()
    {
        if (newCellEditor != null)
        {
            DestroyImmediate(newCellEditor.target);
            DestroyImmediate(newCellEditor);
        }   
    }

    private void OnGUI()
    {
        GUIStyle bg = new GUIStyle();
        //Initialize RectOffset object
        bg.border = new RectOffset(2, 2, 2, 2);
        bg.normal.background = bgTexture;

        scrollPos =
            EditorGUILayout.BeginScrollView(scrollPos);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("New"))
        {
            CreateCell(_path_palletAsset);
        }

        if (GUILayout.Button("Load"))
        {
            string fullpath = EditorUtility.OpenFilePanel("Load Cell", "", "prefab");
            string relativepath = "Assets" + fullpath.Substring(Application.dataPath.Length);
            if (fullpath != "")
            {
                CreateCell(relativepath); 
            }
        }
        GUILayout.EndHorizontal();

        if (newCellEditor != null && newCellEditor.target != null)
        {
            newCellEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(0.5f * position.width, 200), bg);
        }

        if(newCell != null)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Mesh Editor: ", EditorStyles.boldLabel);
            if (GUILayout.Button("Load Mesh"))
            {
                string fullpath = EditorUtility.OpenFilePanel("File asset load", "", "prefab");
                string relativepath = "Assets" + fullpath.Substring(Application.dataPath.Length);
                if (relativepath != "")
                {
                    GameObject newMeshCell = AssetDatabase.LoadAssetAtPath(relativepath, typeof(GameObject)) as GameObject;
                    newMeshCell = PrefabUtility.InstantiatePrefab(newMeshCell) as GameObject;
                    newMeshCell.transform.parent = newCell.transform;
                    PrefabUtility.UnpackPrefabInstance(newMeshCell, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    DestroyImmediate(meshCell);
                    meshCell = newMeshCell;
                    scaleMesh = meshCell.transform.localScale;
                    rotationMesh = meshCell.transform.localEulerAngles;
                    positionMesh = meshCell.transform.localPosition;
                    DestroyImmediate(newCellEditor);
                    newCellEditor = Editor.CreateEditor(newCell);
                }
            }

            GUILayout.EndHorizontal();
            EditorGUI.BeginChangeCheck();
            meshBoxesShow = EditorGUILayout.Toggle("Hide mesh boxes: ", meshBoxesShow);
            if (EditorGUI.EndChangeCheck()) ShowMeshBoxes();
            EditorGUI.BeginChangeCheck();
            positionMesh = EditorGUILayout.Vector3Field("Position Mesh: ", positionMesh);
            scaleMesh = EditorGUILayout.Vector3Field("Scale Mesh: ", scaleMesh);
            rotationMesh = EditorGUILayout.Vector3Field("Rotation Mesh: ", rotationMesh);
            if (EditorGUI.EndChangeCheck()) UpdateMeshShape();

            FuncEditor.DrawUILine(Color.gray);
            EditorGUILayout.LabelField("Properties Editor: ", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            newCell.name = EditorGUILayout.TextField("Name : ", newCell.name);
            newCellInformation.size = EditorGUILayout.Vector3IntField("Size: ", newCellInformation.size);
            if (EditorGUI.EndChangeCheck()) UpdateDefaultMeshes(); 
            newCellInformation.ground = EditorGUILayout.Toggle("Ground: ", newCellInformation.ground);
            newCellInformation.blockPath = EditorGUILayout.Toggle("Block Path: ", newCellInformation.blockPath);
            FuncEditor.DrawUILine(Color.gray);
          
            if (GUILayout.Button("Save"))
            {
                GameObject cellToSave = newCell;
                foreach (Transform child in cellToSave.transform)
                {
                    if (child.name == "coordinates")
                        DestroyImmediate(child.gameObject);
                }

                PrefabUtility.SaveAsPrefabAsset(cellToSave, _path_palletSaveAsset + cellToSave.name + ".prefab");
                MapTileGridCreatorWindow mapWindow = (MapTileGridCreatorWindow)Resources.FindObjectsOfTypeAll(typeof(MapTileGridCreatorWindow))[0];
                mapWindow.RefreshPallet();
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void CreateCell(string path)
    {
        if (newCellEditor != null && newCellEditor.target != null)
            DestroyImmediate(newCellEditor.target);
        DestroyImmediate(newCellEditor);
        newCell = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
        newCell = PrefabUtility.InstantiatePrefab(newCell) as GameObject;
        newCell.transform.localPosition = new Vector3(1000, 1000, 1000);
        PrefabUtility.UnpackPrefabInstance(newCell, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        newCellInformation = newCell.GetComponent<CellInformation>();
        newCellEditor = Editor.CreateEditor(newCell);

        foreach (Transform child in newCell.transform)
        {
            if (child.name != "coordinates" && child.name != "defaultMeshes")
            {
                meshCell = child.gameObject;
                scaleMesh = meshCell.transform.localScale;
                rotationMesh = meshCell.transform.localEulerAngles;
                positionMesh = meshCell.transform.localPosition;
            }
        }

        GameObject coordinates = AssetDatabase.LoadAssetAtPath(_path_coordinatesAsset, typeof(GameObject)) as GameObject;
        coordinates = PrefabUtility.InstantiatePrefab(coordinates) as GameObject;
        PrefabUtility.UnpackPrefabInstance(coordinates, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        coordinates.transform.parent = newCell.transform;
        coordinates.transform.localPosition = new Vector3(0, 0, 0);
        meshBoxesShow = true;
        UpdateDefaultMeshes();
    }

    private void UpdateDefaultMeshes()
    {
        if(newCell.transform.Find("defaultMeshes") != null)
            DestroyImmediate(newCell.transform.Find("defaultMeshes").gameObject);

        GameObject defaultMeshes = new GameObject();
        defaultMeshes.name = "defaultMeshes";
        defaultMeshes.transform.parent = newCell.transform;
        defaultMeshes.transform.localPosition = new Vector3(0, 0, 0);
        GameObject[,,] defaultMesh = new GameObject[newCellInformation.size.x, newCellInformation.size.y, newCellInformation.size.z];

        for (int i=0; i < newCellInformation.size.x; i++)
        {
            for (int j = 0; j < newCellInformation.size.y; j++)
            {
                for (int k = 0; k < newCellInformation.size.z; k++)
                {
                    defaultMesh[i,j,k] = AssetDatabase.LoadAssetAtPath(_path_defaultMeshAsset, typeof(GameObject)) as GameObject;
                    defaultMesh[i, j, k] = PrefabUtility.InstantiatePrefab(defaultMesh[i, j, k]) as GameObject;
                    PrefabUtility.UnpackPrefabInstance(defaultMesh[i, j, k], PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    defaultMesh[i, j, k].transform.parent = newCell.transform.Find("defaultMeshes").transform;
                    defaultMesh[i, j, k].transform.localPosition = new Vector3Int(i, j, k);
                }
            }
        }

        newCellEditor = Editor.CreateEditor(newCell);
    }

    private void ShowMeshBoxes()
    {
        if (meshBoxesShow)
            UpdateDefaultMeshes();
        else
        {
            if (newCell.transform.Find("defaultMeshes") != null)
                DestroyImmediate(newCell.transform.Find("defaultMeshes").gameObject);

            DestroyImmediate(newCellEditor);
            newCellEditor = Editor.CreateEditor(newCell);
        }
    }

    private void UpdateMeshShape()
    {
        meshCell.transform.localPosition = positionMesh;
        meshCell.transform.localScale = scaleMesh;
        meshCell.transform.localEulerAngles = rotationMesh;

        DestroyImmediate(newCellEditor);
        newCellEditor = Editor.CreateEditor(newCell);
    }
}

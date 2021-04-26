using UnityEditor;
using UnityEngine;
using System.IO;
using MapTileGridCreator.UtilitiesMain;

[CanEditMultipleObjects]
public class NewAssetEditor : EditorWindow
{
    private string _path_palletAsset = "Assets/Cells/NewCell";
    private string _path_palletSaveAsset;
    private CellInformation newCellInformation;
    private GameObject newCell;
    Editor newCellEditor;
    private Texture2D bgTexture;

    [MenuItem("3D Map/NewAssetEditor")]
    static void ShowWindow()
    {
        NewAssetEditor window = (NewAssetEditor)GetWindow(typeof(NewAssetEditor));
        window.Show();
    }

    private void OnEnable()
    {
        _path_palletSaveAsset = Application.dataPath + "Assets/Cells/Pallets";
    }

    private void OnGUI()
    {
        GUIStyle bg = new GUIStyle();
        //Initialize RectOffset object
        bg.border = new RectOffset(2, 2, 2, 2);
        bg.normal.background = bgTexture;

        if (GUILayout.Button("New"))
        {
            if(newCellEditor != null && newCellEditor.target != null)
                DestroyImmediate(newCellEditor.target);
            DestroyImmediate(newCellEditor);
            string[] prefabFiles = Directory.GetFiles(_path_palletAsset, "*.prefab");
            newCell = AssetDatabase.LoadAssetAtPath(prefabFiles[0], typeof(GameObject)) as GameObject;
            newCell = PrefabUtility.InstantiatePrefab(newCell) as GameObject;
            PrefabUtility.UnpackPrefabInstance(newCell, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            newCellInformation = newCell.GetComponent<CellInformation>();
            newCellEditor = Editor.CreateEditor(newCell);
        }

        if (newCellEditor != null && newCellEditor.target != null)
        {
            newCellEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(0.5f * position.width, 200), bg);
        }

        newCell.name = EditorGUILayout.TextField("Name : ", newCell.name);
        FuncMain.DrawUILine(Color.gray);
        newCellInformation.size = EditorGUILayout.Vector3IntField("Size: ", newCellInformation.size);
        newCellInformation.ground = EditorGUILayout.Toggle("Ground: ", newCellInformation.ground);
        newCellInformation.blockPath = EditorGUILayout.Toggle("Block Path: ", newCellInformation.blockPath);
        FuncMain.DrawUILine(Color.gray);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load"))
        {
            string fullpath = EditorUtility.OpenFilePanel("File asset load", "", "prefab");
            string relativepath = "Assets" + fullpath.Substring(Application.dataPath.Length);
            if (relativepath != "")
            {
                foreach (Transform child in newCell.transform)
                {
                    if (child.name != "Coordinates")
                        DestroyImmediate(child.gameObject);
                }

                GameObject newChild = AssetDatabase.LoadAssetAtPath(relativepath, typeof(GameObject)) as GameObject;
                newChild = PrefabUtility.InstantiatePrefab(newChild) as GameObject;
                newChild.transform.parent = newCell.transform;
                PrefabUtility.UnpackPrefabInstance(newChild, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

                newCellEditor = Editor.CreateEditor(newCell);
            }
        }

        if (GUILayout.Button("Save"))
        {
            //string fullpath = EditorUtility.SaveFilePanel("File asset save", "", newCell.name, "prefab");
            GameObject cellToSave = newCell;
            foreach (Transform child in cellToSave.transform)
            {
                if (cellToSave.name == "Coordinates")
                    DestroyImmediate(child.gameObject);
            }

            PrefabUtility.SaveAsPrefabAsset(cellToSave, _path_palletSaveAsset);
        }
        GUILayout.EndHorizontal();
    }
}

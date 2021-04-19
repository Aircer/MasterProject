// Simple script that lets you render the main camera in an editor Window.

using UnityEngine;
using UnityEditor;

public class test : EditorWindow
{
    private MapTileGridCreatorWindow mapWindow;
    [MenuItem("Example/Camera viewer")]
    static void Init()
    {
        EditorWindow editorWindow = GetWindow(typeof(test));
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
    }

    public void OnEnable()
    {

    }

    public void Update()
    {
        Repaint();
    }

    void OnGUI()
    {
        if (mapWindow == null)
            mapWindow = (MapTileGridCreatorWindow)Resources.FindObjectsOfTypeAll(typeof(MapTileGridCreatorWindow))[0];

        //GUILayoutUtility.GetRect(0.5f * position.width, position.height * 0.5f - 25);

        Camera previewCam = mapWindow.GetSuggestionGrid()[0].transform.GetComponentInChildren<Camera>();
        previewCam.hideFlags = HideFlags.HideAndDontSave;

        GUILayoutUtility.GetRect(0.5f * position.width, position.height * 0.5f - 25);
        Rect cameraRect = new Rect(5, 10, position.width*0.5f -10, position.height*0.5f - 30);
        Handles.DrawCamera(cameraRect, previewCam, DrawCameraMode.Normal);

        GUILayoutUtility.GetRect(0.5f * position.width, position.height * 0.5f - 25);
        cameraRect = new Rect(5 + position.width * 0.75f, position.height * 0.5f, position.width - 10, position.height*0.5f - 30);
        Handles.DrawCamera(cameraRect, previewCam, DrawCameraMode.Normal);
    }
}
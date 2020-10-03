using UnityEditor;
using UnityEngine;
using System.Collections;

public class GUIWindowDemo : EditorWindow
{
    // The position of the window
    FrameOutputData fod;
    Jiba jiba;
    public Rect windowRect = new Rect(100, 100, 200, 200);
    Dajiaba k;
    void OnGUI()
    {
        //BeginWindows();

        // All GUI.Window or GUILayout.Window must come inside here
        //windowRect = GUILayout.Window(1, windowRect, DoWindow, "Hi There");
        //Selection.activeObject = k;
        //EndWindows();
    }

    private void OnEnable()
    {
        Debug.Log("GUIWindowDemo");
        k = ScriptableObject.CreateInstance<Dajiaba>();
    }

    // The window function. This works just like ingame GUI.Window
    void DoWindow(int unusedWindowID)
    {
        GUILayout.Button("Hi");
        GUI.DragWindow();
    }

    // Add menu item to show this demo.
    [MenuItem("Test/GUIWindow Demo")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(GUIWindowDemo));
        
    }
}

public class Jiba : Object
{
    public FrameOutputData fod;
    public Jiba()
    {
        fod = new FrameOutputData();
    }
}

public class Dajiaba : Editor
{
    FrameOutputData fod; //脚本本体
    
    public Dajiaba()
    {
        fod = new FrameOutputData();
    }
}

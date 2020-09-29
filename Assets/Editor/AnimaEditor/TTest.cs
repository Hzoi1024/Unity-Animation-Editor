using UnityEditor;
using UnityEngine;
using System.Collections;

public class GUIWindowDemo : EditorWindow
{
    // The position of the window
    public Rect windowRect = new Rect(100, 100, 200, 200);
    void OnGUI()
    {
        BeginWindows();

        // All GUI.Window or GUILayout.Window must come inside here
        windowRect = GUILayout.Window(1, windowRect, DoWindow, "Hi There");

        EndWindows();
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

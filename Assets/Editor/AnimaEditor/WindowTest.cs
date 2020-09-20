// Editor Script that shows the mouse movement events
// captured.  With "Receive Movement" set to true the position of the
// mouse over the window will be reported.
// "Mouse Position" shows where the mouse is outside of the window.

using UnityEditor;
using UnityEngine;
using System.Collections;

public class Example : EditorWindow
{
    [MenuItem("Example/Mouse Move Example")]
    static void InitWindow()
    {
        Example window = (Example)GetWindowWithRect(typeof(Example), new Rect(0, 0, 300, 100));
        window.Show();
    }

    private void Update()
    {
        Debug.Log("hotcontrol:" + GUIUtility.hotControl);
    }

    void OnGUI()
    {
        Rect r = new Rect(10, 10, 100, 100);
        EditorGUI.DrawRect(r, Color.blue);
        if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            Event.current.Use();
            if (r.Contains(Event.current.mousePosition)){
                GUIUtility.hotControl = 999;
            }
        }

        if(GUIUtility.hotControl==999&& Event.current.rawType== EventType.MouseUp)
        {
            Debug.Log("hahahaha");
        }


    }
}

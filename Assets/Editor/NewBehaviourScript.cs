using UnityEditor;
using UnityEngine;
using System.Collections;

//Select the dependencies of the found GameObject
public class EditorGUIObjectField : EditorWindow
{
    public Texture2D obj = null;
    [MenuItem("Examples/Select Dependencies")]
    static void Init()
    {
        UnityEditor.EditorWindow window = GetWindow(typeof(EditorGUIObjectField));
        window.position = new Rect(0, 0, 250, 80);
        window.Show();
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }

    void OnGUI()
    {
        obj = (Texture2D)EditorGUI.ObjectField(new Rect(3, 3, position.width - 6, 20), "Find Dependency", obj, typeof(Texture2D));
    }
}

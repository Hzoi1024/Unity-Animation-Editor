using UnityEditor;
using UnityEngine;


// Shows info of a GameObject depending on the selected option

public enum OPTIONS
{
    Position = 0,
    Rotation = 1,
    Scale = 2,
}


public class EditorGUIEnumPopup : EditorWindow
{
    OPTIONS display = OPTIONS.Position;

    [MenuItem("Examples/Editor GUI Enum Popup usage")]
    static void Init()
    {
        EditorWindow window = GetWindow(typeof(EditorGUIEnumPopup));
        window.position = new Rect(0, 0, 220, 80);
        window.Show();
    }
    

    void OnGUI()
    {
        Rect k = new Rect();

        k.x =10;
        k.y = 10;
        k.width = position.width-20;
        k.height = position.height- 20;
        



        GUI.Box(k, "");
        

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AminaEditor_LoadAtlas : EditorWindow
{
    AminaEditor aminaEditor;
    //AminaEditor.AminaEditorComponetData clipData;
    GUIStyle boxStyle;
    GUIStyle nilStyle = new GUIStyle();
    GUIStyle verScrol;
    List<EditorAminaAtlas> allClips;

    Rect selGrid;
    int clipHeight = 20;
    string[] strs;
    int index = 1;
    int indexOld = 1;
    float scroll;
    int BottomHeight = 20;

    double lastCliTime;

    private void OnLostFocus()
    {
        Close();
    }

    public void Init(AminaEditor _aminaEditor)
    {
        Debug.Log("旧版");
        aminaEditor = _aminaEditor;
        allClips = new List<EditorAminaAtlas>();
        clipHeight = 20;
        string[] _all = System.IO.Directory.GetFiles(AminaEditorPath.ATLASPATH, "*.asset", System.IO.SearchOption.AllDirectories);
        List<string> _strs = new List<string>();
        for (int i = 0; i < _all.Length; i++)
        {
            EditorAminaAtlas _new = AssetDatabase.LoadAssetAtPath<EditorAminaAtlas>(_all[i]);
            if (_new != null)
            {
                allClips.Add(_new);
                _strs.Add(_new.name);
            }
        }
        strs = _strs.ToArray();
        boxStyle = new GUIStyle();
        boxStyle.padding = new RectOffset(10, 0, 0, 0);
        boxStyle.onNormal.background = aminaEditor.scriptableObject.Blue;
        boxStyle.onNormal.textColor = Color.white;
        boxStyle.alignment = TextAnchor.MiddleLeft;
        scroll = 0;
        //boxStyle.alignment = TextAnchor.MiddleLeft;
        lastCliTime = -10;
        //AssetDatabase.LoadAssetAtPath<EditorAminaComponentClip>("Assets/Editor/AnimaEditor/Res/Frames/");
        selGrid = new Rect(0, 10, position.width, clipHeight * allClips.Count);
        verScrol = new GUIStyle(GUI.skin.verticalScrollbar);
        index = 0;
        indexOld = -1;
    }

    Event currentEvent;


    private void OnGUI()
    {
        bool isDown = false;

        currentEvent = Event.current;
        if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
        {
            if (selGrid.Contains(currentEvent.mousePosition))
            {
                isDown = true;
            }
        }

        scroll = GUI.BeginScrollView(new Rect(0, selGrid.y, position.width, position.height - BottomHeight), new Vector2(0, scroll), selGrid).y;
        index = GUI.SelectionGrid(selGrid, index, strs, 1, boxStyle);

        if (isDown)
        {
            double delTime = EditorApplication.timeSinceStartup - lastCliTime;
            if (index == indexOld && delTime < 0.5)
            {
                Debug.LogError("aminaEditor.LoadAtlas(allClips[index])");
                //aminaEditor.LoadAtlas(allClips[index]);
                Close();
            }

            indexOld = index;

            lastCliTime = EditorApplication.timeSinceStartup;
        }

        //text = (EditorAminaComponentClip)EditorGUI.ObjectField(new Rect(10, 100, 100, 30), text,typeof(EditorAminaComponentClip),false);
        GUI.EndScrollView();



    }


}

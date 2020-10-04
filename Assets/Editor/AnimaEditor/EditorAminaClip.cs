using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "EditorAminaClip", menuName = "Custom/EditorAminaClip")]
public class EditorAminaClip : ScriptableObject
{
    public int ID;
    public OriginAminType Protype;
    public string Name;
    public List<int> compFramesID;
    public int Blank;
    public bool isLoop;
}

public class EditorAminaClipLoadWindow : EditorWindow
{
    AminaEditor aminaEditor;
    int clipHeight = 20;
    float scroll;
    int BottomHeight = 20;
    List<EditorAminaClip> allClips;
    string[] strs;
    GUIStyle boxStyle;
    GUIStyle nilStyle = new GUIStyle();
    GUIStyle verScrol;
    Rect selGrid;
    double lastCliTime;
    int index = 1;
    int indexOld = 1;
    public void Init(AminaEditor _aminaEditor, OriginAminType _protype)
    {
        aminaEditor = _aminaEditor;
        scroll = 0;
        allClips = new List<EditorAminaClip>();
        string[] _all = System.IO.Directory.GetFiles(AminaEditorPath.CLIPPATH, "*.asset", System.IO.SearchOption.AllDirectories);
        List<string> _strs = new List<string>();
        for (int i =0; i < _all.Length; i++)
        {
            EditorAminaClip _new = AssetDatabase.LoadAssetAtPath<EditorAminaClip>(_all[i]);
            if (_new != null && _new.Protype == _protype)
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
                aminaEditor.LoadEditorAminaClip(allClips[index]);
                Close();
            }

            indexOld = index;

            lastCliTime = EditorApplication.timeSinceStartup;
        }
        //text = (EditorAminaComponentClip)EditorGUI.ObjectField(new Rect(10, 100, 100, 30), text,typeof(EditorAminaComponentClip),false);
        GUI.EndScrollView();

        if(GUI.Button(new Rect(10, position.height - BottomHeight,70, BottomHeight), "Rename"))
        {
            Rename();
        }

    }

    private void OnLostFocus()
    {
        Close();
    }

    private void Rename()
    {
        string[] _all = System.IO.Directory.GetFiles(AminaEditorPath.CLIPPATH, "*.asset", System.IO.SearchOption.AllDirectories);
        List<string> _strs = new List<string>();
        for (int i = 0; i < _all.Length; i++)
        {
            EditorAminaClip _new = AssetDatabase.LoadAssetAtPath<EditorAminaClip>(_all[i]);
            string _newName = _new.ID + " " + _new.Protype + "_" + _new.Name;
            if (_new != null)
            {
                AssetDatabase.RenameAsset(_all[i], _newName);
            }
        }

        for(int i = 0; i < allClips.Count; i++)
        {
            strs[i] = allClips[i].name;
        }
        Repaint();
    }


}

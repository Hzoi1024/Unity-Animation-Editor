using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum EditorAtlasWindowLoadType
{
    Charactor,
    AnimaOnly,
}

public class EditorAtlasWindow : EditorWindow
{
    AminaEditor aminaEditor;
    float scroll;
    int index = 1;
    int indexOld = 1;
    EditorAtlasWindowLoadType originDisplay;
    int topHeight=20;
    string[] atlasPaths;
    Rect selGrid;
    int clipHeight = 20;
    int BottomHeight = 40;
    GUIStyle boxStyle;
    GUIStyle nilStyle = new GUIStyle();
    GUIStyle verScrol;
    double lastCliTime;
    List<AnimaDataCharactorScritable> allCharactors;
    List<AnimaDataOnlyScritable> allAnimaOnly;

    public bool isSonWindow;

    public void Init(AminaEditor _aminaEditor)
    {
        aminaEditor = _aminaEditor;
        scroll = 0;
        originDisplay = EditorAtlasWindowLoadType.Charactor;
        LoadConfig(originDisplay);

        boxStyle = new GUIStyle();
        boxStyle.padding = new RectOffset(10, 0, 0, 0);
        boxStyle.onNormal.background = aminaEditor.scriptableObject.Blue;
        boxStyle.onNormal.textColor = Color.white;
        boxStyle.alignment = TextAnchor.MiddleLeft;

        lastCliTime = -10;
        //AssetDatabase.LoadAssetAtPath<EditorAminaComponentClip>("Assets/Editor/AnimaEditor/Res/Frames/");
        //selGrid = new Rect(0, 10, position.width, clipHeight * allClips.Count);
        verScrol = new GUIStyle(GUI.skin.verticalScrollbar);
        index = 0;
        indexOld = -1;
        isSonWindow = false;
    }

    Event currentEvent;

    private void OnGUI()
    {
        bool isDown = false;

        EditorAtlasWindowLoadType choseType = (EditorAtlasWindowLoadType)EditorGUI.EnumPopup(
            new Rect(0, 0, 100, topHeight), originDisplay);
        if(choseType!= originDisplay)
        {
            LoadConfig(choseType);
            originDisplay = choseType;
        }

        currentEvent = Event.current;
        if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
        {
            if (selGrid.Contains(currentEvent.mousePosition))
            {
                isDown = true;
            }
        }

        scroll = GUI.BeginScrollView(new Rect(0, selGrid.y, position.width, position.height - BottomHeight-topHeight), new Vector2(0, scroll), selGrid).y;
        index = GUI.SelectionGrid(selGrid, index, atlasPaths, 1, boxStyle);

        if (isDown)
        {
            double delTime = EditorApplication.timeSinceStartup - lastCliTime;
            if (index == indexOld && delTime < 0.5)
            {
                switch (originDisplay)
                {
                    case EditorAtlasWindowLoadType.Charactor:
                        aminaEditor.LoadCharactorAtlas(allCharactors[index]);
                        break;
                    case EditorAtlasWindowLoadType.AnimaOnly:
                        aminaEditor.LoadOnlyAtlas(allAnimaOnly[index]);
                        break;
                }
                isSonWindow = true;
                Close();
            }

            indexOld = index;

            lastCliTime = EditorApplication.timeSinceStartup;
        }
        //text = (EditorAminaComponentClip)EditorGUI.ObjectField(new Rect(10, 100, 100, 30), text,typeof(EditorAminaComponentClip),false);
        GUI.EndScrollView();


        if(GUI.Button(new Rect(10, position.height - BottomHeight+10, 80, clipHeight), "Add New"))
        {
            if(originDisplay== EditorAtlasWindowLoadType.Charactor)
            {
                isSonWindow = true;
                EditorAtlasWindow_AddNewWindow atlasLoadWindow = GetWindow<EditorAtlasWindow_AddNewWindow>(false, "AddNewCharactor", true);
                
                atlasLoadWindow.position = new Rect(position.x+10, position.y-10, 250, 200);
                atlasLoadWindow.Init(this,GetNewChID());
            }
            
        }

        if (GUI.Button(new Rect(position.width-10-80, position.height - BottomHeight + 10, 80, clipHeight), "Delete"))
        {
            DeleteFrames(index);

        }

    }

    public void LoadConfig(EditorAtlasWindowLoadType _type)
    {
        string _loadPath="";

        switch (_type)
        {
            case EditorAtlasWindowLoadType.Charactor:
                _loadPath = AminaEditorPath.ATLASCHARACTORPATH;
                allCharactors = new List<AnimaDataCharactorScritable>();
                string[] _all = System.IO.Directory.GetFiles(_loadPath, "*.asset", System.IO.SearchOption.AllDirectories);
                Debug.Log("load ch path:"+ _loadPath);
                List<string> _strs = new List<string>();
                Debug.Log("all count=" + _all.Length);
                for (int i = 0; i < _all.Length; i++)
                {
                    Debug.Log("all path=" + _all[i]);
                    AnimaDataCharactorScritable _new = AssetDatabase.LoadAssetAtPath<AnimaDataCharactorScritable>(_all[i]);
                    var _allll= AssetDatabase.LoadAllAssetsAtPath(_all[i]);
                    Debug.Log("Object length=" + _allll.Length);
                    if (_allll[0])
                    {
                        Debug.Log("Object type=" + _allll[0].GetType());
                    }
                    /*
                    if (_allll.Length == 1)
                    {
                        AnimaDataCharactorScritable _neww = _allll[0] as AnimaDataCharactorScritable;
                        if (_neww != null)
                        {
                            Debug.Log("not null "+ _neww.name);
                        }
                    }*/

                    if (_new != null)
                    {
                        Debug.Log("_new != null");
                        allCharactors.Add(_new);
                        //_strs.Add(_new.name);
                    }
                }
                allCharactors.Sort();

                Debug.Log("ch count = "+allCharactors.Count);
                for (int i = 0; i < allCharactors.Count; i++)
                {
                    _strs.Add(allCharactors[i].name);
                }

                atlasPaths = _strs.ToArray();

                selGrid = new Rect(0, topHeight, position.width, clipHeight * allCharactors.Count);

                break;
            case EditorAtlasWindowLoadType.AnimaOnly:
                Debug.LogWarning("AnimaOnly");

                break;
        }
    }

    private int GetNewChID()
    {
        //allCharactors.Sort();
        int result = 1;
        for(int i = 0; i < allCharactors.Count; i++)
        {
            if (result != allCharactors[i].ChID) break;
            result++;
        }
        return result;
    }

    private int GetNewAnimaOnlyID()
    {
        //allAnimaOnly.Sort();
        int result = 1;
        for (int i = 0; i < allAnimaOnly.Count; i++)
        {
            if (result != allAnimaOnly[i].ID) break;
            result++;
        }
        return result;
    }

    private void OnLostFocus()
    {
        if (!isSonWindow)
        {
            Close();
        }
    }
    
    private void DeleteFrames(int _index)
    {
        //AssetDatabase.DeleteAsset(clipsPath[_id]);
        switch (originDisplay)
        {
            case EditorAtlasWindowLoadType.Charactor:
                AnimaFilesRecycle.DeleteFrames(AssetDatabase.GetAssetPath(allCharactors[_index]));
                allCharactors.RemoveAt(_index);
                List<string> _str = new List<string>();
                for(int i = 0; i < allCharactors.Count; i++)
                {
                    _str.Add(allCharactors[i].name);
                }
                atlasPaths = _str.ToArray();

                if (index > 0)
                {
                    index--;
                }
                selGrid.height = clipHeight * allCharactors.Count;


                break;
            case EditorAtlasWindowLoadType.AnimaOnly:
                Debug.LogWarning("Del atlas only");
                break;

        }
    }

}

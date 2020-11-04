using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AminaEditor_SaveFrames : EditorWindow
{
    AminaEditor aminaEditor;
    //AminaEditor.AminaEditorComponetData clipData;
    int CompID;
    GUIStyle boxStyle;
    GUIStyle nilStyle = new GUIStyle();
    GUIStyle verScrol;
    EditorAminaComponentClip saveClip;
    List<EditorAminaComponentClip> allClips;
    Dictionary<int, string> clipsPath;

    Rect selGrid;
    int clipHeight = 20;
    string[] strs;
    int index = 1;
    //int indexOld = 1;
    float scroll;
    int BottomHeight;

    int saveID;
    int oldSaveID;
    string coveredStr;
    string saveName;
    //double lastCliTime;

    private void OnLostFocus()
    {
        Close();
    }

    public void Init(AminaEditor _aminaEditor, EditorAminaComponentClip _saveClip)
    {
        aminaEditor = _aminaEditor;
        saveClip = _saveClip;
        CompID = saveClip.CompID;
        allClips = new List<EditorAminaComponentClip>();
        clipHeight = 20;
        string[] _all = System.IO.Directory.GetFiles(AminaEditorPath.FRAMEPATH, "*.asset", System.IO.SearchOption.AllDirectories);
        List<string> _strs = new List<string>();
        clipsPath = new Dictionary<int, string>();
        saveName = "";
        EditorAminaComponentClip _newEC = ScriptableObject.CreateInstance<EditorAminaComponentClip>();
        _newEC.Name = "New Frames";
        _strs.Add(_newEC.Name);
        allClips.Add(_newEC);
        //indexOld = 0;
        index = 0;
        int j = 1;
        for (int i = 0; i < _all.Length; i++)
        {
            EditorAminaComponentClip _new = AssetDatabase.LoadAssetAtPath<EditorAminaComponentClip>(_all[i]);


            if (_new != null && _new.CompID == CompID)
            {
                int _id = _new.ID;
                clipsPath.Add(_new.ID, _all[i]);
                allClips.Add(_new);
                _strs.Add(_new.name);
                if (_id == saveClip.ID) {
                    index = j;
                    oldSaveID = _new.ID%1000;
                    saveName = _new.Name;
                    coveredStr = "Cover";
                }

                j++;
            }
        }

        _newEC.ID = GetNotCoveredID();

        if (index == 0)
        {
            oldSaveID = _newEC.ID % 1000;

            coveredStr = "Save";
        }

        saveID = oldSaveID;
        strs = _strs.ToArray();
        boxStyle = new GUIStyle();
        boxStyle.padding = new RectOffset(10, 0, 0, 0);
        boxStyle.onNormal.background = aminaEditor.scriptableObject.Blue;
        boxStyle.onNormal.textColor = Color.white;
        boxStyle.alignment = TextAnchor.MiddleLeft;
        scroll = 0;
        //boxStyle.alignment = TextAnchor.MiddleLeft;
        //lastCliTime = -10;
        //AssetDatabase.LoadAssetAtPath<EditorAminaComponentClip>("Assets/Editor/AnimaEditor/Res/Frames/");
        selGrid = new Rect(0, 10, position.width, clipHeight * allClips.Count);
        verScrol = new GUIStyle(GUI.skin.verticalScrollbar);

        BottomHeight = 90;
        
    }

    Event currentEvent;
    bool mouseDown;

    private void OnGUI()
    {
        mouseDown = false;
        
        currentEvent = Event.current;
        scroll = GUI.BeginScrollView(new Rect(0, selGrid.y, position.width, position.height - BottomHeight - selGrid.y), new Vector2(0, scroll), new Rect(0, selGrid.y, position.width - 15, selGrid.height)).y;

        if (currentEvent.type == EventType.MouseUp)
        {
            if (currentEvent.button == 0)
            {
                if (selGrid.Contains(currentEvent.mousePosition))
                {
                    mouseDown = true;
                }
            }
        }

        index = GUI.SelectionGrid(selGrid, index, strs, 1, boxStyle);

        if (mouseDown)
        {
            oldSaveID = allClips[index].ID % 1000;
            RefrashSaveButtonString(oldSaveID);

            if (index != 0)
            {
                saveName = allClips[index].Name;
            }
        }


        GUI.EndScrollView();
        
        GUI.Label(new Rect(10, position.height - BottomHeight, 100, clipHeight), "CompID:" + CompID.ToString());
        saveID = EditorGUI.DelayedIntField(new Rect(110, position.height - BottomHeight, 100, clipHeight), oldSaveID);

        if (saveID != oldSaveID)
        {
            if (saveID > 999 || saveID<1)
            {
                Debug.LogWarning("Input Number should less 999 and bigger 0");
            }
            else
            {
                RefrashSaveButtonString(saveID);

                oldSaveID = saveID;
            }
        }

        GUI.Label(new Rect(10, position.height - BottomHeight+clipHeight, 100, clipHeight), "ID:" + (CompID*1000+saveID).ToString());
        saveName= GUI.TextField(new Rect(10, position.height - BottomHeight + clipHeight * 2, 200, clipHeight), saveName);
        if (GUI.Button(new Rect(10, position.height - BottomHeight + clipHeight * 3, 80, clipHeight), coveredStr))
        {
            int _id = CompID * 1000 + saveID;
            
            if (clipsPath.ContainsKey(_id))
            {
                //AssetDatabase.DeleteAsset(clipsPath[_id]);
                AnimaFilesRecycle.DeleteFrames(clipsPath[_id]);
            }
            saveClip.ID = _id;
            saveClip.Name = saveName;
            AssetDatabase.CreateAsset(saveClip, AminaEditorPath.FRAMEPATH + _id.ToString()+" " + saveName + ".asset");
            Close();
        }

        if (index != 0)
        {
            if (GUI.Button(new Rect(position.width - 100, position.height - BottomHeight + clipHeight * 3, 80, clipHeight), "Delete"))
            {
                DeleteFrames(index);
            }
        }
        


    }

    private void OnDestroy()
    {
        aminaEditor.aminaEditor_SaveFrames = null;
    }

    private void RefrashSaveButtonString(int _idLast3)
    {
        if (clipsPath.ContainsKey(CompID * 1000 + _idLast3))
        {
            coveredStr = "Cover";
        }
        else
        {
            coveredStr = "Save";
        }

    }

    private void DeleteFrames(int _index)
    {
        int _i = (int)_index;
        int _id = allClips[_i].ID;
        //AssetDatabase.DeleteAsset(clipsPath[_id]);
        AnimaFilesRecycle.DeleteFrames(clipsPath[_id]);
        allClips.RemoveAt(_i);
        clipsPath.Remove(_id);
        selGrid = new Rect(0, 10, position.width, clipHeight * allClips.Count);
        strs = new string[allClips.Count];
        strs[0] = allClips[0].Name;
        for (int i = 1; i < strs.Length; i++)
        {
            strs[i] = allClips[i].name;
        }

        index--;
        RefrashSaveButtonString(oldSaveID);
    }


    private int GetNotCoveredID()
    {
        for (int i = CompID * 1000 + 1; i< (CompID + 1) * 1000; i++)
        {
            if (!clipsPath.ContainsKey(i))
            {
                return i;
            }
        }
        Debug.LogWarning("GetNotCoveredID failed, maybe the Frames is overed 1000");
        return 0;
    }

}

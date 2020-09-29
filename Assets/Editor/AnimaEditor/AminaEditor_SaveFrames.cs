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
    int indexOld = 1;
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
        index = 0;
        int j = 0;
        int k = -1;
        bool isGetSaveID = false;
        for (int i = 0; i < _all.Length; i++)
        {
            EditorAminaComponentClip _new = AssetDatabase.LoadAssetAtPath<EditorAminaComponentClip>(_all[i]);

            clipsPath.Add(_new.ID, _all[i]);

            if (_new != null && _new.CompID == CompID)
            {
                int _id = _new.ID;

                if (!isGetSaveID)
                {
                    if (k == -1)
                    {
                        k = _id;
                    }
                    else
                    {
                        if (k != _id)
                        {
                            oldSaveID = k%100;
                            isGetSaveID = true;
                        }
                        else
                        {
                            k++;
                        }
                    }
                }

                allClips.Add(_new);
                _strs.Add(_new.name);
                if (_id == saveClip.ID) {
                    indexOld = j;
                }

                j++;
            }
        }

        if (!isGetSaveID)
        {
            oldSaveID = k % 100;
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

        BottomHeight = 120;

        coveredStr = "Save";
        saveName = "";
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
        index = GUI.SelectionGrid(selGrid, indexOld, strs, 1, boxStyle);

        if (indexOld != index)
        {
            indexOld = index;
        }
        
        if (isDown)
        {
            //double delTime = EditorApplication.timeSinceStartup - lastCliTime;
            


            //lastCliTime = EditorApplication.timeSinceStartup;
        }

        GUI.EndScrollView();
        
        GUI.Label(new Rect(10, position.height - BottomHeight, 100, clipHeight), "CompID:" + CompID.ToString());
        saveID = EditorGUI.DelayedIntField(new Rect(110, position.height - BottomHeight, 100, clipHeight), oldSaveID);

        if (saveID != oldSaveID)
        {
            if (saveID > 999 || saveID<1)
            {
                Debug.LogError("Input Number should less 999 and bigger 0");
            }
            else
            {
                if (clipsPath.ContainsKey(CompID * 1000 + saveID))
                {
                    coveredStr = "Cover";
                }
                else
                {
                    coveredStr = "Save";
                }
                oldSaveID = saveID;
            }
        }

        GUI.Label(new Rect(10, position.height - BottomHeight+clipHeight, 100, clipHeight), "ID:" + (CompID*1000+saveID).ToString());
        saveName= GUI.TextField(new Rect(10, position.height - BottomHeight + clipHeight * 2, 200, clipHeight), saveName);

        if(GUI.Button(new Rect(10, position.height - BottomHeight + clipHeight * 3, 80, clipHeight), coveredStr))
        {
            int _id = CompID * 1000 + saveID;
            
            if (clipsPath.ContainsKey(_id))
            {
                AssetDatabase.DeleteAsset(clipsPath[_id]);
            }
            saveClip.ID = _id;
            AssetDatabase.CreateAsset(saveClip, AminaEditorPath.FRAMEPATH + _id.ToString()+" " + saveName + ".asset");
            Close();
        }

    }

    private void OnDestroy()
    {
        aminaEditor.aminaEditor_SaveFrames = null;
    }
}

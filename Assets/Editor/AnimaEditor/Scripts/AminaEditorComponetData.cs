using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AminaEditorComponetData
{
    public GameObject ComponentGO;
    public int componentID;
    public EditorAminaComponentClip clip;
    public string componentName;
    public string labelStr;
    public List<SelectedKeyData> SelectionKeyFrame;
    public bool showComponentPara;
    public AminaEditorComponetData(int _componentID, GameObject protype)
    {
        componentID = _componentID;
        showComponentPara = false;
        switch (_componentID)
        {
            case 1://头                    
                ComponentGO = protype.transform.Find("Head").gameObject;
                componentName = "Head:";
                break;
            case 2://身体
                ComponentGO = protype.transform.Find("Body").gameObject;
                componentName = "Body:";
                break;
            case 3://前手
                ComponentGO = protype.transform.Find("FHand").gameObject;
                componentName = "FHand:";
                break;
            case 4://后手
                ComponentGO = protype.transform.Find("BHand").gameObject;
                componentName = "BHand:";
                break;
            case 5://前脚
                ComponentGO = protype.transform.Find("FLeg").gameObject;
                componentName = "FLeg:";
                break;
            case 6://后脚
                ComponentGO = protype.transform.Find("BLeg").gameObject;
                componentName = "BLeg:";
                break;

        }
        SelectionKeyFrame = new List<SelectedKeyData>();
        labelStr = componentName + "New Frames";
        //clip = new EditorAminaComponentClip();
        clip = ScriptableObject.CreateInstance<EditorAminaComponentClip>();
        clip.Init(0, componentID, ComponentGO.transform.localPosition, ComponentGO.transform.localEulerAngles.z);
    }

    public void ClearSelectionKeyFrame()
    {
        SelectionKeyFrame = new List<SelectedKeyData>();
    }

    public bool SelectionKeyContained(int _index, int _paraIndex)
    {
        if (clip.Frames.Count > _index)
        {
            foreach (SelectedKeyData _s in SelectionKeyFrame)
            {
                if (_s.Index == _index)
                {
                    switch (_paraIndex)
                    {
                        case 0:
                            if (_s.all) return true;
                            return false;
                        case 1:
                            if (_s.x) return true;
                            return false;
                        case 2:
                            if (_s.y) return true;
                            return false;
                        case 3:
                            if (_s.a) return true;
                            return false;
                        default:
                            Debug.LogWarning("SelectionKeyContained wrong  _paraIndex int =" + _paraIndex);
                            return false;
                    }
                }
            }
        }
        return false;
    }

    public void SelectionKeyFrameAdd(int _index, int _paraType)
    {
        if (clip.Frames.Count > _index && clip.Frames[_index].isKey)
        {


            foreach (SelectedKeyData _s in SelectionKeyFrame)
            {
                if (_s.Index == _index)
                {
                    switch (_paraType)
                    {
                        case 0:
                            if (clip.Frames[_index].XKey != KeyType.NotKey) _s.x = true;
                            if (clip.Frames[_index].YKey != KeyType.NotKey) _s.y = true;
                            if (clip.Frames[_index].AKey != KeyType.NotKey) _s.a = true;
                            _s.all = true;
                            //_s.all =_s.x = _s.y = _s.a = true;
                            break;
                        case 1:
                            if (clip.Frames[_index].XKey != KeyType.NotKey) _s.x = true;
                            break;
                        case 2:
                            if (clip.Frames[_index].YKey != KeyType.NotKey) _s.y = true;
                            break;
                        case 3:
                            if (clip.Frames[_index].AKey != KeyType.NotKey) _s.a = true;
                            break;
                        default:
                            Debug.LogWarning("SelectionKeyFrameAdd wrong  paraType int =" + _paraType);
                            break;
                    }
                    return;
                }
            }

            SelectedKeyData _newSD = new SelectedKeyData(_index);
            switch (_paraType)
            {
                case 0:
                    _newSD.all = true;
                    if (clip.Frames[_index].XKey != KeyType.NotKey) _newSD.x = true;
                    if (clip.Frames[_index].YKey != KeyType.NotKey) _newSD.y = true;
                    if (clip.Frames[_index].AKey != KeyType.NotKey) _newSD.a = true;
                    break;
                case 1:
                    if (clip.Frames[_index].XKey != KeyType.NotKey) _newSD.x = true;
                    break;
                case 2:
                    if (clip.Frames[_index].YKey != KeyType.NotKey) _newSD.y = true;
                    break;
                case 3:
                    if (clip.Frames[_index].AKey != KeyType.NotKey) _newSD.a = true;
                    break;
                default:
                    Debug.LogWarning("SelectionKeyFrameAdd wrong  paraType int =" + _paraType);
                    break;
            }

            SelectionKeyFrame.Add(_newSD);
        }
    }


    public SelectedKeyData GetSelectedData(int _index)
    {
        for (int i = 0; i < SelectionKeyFrame.Count; i++)
        {
            if (SelectionKeyFrame[i].Index == _index)
            {
                return SelectionKeyFrame[i];
            }
        }
        return null;
    }
}

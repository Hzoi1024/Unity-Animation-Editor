using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimaDataCharactorScritable_Container
{
    public int ChID;
    public List<EditorAnimaClipAtlas> Clips;
    public string ChName;
    public string Name;
    public GameObject Protype;
    public bool isFace;

    public void LoadAnimaDataCharactorScritable(AnimaDataCharactorScritable _chScriptble,GameObject _protype)
    {
        ChID = _chScriptble.ChID;
        ChName = _chScriptble.ChName;
        Name = _chScriptble.name;
        Protype = _protype;
        isFace = _chScriptble.isFace;

        Clips = _chScriptble.Clips;
    }

}

public class EditorAnimaClipAtlas_Container
{
    public int FPS;
    public AnimaCharactorType CharactorType;
    public List<EditorSpriteFrame_Container> Frames;
    public List<EditorSpriteFrame> realFrames;
    public List<FrameOutputData_Atlas> outputEvent;
    public List<SelectedKeyData> SelectionKeyFrame;
    private List<TempSpriteKey> TempKeys;//添加图片的临时帧坐标 x是横坐标 y是纵向
    private List<Vector2Int> TempK;
    private int TemkeyIndexOrigin;//临时坐标 鼠标index基点
    private int TemkeyIndexCurrent;//临时坐标 鼠标index偏移
    public int TemkeyIndexDelta { get { return TemkeyIndexCurrent - TemkeyIndexOrigin; } }
    public bool isHaveTempKeys
    {
        get
        {
            if (TempKeys != null && TempKeys.Count > 0) return true;
            return false;
        }
    }

    public bool showAtlasPara;
    public string AtlasName;

    public static int PARACOUNT = 2;//编辑器里展开参数有几行

    private GameObject protype;
    private SpriteRenderer spriteRenderer;
    private AminaEditor aminaEditor;
    private GameObject face;
    public bool isFace;

    public EditorAnimaClipAtlas_Container(AminaEditor _ae,GameObject _protype,GameObject _face)
    {
        protype = _protype;
        CharactorType = AnimaCharactorType.Def;
        FPS = 30;
        showAtlasPara = false;
        AtlasName = "New Atlas";
        spriteRenderer = _protype.GetComponent<SpriteRenderer>();
        aminaEditor = _ae;
        Frames = new List<EditorSpriteFrame_Container>();
        realFrames = new List<EditorSpriteFrame>();
        outputEvent = new List<FrameOutputData_Atlas>();
        SelectionKeyFrame = new List<SelectedKeyData>();
        TempKeys = null;
        TempK = null;
        face = _face;
        face.transform.localPosition = Vector3.zero;
        isFace = false;
    }

    public void LoadEditorAnimaClipAtlas(EditorAnimaClipAtlas _clip,bool _isFace=false)
    {
        Frames = new List<EditorSpriteFrame_Container>();
        realFrames = new List<EditorSpriteFrame>();
        outputEvent = new List<FrameOutputData_Atlas>();
        SelectionKeyFrame = new List<SelectedKeyData>();
        isFace = _isFace;

        if (_clip != null)
        {
            showAtlasPara = false;
            AtlasName = CharactorType.ToString();

            FPS = _clip.FPS;
            CharactorType = _clip.CharactorType;

            int _index = _clip.Frames[0].index;
            if (_index != 1)
            {
                Debug.LogWarning("first index !=0");
            }

            EditorSpriteFrame _prevFrame;
            EditorSpriteFrame _nextFrame = null;

            for (int i = 0; i < _clip.Frames.Count; i++)
            {
                realFrames.Add(new EditorSpriteFrame(_clip.Frames[i]));

                _prevFrame = _clip.Frames[i];
                if (i < _clip.Frames.Count - 1)
                {
                    _nextFrame = _clip.Frames[i + 1];
                }

                while (_index < _clip.Frames[i].index)
                {
                    EditorSpriteFrame_Container _newTran = new EditorSpriteFrame_Container();
                    _newTran.SetNewTransitionFrame(_prevFrame, _nextFrame, _index);

                    _index++;
                }

                EditorSpriteFrame_Container _newKey = new EditorSpriteFrame_Container();
                _newKey.SetNewKeyFrame(_clip.Frames[i]);
                Frames.Add(_newKey);

                _index++;
            }

            for (int i = 0; i < _clip.Output.Count; i++)
            {
                outputEvent.Add(new FrameOutputData_Atlas(_clip.Output[i]));
            }
        }
    }

    public void AddNewSpriteKey(Sprite _newSprite,int _index)
    {
        if (_index >= Frames.Count)//在最后一帧后面添加新帧
        {
            if (Frames.Count > 0)
            {
                EditorSpriteFrame_Container _prevFrame = Frames[Frames.Count-1];

                for (int i= Frames.Count; i< _index; i++)
                {
                    EditorSpriteFrame_Container _newTran = new EditorSpriteFrame_Container();
                    _newTran.sprite = _prevFrame.sprite;
                    _newTran.Alpha = _prevFrame.Alpha;
                    _newTran.facePos = _prevFrame.facePos;
                    Frames.Add(_newTran);
                }

                EditorSpriteFrame_Container _newKey = new EditorSpriteFrame_Container();
                _newKey.isKeySprite = true;
                _newKey.sprite = _newSprite;
                _newKey.Alpha = _prevFrame.Alpha;

                Frames.Add(_newKey);
            }
            else//第一次添加帧
            {
                if (_index > 0)
                {
                    EditorSpriteFrame_Container _newFirstKey = new EditorSpriteFrame_Container();
                    _newFirstKey.isKeySprite = true;
                    _newFirstKey.isKeyAlpha = true;
                    _newFirstKey.sprite = _newSprite;
                    _newFirstKey.Alpha = 100;

                    for (int i = 0; i < _index; i++)
                    {
                        EditorSpriteFrame_Container _newTran = new EditorSpriteFrame_Container();
                        _newTran.sprite = _newSprite;
                        _newTran.Alpha = 100;
                        Frames.Add(_newTran);
                    }
                }
                EditorSpriteFrame_Container _newKey = new EditorSpriteFrame_Container();
                _newKey.isKeySprite = true;
                _newKey.isKeyAlpha = true;
                _newKey.sprite = _newSprite;
                _newKey.Alpha = 100;
                Frames.Add(_newKey);
            }
            return;
        }

        Frames[_index].sprite = _newSprite;
        Frames[_index].isKeySprite = true;

        int k = _index + 1;
        while (k < Frames.Count && !Frames[k].isKeySprite)
        {
            Frames[k].sprite = _newSprite;
            Frames[k].facePos = default;
            k++;
        }
    }

    public void DeleteSpriteKey(int _index)
    {
        if (_index > Frames.Count - 1 || _index<0)
        {
            Debug.Log("删除的关键帧不存在");
            return;
        }

        if (!Frames[_index].isKeySprite)
        {
            Debug.LogError("删除的不是关键帧 index=" + _index.ToString()); return;
        }

        if (_index == Frames.Count - 1)
        {
            int k = _index;

            if (Frames[k].isKeyAlpha)
            {
                DeleteAlphaKey(k);
            }
            Frames.RemoveAt(k);
            k--;

            while (k>=0&& !Frames[k].isKeySprite)
            {
                if (Frames[k].isKeyAlpha)
                {
                    DeleteAlphaKey(k);
                }
                Frames.RemoveAt(k);
                k--;
            }
            return;
        }

        Sprite _prev = null;
        Vector3 _prevFace = Vector3.zero;
        if (_index > 0)
        {
            _prev = Frames[_index - 1].sprite;
            _prevFace = Frames[_index - 1].facePos;
        }
        Frames[_index].isKeySprite = false;
        Frames[_index].isKeyFace = false;
        int j = _index;

        while (j < Frames.Count && !Frames[j].isKeySprite)
        {
            Frames[j].sprite = _prev;
            Frames[j].facePos = _prevFace;
            j++;
        }
    }

    public void AddNewAlphaKey(int _alpha,int _index)
    {
        if (_index >= Frames.Count)
        {
            Debug.LogError("不能修改超过帧数的alpha");return;
        }

        int _prevA=100;
        int _nextA=100;

        int _pk = _index - 1;
        int _nk = _index + 1;

        while (_pk >= 0)
        {
            if (Frames[_pk].isKeyAlpha) break;
            _pk--;
        }

        if (_pk == -1)
        {
            if (Frames[_index].isKeyFrame)
            {
                _pk = _index;
            }
            else
            {
                Debug.LogError("不能在第一个关键帧之前修改alpha");
                return;
            }
        }

        while (_nk < Frames.Count)
        {
            if (Frames[_nk].isKeyAlpha) break;
            _nk++;
        }

        if (_nk >= Frames.Count)
        {
            /*if (Frames[_index].isKeyFrame)
            {
                _nk = _index;
            }
            else
            {
                Debug.LogError("不能在最后关键帧之后修改alpha");
                return;
            }*/
            _nk = _index;
        }

        Frames[_index].Alpha = _alpha;
        Frames[_index].isKeyAlpha = true;

        _prevA = Frames[_pk].Alpha;
        _nextA = Frames[_nk].Alpha;

        for(int i = _pk + 1; i < _index; i++)
        {
            Frames[i].SetTransitonAlpha(_prevA, _alpha, _pk, _index, i);
        }

        for (int i = _index + 1; i < _nk; i++)
        {
            Frames[i].SetTransitonAlpha(_alpha, _nextA, _index, _nk, i);
        }
    }

    public void DeleteAlphaKey(int _index)
    {
        if (_index >= Frames.Count || _index<0)
        {
            Debug.LogError("删除的key超过量程 index="+_index.ToString()); return;
        }

        if (!Frames[_index].isKeyAlpha)
        {
            Debug.LogError("删除的不是关键帧 index=" + _index.ToString());return;
        }

        int _prevA = 100;
        int _nextA = 100;

        int _pk = _index - 1;
        int _nk = _index + 1;



        while (_pk >= 0)
        {
            if (Frames[_pk].isKeyAlpha) break;
            _pk--;
        }

        while (_nk < Frames.Count)
        {
            if (Frames[_nk].isKeyAlpha) break;
            _nk++;
        }

        bool _isNoPrevKey = false;

        if (_pk == -1)//删的是第一个关键帧
        {
            _pk = 0;
            _isNoPrevKey = true;
        }

        bool _isNoNextKey = false;
        if (_nk >= Frames.Count)//删的是最后一个关键帧
        {
            _nk = Frames.Count - 1;
            _isNoNextKey = true;
        }

        Frames[_index].isKeyAlpha = false;

        if(!_isNoPrevKey && !_isNoNextKey)
        {
            _prevA = Frames[_pk].Alpha;
            _nextA = Frames[_nk].Alpha;
        }else if (!_isNoNextKey)
        {
            _nextA = Frames[_nk].Alpha;
            _prevA = _nextA;
        }else if (!_isNoPrevKey)
        {
            _prevA = Frames[_pk].Alpha;
            _nextA = _prevA;
        }

        for (int i = _pk + 1; i < _nk; i++)
        {
            Frames[i].SetTransitonAlpha(_prevA, _nextA, _pk, _nk, i);
        }

    }

    public void AddNewFacePos(Vector3 _pos,int _index)
    {
        if(_index>= Frames.Count)
        {
            Debug.LogError("addnewfacepos error,index>count.");
            return;
        }

        if (!Frames[_index].isKeySprite)
        {
            Debug.LogError("addnewfacepos error,index="+_index+" is not key sprite.");
            return;
        }

        Frames[_index].facePos = _pos;

        if(_index!= Frames.Count - 1)
        {
            int _nextIndex = _index + 1;

            while(_nextIndex<Frames.Count && !Frames[_nextIndex].isKeySprite)
            {
                Frames[_nextIndex].facePos = _pos;
                _nextIndex++;
            }
        }
    }

    public void AddFacePosKey(Vector2 _pos,int _index)
    {
        Debug.LogWarning("add face key");
    }

    public void DeleteFacePosKey(int _index)
    {
        Debug.LogWarning("Del face key");
    }

    /// <summary>
    /// 判断是否有关键帧
    /// </summary>
    /// <param name="_frameIndex">帧序号</param>
    /// <param name="_indexY">纵坐标，0是总帧，1是sprite，2是alpha</param>
    /// <returns></returns>
    public bool IsHaveIndexKey(int _frameIndex,int _indexY)
    {
        if (_frameIndex < Frames.Count)
        {
            switch (_indexY)
            {
                case 0:
                    return Frames[_frameIndex].isKeyFrame;
                case 1:
                    return Frames[_frameIndex].isKeySprite;
                case 2:
                    return Frames[_frameIndex].isKeyAlpha;
                default:
                    Debug.LogWarning("wrong indexY");
                    return false;
            }


        }
        return false;
    }

    public bool IsSelectionKeyContained(int _index, int _paraIndex)
    {
        if (Frames.Count > _index)
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
                            if (_s.sp) return true;
                            return false;
                        case 2:
                            if (_s.alpha) return true;
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
        if (Frames.Count > _index &&_index>=0&& Frames[_index].isKeyFrame)
        {
            foreach (SelectedKeyData _s in SelectionKeyFrame)
            {
                if (_s.Index == _index)
                {
                    switch (_paraType)
                    {
                        case 0:
                            if (Frames[_index].isKeySprite) _s.sp = true;
                            if (Frames[_index].isKeyAlpha) _s.alpha = true;
                            //if (clip.Frames[_index].AKey != KeyType.NotKey) _s.a = true;
                            _s.all = true;
                            break;
                        case 1:
                            if (Frames[_index].isKeySprite) _s.sp = true;
                            break;
                        case 2:
                            if (Frames[_index].isKeyAlpha) _s.alpha = true;
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
                    if (Frames[_index].isKeySprite) _newSD.sp = true;
                    if (Frames[_index].isKeyAlpha) _newSD.alpha = true;
                    break;
                case 1:
                    if (Frames[_index].isKeySprite) _newSD.sp = true;
                    break;
                case 2:
                    if (Frames[_index].isKeyAlpha) _newSD.alpha = true;
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
        SelectedKeyData _nullSd = new SelectedKeyData(_index);
        return _nullSd;
    }


    public void ClearSelectionKeyFrame()
    {
        SelectionKeyFrame = new List<SelectedKeyData>();
    }


    public GameObject GetAtlasGameObject()
    {
        return protype;
    }

    public GameObject GetAtlasFace()
    {
        return face;
    }

    public void GetSprite(int _index)
    {
        if (Frames.Count > _index)
        {
            spriteRenderer.sprite = Frames[_index].sprite;
            Color _c = spriteRenderer.color;
            _c.a = Frames[_index].Alpha/100f;
            spriteRenderer.color = _c;

            if (isFace)
            {
                Vector2 _pos = Frames[_index].facePos;
                face.transform.localPosition = _pos;
                face.transform.localEulerAngles = new Vector3(0, 0, Frames[_index].facePos.z);
            }

        }
    }

    public void SetTempSpriteKeys(List<TempSpriteKey> _keys,int _oriIndex)
    {
        TempKeys = _keys;

        TemkeyIndexOrigin = _oriIndex;
        TemkeyIndexCurrent = _oriIndex;
    }

    public void SetTemkeyIndexCurrent(int _delta)
    {
        TemkeyIndexCurrent = _delta;
    }

    public void GetTempKeys(out List<Vector2Int> _keys,out Color _color)
    {
        if (TempK == null)
        {
            TempK = new List<Vector2Int>();
            for (int i = 0; i < TempKeys.Count; i++)
            {
                TempK.Add(new Vector2Int(TempKeys[i].Index, 0));
            }

            if (showAtlasPara)
            {
                for (int i = 0; i < TempKeys.Count; i++)
                {
                    TempK.Add(new Vector2Int(TempKeys[i].Index, 1));
                }
            }
        }
        _keys= TempK;
        _color = Color.red;
    }

    public void TempkeysToAddNewSprite()
    {
        if (isHaveTempKeys)
        {
            int _d = TemkeyIndexDelta;

            for (int i=0;i< TempKeys.Count; i++)
            {
                TempKeys[i].Index += _d;
            }

            aminaEditor.AddSpriteKey(TempKeys);
            TempKeys = null;
        }
    }

    public void ClearTempKeys()
    {
        TempKeys = null;
        TempK = null;
    }
}

public class TempSpriteKey
{
    public Sprite Sp;
    public int Index;
}


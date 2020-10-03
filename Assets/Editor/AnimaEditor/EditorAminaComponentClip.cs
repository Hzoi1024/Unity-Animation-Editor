using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "EditorAminaComponentClip", menuName = "Custom/EditorAminaComponentClip")][Serializable]
public class EditorAminaComponentClip:ScriptableObject
{
    public static readonly float HALFPI=Mathf.PI/2;
    public int ID;
    public int CompID;
    public string Name;
    public int[] PauseTime;

    public int FramesCount { get { return Frames.Count; } }

    [SerializeField]
    public List<EditorAminaComponentClipData> Frames;
    public List<int> GeneralKeysIndex;
    [SerializeField]
    public List<FrameOutputData> OutData;

    [SerializeField]
    public AminaFrame defaultAminaFrame;
    
    public EditorAminaComponentClip()
    {
        Frames = new List<EditorAminaComponentClipData>();
        GeneralKeysIndex = new List<int>();
        OutData = new List<FrameOutputData>();
        defaultAminaFrame = new AminaFrame();
        defaultAminaFrame.pos = Vector2.zero;
        defaultAminaFrame.angle = 0;
        PauseTime = null;
    }

    public void Init(int _id,int _compID, Vector2 _xy,float _angle)
    {
        ID = _id;
        CompID = _compID;
        defaultAminaFrame.pos = new Vector2(_xy.x, _xy.y);
        defaultAminaFrame.angle = _angle;
    }

    public void Init(EditorAminaComponentClip _ori)
    {
        ID = _ori.ID;
        CompID = _ori.CompID;
        Name = _ori.Name;
        defaultAminaFrame.pos = new Vector2(_ori.defaultAminaFrame.pos.x, _ori.defaultAminaFrame.pos.y);
        defaultAminaFrame.angle = _ori.defaultAminaFrame.angle;
        if (_ori.PauseTime == null)
        {
            Debug.Log("pasue null");
        }
        PauseTime = _ori.PauseTime;

        for(int i = 0; i < _ori.GeneralKeysIndex.Count; i++)
        {
            GeneralKeysIndex.Add(_ori.GeneralKeysIndex[i]);
        }

        for (int i = 0; i < _ori.Frames.Count; i++)
        {
            EditorAminaComponentClipData _newframe = new EditorAminaComponentClipData(_ori.Frames[i]);
            Frames.Add(_newframe);
        }

        OutData = new List<FrameOutputData>();
        for (int i = 0; i < _ori.OutData.Count; i++)
        {
            FrameOutputData _new = new FrameOutputData(_ori.OutData[i]);
            OutData.Add(_new);
        }
    }

    public void Load(EditorAminaComponentClip _other)
    {
        if (_other.CompID != CompID)
        {
            Debug.LogWarning("LoadFailed,compID not equal");
            return;
        }

        Frames = new List<EditorAminaComponentClipData>();
        GeneralKeysIndex = new List<int>();
        PauseTime = null;
        defaultAminaFrame = new AminaFrame();
        defaultAminaFrame.pos = Vector2.zero;
        defaultAminaFrame.angle = 0;

        Init(_other);

    }

    #region
    /*
    public void AddKeyFrame(Vector2 _pos, float _angle,int _index, KeyType _Type)
    {
        bool isAddNewGeneralKey = true;
        bool isAddNewKey = false;

        if (Frames.Count > _index)
        {
            if (Frames[_index].isKey) isAddNewGeneralKey = false;
        }
        else
        {
            isAddNewKey = true;
        }

        if (isAddNewGeneralKey)
        {
            GeneralKeysIndex.Add(_index);
            GeneralKeysIndex.Sort();
        }

        //如果是超出范围 需要新建帧
        if (isAddNewKey)
        {
            //超出范围 且只有一帧 可以确定是新建
            if (GeneralKeysIndex.Count == 1)
            {
                for (int i = 0; i < _index; i++)
                {
                    EditorAminaComponentClipData _newCD = new EditorAminaComponentClipData(i);
                    _newCD.x = _pos.x;
                    _newCD.y = _pos.y;
                    _newCD.angle = _angle;
                    Frames.Add(_newCD);
                }
                EditorAminaComponentClipData _newKey = new EditorAminaComponentClipData(_index);
                _newKey.x = _pos.x; 
                _newKey.y = _pos.y;
                _newKey.angle = _angle;
                _newKey.XKey = _Type;
                _newKey.YKey = _Type;
                _newKey.AKey = _Type;
                Frames.Add(_newKey);
            }
            else//超出范围 前面有帧 另外两个属性取前面的最后一帧  修改属性向前找关键帧 没找到就把第一帧到最后一帧都变成_x
            {

                for (int i = Frames.Count + 1; i <= _index; i++)
                {
                    EditorAminaComponentClipData _newCD = new EditorAminaComponentClipData(i);
                    Frames.Add(_newCD);
                }
                Frames[_index].XKey = _Type;
                Frames[_index].x = _pos.x;
                Frames[_index].YKey = _Type;
                Frames[_index].y = _pos.y;
                Frames[_index].AKey = _Type;
                Frames[_index].angle = _angle;

                int _preXIndex = -1;
                int _preYIndex = -1;
                int _preAIndex = -1;
                for (int i = GeneralKeysIndex.Count - 2; i >= 0; i--)
                {
                    if (_preXIndex == -1 && Frames[GeneralKeysIndex[i]].XKey != KeyType.NotKey)
                    {
                        _preXIndex = i;
                    }
                    if (_preYIndex == -1 && Frames[GeneralKeysIndex[i]].YKey != KeyType.NotKey)
                    {
                        _preYIndex = i;
                    }
                    if (_preAIndex == -1 && Frames[GeneralKeysIndex[i]].AKey != KeyType.NotKey)
                    {
                        _preAIndex = i;
                    }
                }
                
                if (_preXIndex == -1)
                {
                    for (int i = 0; i < _index; i++)
                    {
                        Frames[i].x = _pos.x;
                    }
                }
                else
                {
                    FillFrame(_preXIndex, _index, _Type, FrameParaType.x);
                }

                if (_preYIndex == -1)
                {
                    for (int i = 0; i < _index; i++)
                    {
                        Frames[i].y = _pos.y;
                    }
                }
                else
                {
                    FillFrame(_preYIndex, _index, _Type, FrameParaType.y);
                }

                if (_preAIndex == -1)
                {
                    for (int i = 0; i < _index; i++)
                    {
                        Frames[i].angle = _angle;
                    }
                }
                else
                {
                    FillFrame(_preAIndex, _index, _Type, FrameParaType.a);
                }

            }
        }
        else
        {
            Frames[_index].x = _pos.x;
            Frames[_index].XKey = _Type;
            Frames[_index].y = _pos.y;
            Frames[_index].YKey = _Type;
            Frames[_index].angle = _angle;
            Frames[_index].AKey = _Type;

            int _k = GeneralKeysIndex.IndexOf(_index);

            int _preXIndex = -1;
            int _preYIndex = -1;
            int _preAIndex = -1;

            if (_k != 0)
            {
                for (int i = _k - 1; i >= 0; i--)
                {
                    if (_preXIndex==-1 && Frames[GeneralKeysIndex[i]].XKey != KeyType.NotKey)
                    {
                        _preXIndex = i;
                    }

                    if (_preYIndex == -1 && Frames[GeneralKeysIndex[i]].YKey != KeyType.NotKey)
                    {
                        _preYIndex = i;
                        break;
                    }

                    if (_preAIndex == -1 && Frames[GeneralKeysIndex[i]].AKey != KeyType.NotKey)
                    {
                        _preAIndex = i;
                        break;
                    }
                }
            }

            if (_preXIndex == -1)
            {
                for (int i = 0; i < _index; i++)
                {
                    Frames[i].x = _pos.x;
                }
            }
            else
            {
                FillFrame(_preXIndex, _index, _Type, FrameParaType.x);
            }

            if (_preYIndex == -1)
            {
                for (int i = 0; i < _index; i++)
                {
                    Frames[i].y = _pos.y;
                }
            }
            else
            {
                FillFrame(_preYIndex, _index, _Type, FrameParaType.y);
            }

            if (_preAIndex == -1)
            {
                for (int i = 0; i < _index; i++)
                {
                    Frames[i].angle = _angle;
                }
            }
            else
            {
                FillFrame(_preAIndex, _index, _Type, FrameParaType.a);
            }

            int _nextXIndex = -1;
            int _nextYIndex = -1;
            int _nextAIndex = -1;
            if (_k != GeneralKeysIndex.Count - 1)
            {
                for (int i = _k + 1; i < GeneralKeysIndex.Count; i++)
                {
                    if (_nextXIndex == -1 && Frames[GeneralKeysIndex[i]].XKey != KeyType.NotKey)
                    {
                        _nextXIndex = i;
                    }
                    if (_nextYIndex == -1 && Frames[GeneralKeysIndex[i]].YKey != KeyType.NotKey)
                    {
                        _nextYIndex = i;
                    }
                    if (_nextAIndex == -1 && Frames[GeneralKeysIndex[i]].AKey != KeyType.NotKey)
                    {
                        _nextAIndex = i;
                    }
                }
            }

            if (_nextXIndex == -1)
            {
                for (int i = _index + 1; i < Frames.Count; i++)
                {
                    Frames[i].x = _pos.x;
                }
            }
            else
            {
                FillFrame(_index, _nextXIndex, _Type, FrameParaType.x);
            }

            if (_nextYIndex == -1)
            {
                for (int i = _index + 1; i < Frames.Count; i++)
                {
                    Frames[i].y = _pos.y;
                }
            }
            else
            {
                FillFrame(_index, _nextYIndex, _Type, FrameParaType.y);
            }

            if (_nextAIndex == -1)
            {
                for (int i = _index + 1; i < Frames.Count; i++)
                {
                    Frames[i].angle = _angle;
                }
            }
            else
            {
                FillFrame(_index, _nextAIndex, _Type, FrameParaType.a);
            }
        }
    }*/

    /*public void AddKeyFrame(Vector2 _pos, float _angle, Key _newKey)
    {
        bool isAddNewKey = true ;

        for(int i = 0; i < keys.Count; i++)
        {
            if (keys[i].index == _newKey.index)
            {
                keys[i] = _newKey;
                isAddNewKey = false;
                break;
            }
        }

        if (isAddNewKey)
        {
            keys.Add(_newKey);
            keys.Sort();
        }

        if (keys.Count == 1)
        {
            frames = new List<AminaFrame>();

            for (int i = 0; i <= keys[0].index; i++)
            {
                AminaFrame _af = new AminaFrame();
                _af.pos = new Vector2(_pos.x, _pos.y);
                _af.angle = _angle;
                frames.Add(_af);
            }
            return;
        }

        int _index = _newKey.index;

        int k = GetIndexOf(_index);
        if (k == 0)
        {
            for (int i = 0; i <= _index; i++)
            {
                AminaFrame _af = new AminaFrame();
                _af.pos = new Vector2(_pos.x, _pos.y);
                _af.angle = _angle;
                frames[i] = _af;
            }

            int nextIndex = keys[k + 1].index;

            InsertFirstKeyFrame();
        }
        else if (k == keys.Count - 1)
        {
            int previousIndex = keys[k - 1].index;
            AminaFrame _end = new AminaFrame();
            _end.pos = new Vector2(_pos.x, _pos.y);
            _end.angle = _angle;
            //float _deltaAng = (_angle - frames[previousIndex].angle) / (_index - previousIndex);

            bool isAdd = false;
            if (_index > frames.Count - 1) isAdd = true;

            if (isAdd)
            {
                for (int i = 0; i < _index - previousIndex - 1; i++)
                {
                    frames.Add(null);
                }
                frames.Add(_end);
            }
            else
            {
                frames[_index] = _end;
            }

            switch (_newKey.type)
            {
                case KeyType.LinearKey:
                    LinearFillFrame(previousIndex, _index);
                    break;
                case KeyType.AccelerateKey:
                    AccelerateFillFrame(previousIndex, _index);
                    break;
                case KeyType.TrigonoKey:
                    TrangonoFillFrame(previousIndex, _index);
                    break;
                case KeyType.DecelerateKey:
                    DecelerateFillFrame(previousIndex, _index);
                    break;
            }
        }
        else
        {
            AminaFrame _k = new AminaFrame();
            _k.pos = new Vector2(_pos.x, _pos.y);
            _k.angle = _angle;
            frames[_index] = _k;

            InsertFrame(k);
        }
    }*/
    #endregion
    public void AddXKeyFrame(float _x, int _index,KeyType _keyType,IFrameFillPara _fillPara)
    {
        //Debug.Log("AddXKeyFrame");
        bool isAddNewGeneralKey = true;
        bool isAddNewKey = false;

        if(Frames.Count> _index)
        {
            if (Frames[_index].isKey) isAddNewGeneralKey = false;
        }
        else
        {
            isAddNewKey = true;
        }

        if (isAddNewGeneralKey)
        {
            GeneralKeysIndex.Add(_index);
            GeneralKeysIndex.Sort();
        }

        //如果是超出范围 需要新建帧
        if (isAddNewKey)
        {
            //超出范围 且只有一帧 可以确定是新建
            if (GeneralKeysIndex.Count == 1)
            {
                for(int i = 0; i < _index; i++)
                {
                    EditorAminaComponentClipData _newCD = new EditorAminaComponentClipData(i);
                    _newCD.x = _x;
                    _newCD.y = defaultAminaFrame.pos.y;
                    _newCD.angle = defaultAminaFrame.angle;
                    Frames.Add(_newCD);
                }
                EditorAminaComponentClipData _newKey = new EditorAminaComponentClipData(_index);
                _newKey.x = _x;
                _newKey.y = defaultAminaFrame.pos.y;
                _newKey.angle = defaultAminaFrame.angle;
                _newKey.XKey = _keyType;
                _newKey.XfillPara = _fillPara;
                Frames.Add(_newKey);
            }
            else//超出范围 前面有帧 另外两个属性取前面的最后一帧  修改属性向前找关键帧 没找到就把第一帧到最后一帧都变成_x
            {
                EditorAminaComponentClipData _last = Frames[Frames.Count - 1];
                float _preY= _last.y;
                float _preA= _last.angle;

                for (int i = Frames.Count; i <= _index; i++)
                {
                    EditorAminaComponentClipData _newCD = new EditorAminaComponentClipData(i);
                    _newCD.y = _preY;
                    _newCD.angle = _preA;
                    Frames.Add(_newCD);
                }
                Frames[_index].XKey = _keyType;
                Frames[_index].x = _x;
                Frames[_index].XfillPara = _fillPara;

                int _preXIndex = -1;
                for(int i = GeneralKeysIndex.Count - 2; i >= 0; i--)
                {
                    if(Frames[GeneralKeysIndex[i]].XKey!= KeyType.NotKey)
                    {
                        _preXIndex = GeneralKeysIndex[i];
                        break;
                    }
                }

                if (_preXIndex == -1)
                {
                    for(int i = 0; i < _index; i++)
                    {
                        Frames[i].x = _x;
                    }
                }
                else
                {
                    FillFrame(_preXIndex, _index, _keyType, FrameParaType.x,_fillPara);
                }
            }
        }
        else
        {
            Frames[_index].x = _x;
            Frames[_index].XKey = _keyType;
            Frames[_index].XfillPara = _fillPara;

            int _k = GeneralKeysIndex.IndexOf(_index);

            int _preXIndex = -1;
            if (_k != 0)
            {
                for (int i = _k-1; i >= 0; i--)
                {
                    if (Frames[GeneralKeysIndex[i]].XKey != KeyType.NotKey)
                    {
                        _preXIndex = GeneralKeysIndex[i];
                        break;
                    }
                }
            }

            if (_preXIndex == -1)
            {
                for (int i = 0; i < _index; i++)
                {
                    Frames[i].x = _x;
                }
            }
            else
            {
                FillFrame(_preXIndex, _index, _keyType, FrameParaType.x,_fillPara);
            }

            int _nextXIndex = -1;
            if (_k != GeneralKeysIndex.Count-1)
            {
                for (int i = _k + 1; i < GeneralKeysIndex.Count; i++)
                {
                    if (Frames[GeneralKeysIndex[i]].XKey != KeyType.NotKey)
                    {
                        _nextXIndex = GeneralKeysIndex[i];
                        break;
                    }
                }
            }

            if (_nextXIndex == -1)
            {
                for (int i = _index+1; i < Frames.Count; i++)
                {
                    Frames[i].x = _x;
                }
            }
            else
            {
                FillFrame(_index, _nextXIndex,Frames[_nextXIndex].XKey, FrameParaType.x,Frames[_nextXIndex].XfillPara);
            }
        }
    }

    public void AddYKeyFrame(float _y, int _index, KeyType _keyType, IFrameFillPara _fillPara)
    {
        //Debug.Log("Add y key");
        bool isAddNewGeneralKey = true;
        bool isAddNewKey = false;

        if (Frames.Count > _index)
        {
            if (Frames[_index].isKey) isAddNewGeneralKey = false;
        }
        else
        {
            isAddNewKey = true;
        }

        if (isAddNewGeneralKey)
        {
            GeneralKeysIndex.Add(_index);
            GeneralKeysIndex.Sort();
        }

        //如果是超出范围 需要新建帧
        if (isAddNewKey)
        {
            //超出范围 且只有一帧 可以确定是新建
            if (GeneralKeysIndex.Count == 1)
            {
                for (int i = 0; i < _index; i++)
                {
                    EditorAminaComponentClipData _newCD = new EditorAminaComponentClipData(i);
                    _newCD.x = defaultAminaFrame.pos.x;
                    _newCD.y = _y;
                    _newCD.angle = defaultAminaFrame.angle;
                    Frames.Add(_newCD);
                }
                EditorAminaComponentClipData _newKey = new EditorAminaComponentClipData(_index);
                _newKey.x = defaultAminaFrame.pos.x;
                _newKey.y = _y;
                _newKey.angle = defaultAminaFrame.angle;
                _newKey.YKey = _keyType;
                _newKey.YfillPara = _fillPara;
                Frames.Add(_newKey);
            }
            else//超出范围 前面有帧 另外两个属性取前面的最后一帧  修改属性向前找关键帧 没找到就把第一帧到最后一帧都变成_x
            {
                EditorAminaComponentClipData _last = Frames[Frames.Count - 1];
                float _preX = _last.x;
                float _preA = _last.angle;

                for (int i = Frames.Count; i <= _index; i++)
                {
                    EditorAminaComponentClipData _newCD = new EditorAminaComponentClipData(i);
                    _newCD.x = _preX;
                    _newCD.angle = _preA;
                    Frames.Add(_newCD);
                }
                Frames[_index].YKey = _keyType;
                Frames[_index].y = _y;
                Frames[_index].YfillPara = _fillPara;

                int _preYIndex = -1;
                for (int i = GeneralKeysIndex.Count - 2; i >= 0; i--)
                {
                    if (Frames[GeneralKeysIndex[i]].YKey != KeyType.NotKey)
                    {
                        _preYIndex = GeneralKeysIndex[i];
                        break;
                    }
                }

                if (_preYIndex == -1)
                {
                    for (int i = 0; i < _index; i++)
                    {
                        Frames[i].y = _y;
                    }
                }
                else
                {
                    FillFrame(_preYIndex, _index, _keyType, FrameParaType.y,_fillPara);
                }
            }
        }
        else
        {
            Frames[_index].y = _y;
            Frames[_index].YKey = _keyType;
            Frames[_index].YfillPara = _fillPara;

            int _k = GeneralKeysIndex.IndexOf(_index);

            int _preYIndex = -1;
            if (_k != 0)
            {
                for (int i = _k - 1; i >= 0; i--)
                {
                    if (Frames[GeneralKeysIndex[i]].YKey != KeyType.NotKey)
                    {
                        _preYIndex = GeneralKeysIndex[i];
                        break;
                    }
                }
            }

            if (_preYIndex == -1)
            {
                for (int i = 0; i < _index; i++)
                {
                    Frames[i].y = _y;
                }
            }
            else
            {
                FillFrame(_preYIndex, _index, _keyType, FrameParaType.y,_fillPara);
            }

            int _nextYIndex = -1;
            if (_k != GeneralKeysIndex.Count - 1)
            {
                for (int i = _k + 1; i < GeneralKeysIndex.Count; i++)
                {
                    if (Frames[GeneralKeysIndex[i]].YKey != KeyType.NotKey)
                    {
                        _nextYIndex = GeneralKeysIndex[i];
                        break;
                    }
                }
            }

            if (_nextYIndex == -1)
            {
                for (int i = _index + 1; i < Frames.Count; i++)
                {
                    Frames[i].y = _y;
                }
            }
            else
            {
                FillFrame(_index, _nextYIndex,Frames[_nextYIndex].YKey, FrameParaType.y, Frames[_nextYIndex].YfillPara);
            }
        }
    }

    public void AddAKeyFrame(float _a, int _index, KeyType _keyType, IFrameFillPara _fillPara)
    {
        bool isAddNewGeneralKey = true;
        bool isAddNewKey = false;

        if (Frames.Count > _index)
        {
            if (Frames[_index].isKey) isAddNewGeneralKey = false;
        }
        else
        {
            isAddNewKey = true;
        }

        if (isAddNewGeneralKey)
        {
            GeneralKeysIndex.Add(_index);
            GeneralKeysIndex.Sort();
        }

        //如果是超出范围 需要新建帧
        if (isAddNewKey)
        {
            //超出范围 且只有一帧 可以确定是新建
            if (GeneralKeysIndex.Count == 1)
            {
                for (int i = 0; i < _index; i++)
                {
                    EditorAminaComponentClipData _newCD = new EditorAminaComponentClipData(i);
                    _newCD.x = defaultAminaFrame.pos.x;
                    _newCD.y = defaultAminaFrame.pos.y;
                    _newCD.angle = _a;
                    Frames.Add(_newCD);
                }
                EditorAminaComponentClipData _newKey = new EditorAminaComponentClipData(_index);
                _newKey.x = defaultAminaFrame.pos.x;
                _newKey.y = defaultAminaFrame.pos.y;
                _newKey.angle = _a;
                _newKey.AKey = _keyType;
                _newKey.AfillPara = _fillPara;
                Frames.Add(_newKey);
            }
            else//超出范围 前面有帧 另外两个属性取前面的最后一帧  修改属性向前找关键帧 没找到就把第一帧到最后一帧都变成_x
            {
                EditorAminaComponentClipData _last = Frames[Frames.Count - 1];
                float _preX = _last.x;
                float _preY = _last.y;

                for (int i = Frames.Count; i <= _index; i++)
                {
                    EditorAminaComponentClipData _newCD = new EditorAminaComponentClipData(i);
                    _newCD.x = _preX;
                    _newCD.y = _preY;
                    Frames.Add(_newCD);
                }
                Frames[_index].AKey = _keyType;
                Frames[_index].angle = _a;
                Frames[_index].AfillPara = _fillPara;

                int _preAIndex = -1;
                for (int i = GeneralKeysIndex.Count - 2; i >= 0; i--)
                {
                    if (Frames[GeneralKeysIndex[i]].AKey != KeyType.NotKey)
                    {
                        _preAIndex = GeneralKeysIndex[i];
                        break;
                    }
                }

                if (_preAIndex == -1)
                {
                    for (int i = 0; i < _index; i++)
                    {
                        Frames[i].angle = _a;
                    }
                }
                else
                {
                    FillFrame(_preAIndex, _index, _keyType, FrameParaType.a, _fillPara);
                }
            }
        }
        else
        {
            Frames[_index].angle = _a;
            Frames[_index].AKey = _keyType;
            Frames[_index].AfillPara = _fillPara;

            int _k = GeneralKeysIndex.IndexOf(_index);

            int _preAIndex = -1;
            if (_k != 0)
            {
                for (int i = _k - 1; i >= 0; i--)
                {
                    if (Frames[GeneralKeysIndex[i]].AKey != KeyType.NotKey)
                    {
                        _preAIndex = GeneralKeysIndex[i];
                        break;
                    }
                }
            }

            if (_preAIndex == -1)
            {
                for (int i = 0; i < _index; i++)
                {
                    Frames[i].angle = _a;
                }
            }
            else
            {
                FillFrame(_preAIndex, _index, _keyType, FrameParaType.a, _fillPara);
            }

            int _nextAIndex = -1;
            if (_k != GeneralKeysIndex.Count - 1)
            {
                for (int i = _k + 1; i < GeneralKeysIndex.Count; i++)
                {
                    if (Frames[GeneralKeysIndex[i]].AKey != KeyType.NotKey)
                    {
                        _nextAIndex = GeneralKeysIndex[i];
                        break;
                    }
                }
            }

            if (_nextAIndex == -1)
            {
                for (int i = _index + 1; i < Frames.Count; i++)
                {
                    Frames[i].angle = _a;
                }
            }
            else
            {
                FillFrame(_index, _nextAIndex, Frames[_nextAIndex].AKey, FrameParaType.a,Frames[_nextAIndex].AfillPara);
            }
        }
    }
    #region
    /*
    public void DeleteKeyFrame(int _keyIndex)
    {
        bool isCotains = false;

        if (GeneralKeysIndex.Contains(_keyIndex) && Frames[_keyIndex].isKey)
        {
            isCotains = true;
        }

        if (!isCotains)
        {
            Debug.Log("can not delete the key,wrong key index");
            return;
        }

        if (GeneralKeysIndex.Count == 1)
        {
            GeneralKeysIndex = new List<int>();
            Frames = new List<EditorAminaComponentClipData>();
            return;
        }

        int k = GeneralKeysIndex.IndexOf(_keyIndex);

        int _preXIndex = -1;
        int _preYIndex = -1;
        int _preAIndex = -1;
        if (k != 0)
        {
            for (int i = k - 1; i >= 0; i--)
            {
                if (_preXIndex==-1&& Frames[GeneralKeysIndex[i]].XKey != KeyType.NotKey)
                {
                    _preXIndex = i;
                    break;
                }
                if (_preYIndex == -1 && Frames[GeneralKeysIndex[i]].YKey != KeyType.NotKey)
                {
                    _preYIndex = i;
                    break;
                }
                if (_preAIndex == -1 && Frames[GeneralKeysIndex[i]].AKey != KeyType.NotKey)
                {
                    _preAIndex = i;
                    break;
                }
            }
        }

        int _nextXIndex = -1;
        int _nextYIndex = -1;
        int _nextAIndex = -1;

        if (k != GeneralKeysIndex.Count - 1)
        {
            for (int i = k + 1; i < GeneralKeysIndex.Count; i++)
            {
                if (_nextXIndex==-1 && Frames[GeneralKeysIndex[i]].XKey != KeyType.NotKey)
                {
                    _nextXIndex = i;
                }
                if (_nextYIndex == -1 && Frames[GeneralKeysIndex[i]].YKey != KeyType.NotKey)
                {
                    _nextYIndex = i;
                }
                if (_nextAIndex == -1 && Frames[GeneralKeysIndex[i]].AKey != KeyType.NotKey)
                {
                    _nextAIndex = i;
                }
            }
        }
        else
        {
            //删除多余的帧  
            int _limit = GeneralKeysIndex[k - 1];

            for (int i = _keyIndex; i > _limit; i--)
            {
                Frames.RemoveAt(i);
            }

            float _newX;
            if (_preXIndex == -1)
            {
                _newX = defaultAminaFrame.pos.x;
            }
            else
            {
                _newX = Frames[_preXIndex].x;
            }
            for (int i = _preXIndex + 1; i < Frames.Count; i++)
            {
                Frames[i].x = _newX;
            }
            GeneralKeysIndex.RemoveAt(k);
            return;
        }

        Frames[_keyIndex].XKey = KeyType.NotKey;
        Frames[_keyIndex].YKey = KeyType.NotKey;
        Frames[_keyIndex].AKey = KeyType.NotKey;


        if (_preXIndex == -1 && _nextXIndex == -1)
        {
            for (int i = 0; i < Frames.Count; i++)
            {
                Frames[i].x = defaultAminaFrame.pos.x;
            }
        }
        else if (_preXIndex == -1)
        {
            float _endX = Frames[_nextXIndex].x;
            for (int i = 0; i < _nextXIndex; i++)
            {
                Frames[i].x = _endX;
            }
        }
        else if (_nextXIndex == -1)
        {
            float _preX = Frames[_preXIndex].x;
            for (int i = _preXIndex + 1; i < Frames.Count; i++)
            {
                Frames[i].x = _preX;
            }
        }
        else
        {
            FillFrame(_preXIndex, _nextXIndex, Frames[_nextXIndex].XKey, FrameParaType.x);
        }

        if (_preYIndex == -1 && _nextYIndex == -1)
        {
            for (int i = 0; i < Frames.Count; i++)
            {
                Frames[i].y = defaultAminaFrame.pos.y;
            }
        }
        else if (_preYIndex == -1)
        {
            float _endY = Frames[_nextYIndex].y;
            for (int i = 0; i < _nextYIndex; i++)
            {
                Frames[i].y = _endY;
            }
        }
        else if (_nextYIndex == -1)
        {
            float _preY = Frames[_preYIndex].y;
            for (int i = _preYIndex + 1; i < Frames.Count; i++)
            {
                Frames[i].y = _preY;
            }
        }
        else
        {
            FillFrame(_preYIndex, _nextYIndex, Frames[_nextYIndex].YKey, FrameParaType.y);
        }

        if (_preAIndex == -1 && _nextAIndex == -1)
        {
            for (int i = 0; i < Frames.Count; i++)
            {
                Frames[i].angle = defaultAminaFrame.angle;
            }
        }
        else if (_preAIndex == -1)
        {
            float _endA = Frames[_nextAIndex].angle;
            for (int i = 0; i < _nextAIndex; i++)
            {
                Frames[i].x = _endA;
            }
        }
        else if (_nextAIndex == -1)
        {
            float _preA = Frames[_preAIndex].angle;
            for (int i = _preAIndex + 1; i < Frames.Count; i++)
            {
                Frames[i].angle = _preA;
            }
        }
        else
        {
            FillFrame(_preAIndex, _nextAIndex, Frames[_nextXIndex].AKey, FrameParaType.a);
        }

        GeneralKeysIndex.RemoveAt(k);
    }*/

    /*public void DeleteKeyFrame(int _keyIndex)
    {
        bool isCotains = false;

        foreach(Key _key in keys)
        {
            if (_key.index == _keyIndex)
            {
                isCotains = true;
                break;
            }
        }

        if (!isCotains)
        {
            Debug.Log("can not delete the key,wrong key index");
            return;
        }

        if (keys.Count == 1)
        {
            keys = new List<Key>();
            frames = new List<AminaFrame>();
            return;
        }

        int k = GetIndexOf(_keyIndex);
        if (k == 0)
        {
            AminaFrame _nextFrame = frames[keys[k + 1].index];

            for(int i=0;i< keys[k + 1].index; i++)
            {
                AminaFrame _af = new AminaFrame();
                _af.pos = new Vector2(_nextFrame.pos.x, _nextFrame.pos.y);
                _af.angle = _nextFrame.angle;
                frames[i] = _af;
            }            
        }else 
        if(k == keys.Count - 1)
        {
            int preIndex = keys[k - 1].index;

            for(int i=_keyIndex;i> preIndex; i--)
            {
                frames.RemoveAt(i);
            }
        }
        else
        {
            int preIndex = keys[k - 1].index;

            Key nextKey = keys[k + 1];

            switch (nextKey.type)
            {
                case KeyType.AccelerateKey:
                    AccelerateFillFrame(preIndex, nextKey.index);
                    break;
                case KeyType.LinearKey:
                    LinearFillFrame(preIndex, nextKey.index);
                    break;
                case KeyType.TrigonoKey:
                    TrangonoFillFrame(preIndex, nextKey.index);
                    break;
                case KeyType.DecelerateKey:
                    DecelerateFillFrame(preIndex, nextKey.index);
                    break;
                default:
                    Debug.LogError("delete nextkey type not right");
                    break;

            }

        }
        keys.RemoveAt(k);
    }*/
    #endregion
    public void DeleteXKeyFrame(int _keyIndex)
    {
        //Debug.Log("DeleteXkey  index="+ _keyIndex);

        bool isCotains = false;

        if (GeneralKeysIndex.Contains(_keyIndex) && Frames[_keyIndex].XKey!= KeyType.NotKey)
        {
            isCotains = true;
        }

        if (!isCotains)
        {
            Debug.Log("can not delete the key,wrong key index or xkey not include  _keyIndex=" + _keyIndex);
            return;
        }

        if (GeneralKeysIndex.Count == 1)
        {
            if(Frames[_keyIndex].YKey== KeyType.NotKey&& Frames[_keyIndex].AKey== KeyType.NotKey)
            {
                GeneralKeysIndex = new List<int>();
                Frames = new List<EditorAminaComponentClipData>();
                return;
            }

            for (int i = 0; i < Frames.Count; i++)
            {
                Frames[i].x = defaultAminaFrame.pos.x;
            }
            Frames[_keyIndex].XKey = KeyType.NotKey;
            Frames[_keyIndex].XfillPara = null;
            return;
        }

        int k = GeneralKeysIndex.IndexOf(_keyIndex);

        int _preXIndex = -1;
        if (k != 0)
        {
            for (int i = k - 1; i >= 0; i--)
            {
                if (Frames[GeneralKeysIndex[i]].XKey != KeyType.NotKey)
                {
                    _preXIndex = GeneralKeysIndex[i];
                    break;
                }
            }
        }
        
        int _nextXIndex = -1;
        if (k != GeneralKeysIndex.Count - 1)
        {
            for (int i = k + 1; i < GeneralKeysIndex.Count; i++)
            {
                if (Frames[GeneralKeysIndex[i]].XKey != KeyType.NotKey)
                {
                    _nextXIndex = GeneralKeysIndex[i];
                    break;
                }
            }
        }
        else
        {
            if(Frames[_keyIndex].YKey== KeyType.NotKey&& Frames[_keyIndex].AKey == KeyType.NotKey)
            {
                //删除多余的帧  
                int _limit = GeneralKeysIndex[k - 1];

                for(int i = _keyIndex; i > _limit; i--)
                {
                    Frames.RemoveAt(i);
                }

                float _newX;
                if (_preXIndex == -1)
                {
                    _newX = defaultAminaFrame.pos.x;
                }
                else
                {
                    _newX = Frames[_preXIndex].x;
                }
                for(int i = _preXIndex+1; i < Frames.Count; i++)
                {
                    Frames[i].x = _newX;
                }
                GeneralKeysIndex.RemoveAt(k);
                return;
            }
        }

        Frames[_keyIndex].XKey = KeyType.NotKey;
        Frames[_keyIndex].XfillPara = null;

        if (_preXIndex == -1 && _nextXIndex == -1)
        {
            for(int i = 0; i < Frames.Count; i++)
            {
                Frames[i].x = defaultAminaFrame.pos.x;
            }
        }else if(_preXIndex == -1)
        {
            float _endX = Frames[_nextXIndex].x;
            for (int i = 0; i < _nextXIndex; i++)
            {
                Frames[i].x = _endX;
            }
        }else if(_nextXIndex == -1)
        {
            float _preX = Frames[_preXIndex].x;
            for (int i = _preXIndex + 1; i < Frames.Count; i++)
            {
                Frames[i].x = _preX;
            }
        }
        else
        {
            FillFrame(_preXIndex, _nextXIndex, Frames[_nextXIndex].XKey, FrameParaType.x, Frames[_nextXIndex].XfillPara);
        }
        
        if(!Frames[_keyIndex].isKey)
        {
            GeneralKeysIndex.RemoveAt(k);
        }
    }

    public void DeleteYKeyFrame(int _keyIndex)
    {
        bool isCotains = false;

        if (GeneralKeysIndex.Contains(_keyIndex) && Frames[_keyIndex].YKey != KeyType.NotKey)
        {
            isCotains = true;
        }

        if (!isCotains)
        {
            Debug.Log("can not delete the key,wrong key index or ykey not include");
            return;
        }

        if (GeneralKeysIndex.Count == 1)
        {
            if (Frames[_keyIndex].XKey == KeyType.NotKey && Frames[_keyIndex].AKey == KeyType.NotKey)
            {
                GeneralKeysIndex = new List<int>();
                Frames = new List<EditorAminaComponentClipData>();
                return;
            }

            for (int i = 0; i < Frames.Count; i++)
            {
                Frames[i].y = defaultAminaFrame.pos.y;
            }
            Frames[_keyIndex].YKey = KeyType.NotKey;
            Frames[_keyIndex].YfillPara = null;
            return;
        }

        int k = GeneralKeysIndex.IndexOf(_keyIndex);

        int _preYIndex = -1;
        if (k != 0)
        {
            for (int i = k - 1; i >= 0; i--)
            {
                if (Frames[GeneralKeysIndex[i]].YKey != KeyType.NotKey)
                {
                    _preYIndex = GeneralKeysIndex[i];
                    break;
                }
            }
        }

        int _nextYIndex = -1;
        if (k != GeneralKeysIndex.Count - 1)
        {
            for (int i = k + 1; i < GeneralKeysIndex.Count; i++)
            {
                if (Frames[GeneralKeysIndex[i]].YKey != KeyType.NotKey)
                {
                    _nextYIndex = GeneralKeysIndex[i];
                    break;
                }
            }
        }
        else
        {
            if (Frames[_keyIndex].XKey == KeyType.NotKey && Frames[_keyIndex].AKey == KeyType.NotKey)
            {
                //删除多余的帧  
                int _limit = GeneralKeysIndex[k - 1];

                for (int i = _keyIndex; i > _limit; i--)
                {
                    Frames.RemoveAt(i);
                }

                float _newY;
                if (_preYIndex == -1)
                {
                    _newY = defaultAminaFrame.pos.y;
                }
                else
                {
                    _newY = Frames[_preYIndex].y;
                }
                for (int i = _preYIndex + 1; i < Frames.Count; i++)
                {
                    Frames[i].y = _newY;
                }
                GeneralKeysIndex.RemoveAt(k);
                return;
            }
        }

        Frames[_keyIndex].YKey = KeyType.NotKey;
        Frames[_keyIndex].YfillPara = null;

        if (_preYIndex == -1 && _nextYIndex == -1)
        {
            for (int i = 0; i < Frames.Count; i++)
            {
                Frames[i].y = defaultAminaFrame.pos.y;
            }
        }
        else if (_preYIndex == -1)
        {
            float _endY = Frames[_nextYIndex].y;
            for (int i = 0; i < _nextYIndex; i++)
            {
                Frames[i].y = _endY;
            }
        }
        else if (_nextYIndex == -1)
        {
            float _preY = Frames[_preYIndex].y;
            for (int i = _preYIndex + 1; i < Frames.Count; i++)
            {
                Frames[i].y = _preY;
            }
        }
        else
        {
            FillFrame(_preYIndex, _nextYIndex, Frames[_nextYIndex].YKey, FrameParaType.y, Frames[_nextYIndex].YfillPara);
        }

        if (!Frames[_keyIndex].isKey)
        {
            GeneralKeysIndex.RemoveAt(k);
        }
    }

    public void DeleteAKeyFrame(int _keyIndex)
    {
        bool isCotains = false;

        if (GeneralKeysIndex.Contains(_keyIndex) && Frames[_keyIndex].AKey != KeyType.NotKey)
        {
            isCotains = true;
        }

        if (!isCotains)
        {
            Debug.Log("can not delete the key,wrong key index or xkey not include");
            return;
        }

        if (GeneralKeysIndex.Count == 1)
        {
            if (Frames[_keyIndex].YKey == KeyType.NotKey && Frames[_keyIndex].XKey == KeyType.NotKey)
            {
                GeneralKeysIndex = new List<int>();
                Frames = new List<EditorAminaComponentClipData>();
                return;
            }

            for (int i = 0; i < Frames.Count; i++)
            {
                Frames[i].angle = defaultAminaFrame.angle;
            }
            Frames[_keyIndex].AKey = KeyType.NotKey;
            Frames[_keyIndex].AfillPara = null;
            return;
        }

        int k = GeneralKeysIndex.IndexOf(_keyIndex);

        int _preAIndex = -1;
        if (k != 0)
        {
            for (int i = k - 1; i >= 0; i--)
            {
                if (Frames[GeneralKeysIndex[i]].AKey != KeyType.NotKey)
                {
                    _preAIndex = GeneralKeysIndex[i];
                    break;
                }
            }
        }

        int _nextAIndex = -1;
        if (k != GeneralKeysIndex.Count - 1)
        {
            for (int i = k + 1; i < GeneralKeysIndex.Count; i++)
            {
                if (Frames[GeneralKeysIndex[i]].AKey != KeyType.NotKey)
                {
                    _nextAIndex = GeneralKeysIndex[i];
                    break;
                }
            }
        }
        else
        {
            if (Frames[_keyIndex].YKey == KeyType.NotKey && Frames[_keyIndex].XKey == KeyType.NotKey)
            {
                //删除多余的帧  
                int _limit = GeneralKeysIndex[k - 1];

                for (int i = _keyIndex; i > _limit; i--)
                {
                    Frames.RemoveAt(i);
                }

                float _newA;
                if (_preAIndex == -1)
                {
                    _newA = defaultAminaFrame.angle;
                }
                else
                {
                    _newA = Frames[_preAIndex].angle;
                }
                for (int i = _preAIndex + 1; i < Frames.Count; i++)
                {
                    Frames[i].angle = _newA;
                }
                GeneralKeysIndex.RemoveAt(k);
                return;
            }
        }

        Frames[_keyIndex].AKey = KeyType.NotKey;
        Frames[_keyIndex].AfillPara = null;

        if (_preAIndex == -1 && _nextAIndex == -1)
        {
            for (int i = 0; i < Frames.Count; i++)
            {
                Frames[i].angle = defaultAminaFrame.angle;
            }
        }
        else if (_preAIndex == -1)
        {
            float _endA = Frames[_nextAIndex].angle;
            for (int i = 0; i < _nextAIndex; i++)
            {
                Frames[i].angle = _endA;
            }
        }
        else if (_nextAIndex == -1)
        {
            float _preA = Frames[_preAIndex].angle;
            for (int i = _preAIndex + 1; i < Frames.Count; i++)
            {
                Frames[i].angle = _preA;
            }
        }
        else
        {
            FillFrame(_preAIndex, _nextAIndex, Frames[_nextAIndex].AKey, FrameParaType.a, Frames[_nextAIndex].AfillPara);
        }

        if (!Frames[_keyIndex].isKey)
        {
            GeneralKeysIndex.RemoveAt(k);
        }
    }
    #region
    /*
    public void MoveKey(int _fromIndex, int _deltaIndex)
    {
        bool isContains = false;
        if (GeneralKeysIndex.Contains(_fromIndex)&&Frames[_fromIndex].isKey)
        {
            isContains = true;
        }

        if (!isContains)
        {
            Debug.Log("can not Move X ,wrong fromIndex ");
            return;
        }


        
        int _targetIndex = _fromIndex + _deltaIndex;

        if (_targetIndex < 0)
        {
            Debug.LogWarning("can not MoveKey the key,targetIndex <0 ");
            return;
        }

        if (_deltaIndex == 0)
        {
            return;
        }

        KeyType _typeX = Frames[_fromIndex].XKey;
        KeyType _typeY = Frames[_fromIndex].YKey;
        KeyType _typeA = Frames[_fromIndex].AKey;

        int keyIndex = GeneralKeysIndex.IndexOf(_fromIndex);
        GeneralKeysIndex.RemoveAt(keyIndex);
        if (GeneralKeysIndex.Count == 1)
        {
            _targetIndex = _fromIndex + _deltaIndex;
            if (_deltaIndex < 0)
            {
                for (int i = _fromIndex; i > _targetIndex; i--)
                {
                    Frames.RemoveAt(i);
                }
            }
            else
            {
                for (int i = _fromIndex + 1; i <= _targetIndex; i++)
                {
                    EditorAminaComponentClipData _af = new EditorAminaComponentClipData(i);
                    _af.x = Frames[i - 1].x;
                    _af.y = Frames[i - 1].y;
                    _af.angle = Frames[i - 1].angle;
                    Frames.Add(_af);
                }
            }
            Frames[_targetIndex].XKey = _typeX;
            Frames[_targetIndex].YKey = _typeY;
            Frames[_targetIndex].AKey = _typeA;
            GeneralKeysIndex.Add(_targetIndex);
        }
        else
        {

            float _x = Frames[_fromIndex].x;
            float _y = Frames[_fromIndex].y;
            float _a = Frames[_fromIndex].angle;

            DeleteKeyFrame(_fromIndex);
            AddXKeyFrame(_x, _targetIndex, _typeX);
            AddYKeyFrame(_y, _targetIndex, _typeY);
            AddAKeyFrame(_a, _targetIndex, _typeA);
        }
    }*/

    /*public void MoveKey(int _fromIndex,int _deltaIndex)
    {

        bool isContains = false;

        foreach (Key _key in keys)
        {
            if (_key.index == _fromIndex)
            {
                isContains = true;
                break;
            }
        }

        if (!isContains)
        {
            Debug.Log("can not MoveKey the key,wrong fromIndex ");
            return;
        }


        int _targetIndex = _fromIndex + _deltaIndex;

        if (_targetIndex < 0)
        {
            Debug.Log("can not MoveKey the key,targetIndex <0 ");
            return;
        }

        if (_deltaIndex == 0)
        {
            return;
        }

        int keyIndex = GetIndexOf(_fromIndex);

        Key _moveKey = keys[keyIndex];
        if (keys.Count == 1)
        {
            _moveKey.index = _fromIndex + _deltaIndex;
            if (_deltaIndex < 0)
            {
                for(int i = _fromIndex; i > _targetIndex; i--)
                {
                    frames.RemoveAt(i);
                }
            }
            else
            {
                for (int i = _fromIndex+1; i <= _targetIndex; i++)
                {
                    AminaFrame _af = new AminaFrame();
                    _af.pos = new Vector2(frames[i - 1].pos.x, frames[i - 1].pos.y);
                    _af.angle = frames[i - 1].angle;
                    frames.Add(_af);
                }
            }
        }
        else
        {
            Key _newKey = new Key(_fromIndex + _deltaIndex, _moveKey.type);
            Vector2 _pos = new Vector2(frames[_fromIndex].pos.x, frames[_fromIndex].pos.y);
            float _angle = frames[_fromIndex].angle;

            DeleteKeyFrame(_fromIndex);
            AddKeyFrame(_pos, _angle, _newKey);
        }
    }*/
    #endregion
    public void MoveXYAKey(int _fromIndex, int _deltaIndex,FrameParaType _paraType)
    {
        //Debug.Log("MoveXYAKey " + _fromIndex + " " + _deltaIndex + " " + _paraType);


        bool isContains = false;
        KeyType _type= KeyType.NotKey;
        IFrameFillPara _fillPara = null;
        if (GeneralKeysIndex.Contains(_fromIndex))
        {
            _type = Frames[_fromIndex].GetKeyType(_paraType);
            _fillPara = Frames[_fromIndex].GetKeyFillPara(_paraType);
            if (_type != KeyType.NotKey)
            {
                isContains = true;
            }
        }

        if (!isContains)
        {
            Debug.Log("can not Move X ,wrong fromIndex "+_fromIndex);
            return;
        }


        int _targetIndex = _fromIndex + _deltaIndex;

        if (_targetIndex < 0)
        {
            Debug.Log("can not MoveKey the key,targetIndex <0 ");
            return;
        }

        if (_deltaIndex == 0)
        {
            return;
        }

        int keyIndex = GeneralKeysIndex.IndexOf(_fromIndex);
        //GeneralKeysIndex.RemoveAt(keyIndex);
        if (GeneralKeysIndex.Count == 0)
        {
            _targetIndex = _fromIndex + _deltaIndex;
            if (_deltaIndex < 0)
            {
                for (int i = _fromIndex; i > _targetIndex; i--)
                {
                    Frames.RemoveAt(i);
                }

                Frames[_targetIndex].SetKeyType(_type,_paraType);
            }
            else
            {
                for (int i = _fromIndex + 1; i <= _targetIndex; i++)
                {
                    EditorAminaComponentClipData _af = new EditorAminaComponentClipData(i);
                    _af.x = Frames[i - 1].x;
                    _af.y = Frames[i - 1].y;
                    _af.angle = Frames[i - 1].angle;
                    
                    Frames.Add(_af);
                }
                Frames[_targetIndex].SetKeyType(_type,_paraType);
            }

            GeneralKeysIndex.Add(_targetIndex);
        }
        else
        {
            float _p;

            switch (_paraType)
            {
                case FrameParaType.x:
                    _p = Frames[_fromIndex].x;
                    DeleteXKeyFrame(_fromIndex);
                    AddXKeyFrame(_p, _targetIndex, _type, _fillPara);
                    break;
                case FrameParaType.y:
                    _p = Frames[_fromIndex].y;
                    DeleteYKeyFrame(_fromIndex);
                    AddYKeyFrame(_p, _targetIndex, _type, _fillPara);
                    break;
                case FrameParaType.a:
                    _p = Frames[_fromIndex].angle;
                    DeleteAKeyFrame(_fromIndex);
                    AddAKeyFrame(_p, _targetIndex, _type, _fillPara);
                    break;

            }
        }
    }
    #region
    /*
    void InsertFrame(int _InsertKeyIndex)
    {
        int _preIndex = keys[_InsertKeyIndex-1].index;

        Key _currentKey = keys[_InsertKeyIndex];
        switch (_currentKey.type)
        {
            case KeyType.AccelerateKey:
                AccelerateFillFrame(_preIndex, _currentKey.index);
                break;
            case KeyType.LinearKey:
                LinearFillFrame(_preIndex, _currentKey.index);
                break;
            case KeyType.TrigonoKey:
                TrangonoFillFrame(_preIndex, _currentKey.index);
                break;
            case KeyType.DecelerateKey:
                DecelerateFillFrame(_preIndex, _currentKey.index);
                break;
            default:
                Debug.LogError("currentKey.type  not right");
                break;
        }

        Key _nextKey = keys[_InsertKeyIndex + 1];
        switch (_nextKey.type)
        {
            case KeyType.AccelerateKey:
                AccelerateFillFrame(_currentKey.index, _nextKey.index);
                break;
            case KeyType.LinearKey:
                LinearFillFrame(_currentKey.index, _nextKey.index);
                break;
            case KeyType.TrigonoKey:
                TrangonoFillFrame(_currentKey.index, _nextKey.index);
                break;
            case KeyType.DecelerateKey:
                DecelerateFillFrame(_currentKey.index, _nextKey.index);
                break;
            default:
                Debug.LogError("nextKey.type  not right");
                break;
        }
    }

    void InsertFirstKeyFrame()
    {
        int _index = keys[0].index;
        Key _nextKey = keys[1];
        switch (_nextKey.type)
        {
            case KeyType.AccelerateKey:
                AccelerateFillFrame(_index, _nextKey.index);
                break;
            case KeyType.LinearKey:
                LinearFillFrame(_index, _nextKey.index);
                break;
            case KeyType.TrigonoKey:
                TrangonoFillFrame(_index, _nextKey.index);
                break;
            case KeyType.DecelerateKey:
                DecelerateFillFrame(_index, _nextKey.index);
                break;
            default:
                Debug.LogError("nextKey.type  not right");
                break;
        }

    }

    void LinearFillFrame(int _start,int _end)
    {
        AminaFrame _endKey = frames[_end];
        AminaFrame _startKey = frames[_start];

        float _deltaAngle = (_endKey.angle - _startKey.angle) / (_end-_start);

        for (int i = _start + 1; i < _end; i++)
        {
            AminaFrame _af = new AminaFrame();
            _af.pos = Vector2.Lerp(frames[i - 1].pos, _endKey.pos, 1f / (_end - i + 1));
            _af.angle = frames[i - 1].angle + _deltaAngle;
            frames[i] = _af;
        }
    }

    void AccelerateFillFrame(int _start,int _end)
    {
        AminaFrame _startKey = frames[_start];
        AminaFrame _endKey = frames[_end];

        int _preDeltaFrames = _end - _start;

        float _startX = _startKey.pos.x;
        float _startY = _startKey.pos.y;
        float _startA = _startKey.angle;
        float _startVX = 0;
        float _startVY = 0;
        float _startVA = 0;
        if (_start > 0)
        {
            _startVX = _startKey.pos.x - frames[_start - 1].pos.x;
            _startVY = _startKey.pos.y - frames[_start - 1].pos.y;
            _startVA = _startKey.angle - frames[_start - 1].angle;
        }
        float aX = GetUniformAcceleration(_startX, _endKey.pos.x, _startVX, _preDeltaFrames);
        float aY = GetUniformAcceleration(_startY, _endKey.pos.y, _startVY, _preDeltaFrames);
        float _deltaAng = GetUniformAcceleration(_startA, _endKey.angle, _startVA, _preDeltaFrames);

        for (int i = 1; i < _preDeltaFrames; i++)
        {
            AminaFrame _af = new AminaFrame();
            float _realV = i - 0.5f;
            _af.pos = new Vector2(frames[_start + i - 1].pos.x + _startVX + _realV * aX, frames[_start + i - 1].pos.y + _startVY + _realV * aY);
            _af.angle = frames[_start + i - 1].angle + _startVA + _realV * _deltaAng;
            //_af.angle = Vector2.Lerp(frames[i - 1].angle, _end.angle, 1f / (nextIndex - i + 1));

            frames[_start + i] = _af;
        }
    }

    void TrangonoFillFrame(int _start, int _end)
    {
        AminaFrame _startKey = frames[_start];
        AminaFrame _endKey = frames[_end];

        int _deltaFrames = _end - _start;

        float _deltaRadias = Mathf.PI / _deltaFrames;

        float _startX = _startKey.pos.x;
        float _startY = _startKey.pos.y;
        float _startA = _startKey.angle;
        float _disX = _endKey.pos.x - _startX;
        float _disY = _endKey.pos.y - _startY;
        float _disA = _endKey.angle - _startA;

        for (int i = 1; i < _deltaFrames; i++)
        {
            AminaFrame _af = new AminaFrame();
            float _d =  1 - Mathf.Cos(_deltaRadias * i);
            float _x = _startX + _d  / 2 * _disX;
            float _y = _startY + _d  / 2 * _disY;
            float _a = _startA + _d  / 2 * _disA;
            _af.pos = new Vector2(_x, _y);
            _af.angle = _a;
            //_af.angle = Vector2.Lerp(frames[i - 1].angle, _end.angle, 1f / (nextIndex - i + 1));

            frames[_start + i] = _af;
        }
    }

    void DecelerateFillFrame(int _start, int _end)
    {
        AminaFrame _startKey = frames[_start];
        AminaFrame _endKey = frames[_end];

        int _deltaFrames = _end - _start;

        float _deltaRadias = Mathf.PI / _deltaFrames;

        float _startX = _startKey.pos.x;
        float _startY = _startKey.pos.y;
        float _startA = _startKey.angle;
        float _disX = _endKey.pos.x - _startX;
        float _disY = _endKey.pos.y - _startY;
        float _disA = _endKey.angle - _startA;

        float _kX = GetDecelerationCoefficient(_deltaFrames, _disX);
        float _kY = GetDecelerationCoefficient(_deltaFrames, _disY);
        float _kA = GetDecelerationCoefficient(_deltaFrames, _disA);


        for (int i = 1; i < _deltaFrames; i++)
        {
            AminaFrame _af = new AminaFrame();
            float _x = _startX - (i - 2 * _deltaFrames) * i * _kX;
            float _y = _startY - (i - 2 * _deltaFrames) * i * _kY;
            float _a = _startA - (i - 2 * _deltaFrames) * i * _kA;
            _af.pos = new Vector2(_x, _y);
            _af.angle = _a;
            //_af.angle = Vector2.Lerp(frames[i - 1].angle, _end.angle, 1f / (nextIndex - i + 1));

            frames[_start + i] = _af;
        }
    }*/
    #endregion
    void FillFrame(int _start, int _end, KeyType _keyType, FrameParaType _paraType,IFrameFillPara _fillPara)
    {
        switch (_keyType)
        {
            case KeyType.LinearKey:
                LinearFillFrame(_start, _end, _paraType);
                break;
            case KeyType.AccelerateKey:
                AccelerateFillFrame(_start, _end, _paraType);
                break;
            case KeyType.DecelerateKey:
                DecelerateFillFrame(_start, _end, _paraType);
                break;
            case KeyType.TrigonoKey:
                TrangonoFillFrame(_start, _end, _paraType);
                break;
            case KeyType.SineKey:
                SineFillFrame(_start, _end, _paraType, _fillPara);
                break;
            case KeyType.CosineKey:
                CosineFillFrame(_start, _end, _paraType, _fillPara);
                break;
            default:
                Debug.LogWarning("FillFrame failed,not expect key type=" + _keyType);
                break;
        }
    }

    void LinearFillFrame(int _start, int _end,FrameParaType _type)
    {
        float _startPara;
        float _endPara;
        float _delta;
        switch (_type)
        {
            case FrameParaType.x:
                _startPara = Frames[_start].x;
                _endPara = Frames[_end].x;
                _delta = (_endPara - _startPara) / (_end - _start);
                for (int i = _start + 1; i < _end; i++)
                {
                    Frames[i].x = Frames[i - 1].x + _delta;
                }

                break;
            case FrameParaType.y:
                _startPara = Frames[_start].y;
                _endPara = Frames[_end].y;
                _delta = (_endPara - _startPara) / (_end - _start);
                for (int i = _start + 1; i < _end; i++)
                {
                    Frames[i].y = Frames[i - 1].y + _delta;
                }
                break;
            case FrameParaType.a:
                _startPara = Frames[_start].angle;
                _endPara = Frames[_end].angle;
                _delta = (_endPara - _startPara) / (_end - _start);
                for (int i = _start + 1; i < _end; i++)
                {
                    Frames[i].angle = Frames[i - 1].angle + _delta;
                }
                break;
        }
    }

    void AccelerateFillFrame(int _start, int _end, FrameParaType _type)
    {
        int _preDeltaFrames = _end - _start;

        float _startP;
        float _startVP = 0;
        float _aP;
        switch (_type)
        {
            case FrameParaType.x:
                _startP = Frames[_start].x;
                if (_start > 0)
                {
                    _startVP = Frames[_start].x - Frames[_start - 1].x;
                }
                _aP = GetUniformAcceleration(_startP, Frames[_end].x, _startVP, _preDeltaFrames);
                for (int i = 1; i < _preDeltaFrames; i++)
                {
                    float _realV = i - 0.5f;
                    Frames[_start + i].x = Frames[_start + i - 1].x + _startVP + _realV * _aP;
                }
                break;
            case FrameParaType.y:
                _startP = Frames[_start].y;
                if (_start > 0)
                {
                    _startVP = Frames[_start].y - Frames[_start - 1].y;
                }
                _aP = GetUniformAcceleration(_startP, Frames[_end].y, _startVP, _preDeltaFrames);
                for (int i = 1; i < _preDeltaFrames; i++)
                {
                    float _realV = i - 0.5f;
                    Frames[_start + i].y = Frames[_start + i - 1].y + _startVP + _realV * _aP;
                }
                break;
            case FrameParaType.a:
                _startP = Frames[_start].angle;
                if (_start > 0)
                {
                    _startVP = Frames[_start].angle - Frames[_start - 1].angle;
                }
                _aP = GetUniformAcceleration(_startP, Frames[_end].angle, _startVP, _preDeltaFrames);
                for (int i = 1; i < _preDeltaFrames; i++)
                {
                    float _realV = i - 0.5f;
                    Frames[_start + i].angle = Frames[_start + i - 1].angle + _startVP + _realV * _aP;
                }
                break;
        }

        
    }

    void TrangonoFillFrame(int _start, int _end, FrameParaType _type)
    {
        //Debug.Log("TrangonoFillFrame");
        int _deltaFrames = _end - _start;

        float _deltaRadias = Mathf.PI / _deltaFrames;
        float _startP; float _disP;
        switch (_type)
        {
            case FrameParaType.x:
                _startP = Frames[_start].x;
                _disP = Frames[_end].x - _startP;

                for (int i = 1; i < _deltaFrames; i++)
                {
                    float _d = 1 - Mathf.Cos(_deltaRadias * i);
                    float _x = _startP + _d / 2 * _disP;
                    Frames[_start + i].x = _x;
                }
                break;
            case FrameParaType.y:
                _startP = Frames[_start].y;
                _disP = Frames[_end].y - _startP;

                for (int i = 1; i < _deltaFrames; i++)
                {
                    float _d = 1 - Mathf.Cos(_deltaRadias * i);
                    float _y = _startP + _d / 2 * _disP;
                    Frames[_start + i].y = _y;
                }

                break;
            case FrameParaType.a:
                _startP = Frames[_start].angle;
                _disP = Frames[_end].angle - _startP;

                for (int i = 1; i < _deltaFrames; i++)
                {
                    float _d = 1 - Mathf.Cos(_deltaRadias * i);
                    float _a = _startP + _d / 2 * _disP;
                    Frames[_start + i].angle = _a;
                }
                break;
        }
    }

    void DecelerateFillFrame(int _start, int _end, FrameParaType _type)
    {

        int _deltaFrames = _end - _start;

        float _deltaRadias = Mathf.PI / _deltaFrames;
        float _startP;
        float _disP;
        float _k;
        switch (_type)
        {
            case FrameParaType.x:
                _startP = Frames[_start].x;
                _disP = Frames[_end].x - _startP;
                _k = GetDecelerationCoefficient(_deltaFrames, _disP);
                for (int i = 1; i < _deltaFrames; i++)
                {
                    Frames[_start + i].x = _startP - (i - 2 * _deltaFrames) * i * _k;
                }
                break;
            case FrameParaType.y:
                _startP = Frames[_start].y;
                _disP = Frames[_end].y - _startP;
                _k = GetDecelerationCoefficient(_deltaFrames, _disP);
                for (int i = 1; i < _deltaFrames; i++)
                {
                    Frames[_start + i].y = _startP - (i - 2 * _deltaFrames) * i * _k;
                }
                break;
            case FrameParaType.a:
                _startP = Frames[_start].angle;
                _disP = Frames[_end].angle - _startP;
                _k = GetDecelerationCoefficient(_deltaFrames, _disP);
                for (int i = 1; i < _deltaFrames; i++)
                {
                    Frames[_start + i].angle = _startP - (i - 2 * _deltaFrames) * i * _k;
                }
                break;
        }
    }

    void SineFillFrame(int _start, int _end, FrameParaType _type,IFrameFillPara _frameFillPara)
    {
        SineKeyFillPara _sineFillPara = _frameFillPara as SineKeyFillPara;
        if (_sineFillPara == null)
        {
            Debug.LogWarning("SineKeyFillPara is null");
            return;
        }

        int _deltaFrames = _end - _start;

        float _deltaRadias = Mathf.PI*(_sineFillPara.EndAngle - _sineFillPara.StartAngle  ) / _deltaFrames;
        float _startAng = Mathf.PI * _sineFillPara.StartAngle;
        float _sinEnd = 2;
        if (_sineFillPara.EndAngle > 0.5f)
        {
            _sinEnd = 2 - Mathf.Sin(Mathf.PI * _sineFillPara.EndAngle);
        }
        else
        {
            _sinEnd = Mathf.Sin(Mathf.PI * _sineFillPara.EndAngle);
        }

        float _sinSt = 0;
        if (_sineFillPara.StartAngle > 0.5f)
        {
            _sinSt = 2 - Mathf.Sin(Mathf.PI * _sineFillPara.StartAngle);
        }
        else
        {
            _sinSt = Mathf.Sin(Mathf.PI * _sineFillPara.StartAngle);
        }

        float _sinDis = _sinEnd - _sinSt;
        //float _delDistance;
        float _startP;
        float _disP;
        switch (_type)
        {
            case FrameParaType.x:
                _startP = Frames[_start].x;
                _disP = Frames[_end].x - _startP;
                //_delDistance = _disP * _sinDis / 2;
                //_delDistance = _disP * _sinDis / 2;
                for (int i = 1; i < _deltaFrames; i++)
                {
                    float _ang = _startAng + _deltaRadias * i;
                    float _sinAng = 0;
                    if (_ang > HALFPI) _sinAng = 2 - Mathf.Sin(_ang);
                    else _sinAng = Mathf.Sin(_ang);
                    float _k = (_sinAng - _sinSt) / _sinDis;
                    //float _d = Mathf.Sin(_deltaRadias * i);
                    float _x = _startP + _disP * _k;
                    Frames[_start + i].x = _x;
                }
                break;
            case FrameParaType.y:
                _startP = Frames[_start].y;
                _disP = Frames[_end].y - _startP;
                //_delDistance = _disP * _sinDis / 2;
                //_delDistance = _disP * _sinDis / 2;
                for (int i = 1; i < _deltaFrames; i++)
                {
                    float _ang = _startAng + _deltaRadias * i;
                    float _sinAng = 0;
                    if (_ang > HALFPI) _sinAng = 2 - Mathf.Sin(_ang);
                    else _sinAng = Mathf.Sin(_ang);
                    float _k = (_sinAng - _sinSt) / _sinDis;
                    //float _d = Mathf.Sin(_deltaRadias * i);
                    float _y = _startP + _disP * _k;
                    Frames[_start + i].y = _y;
                }
                break;
            case FrameParaType.a:
                _startP = Frames[_start].angle;
                _disP = Frames[_end].angle - _startP;
                //_delDistance = _disP * _sinDis / 2;
                //_delDistance = _disP * _sinDis / 2;
                for (int i = 1; i < _deltaFrames; i++)
                {
                    float _ang = _startAng + _deltaRadias * i;
                    float _sinAng = 0;
                    if (_ang > HALFPI) _sinAng = 2 - Mathf.Sin(_ang);
                    else _sinAng = Mathf.Sin(_ang);
                    float _k = (_sinAng - _sinSt) / _sinDis;
                    //float _d = Mathf.Sin(_deltaRadias * i);
                    float _a = _startP + _disP * _k;
                    Frames[_start + i].angle = _a;
                }
                break;
        }

    }

    void CosineFillFrame(int _start, int _end, FrameParaType _type, IFrameFillPara _frameFillPara)
    {
        Debug.Log("CosineFillFrame");
        SineKeyFillPara _sineFillPara = _frameFillPara as SineKeyFillPara;
        if (_sineFillPara == null)
        {
            Debug.LogWarning("SineKeyFillPara is null");
            return;
        }

        int _deltaFrames = _end - _start;

        float _deltaRadias = Mathf.PI * (_sineFillPara.EndAngle - _sineFillPara.StartAngle) / _deltaFrames;
        float _startAng = Mathf.PI * _sineFillPara.StartAngle;
        float _sinEnd = 1- Mathf.Cos(Mathf.PI * _sineFillPara.EndAngle);

        float _sinSt = 1 - Mathf.Cos(Mathf.PI * _sineFillPara.StartAngle);

        float _sinDis = _sinEnd - _sinSt;
        //float _delDistance;
        float _startP;
        float _disP;
        switch (_type)
        {
            case FrameParaType.x:
                _startP = Frames[_start].x;
                _disP = Frames[_end].x - _startP;
                //_delDistance = _disP * _sinDis / 2;
                //_delDistance = _disP * _sinDis / 2;
                for (int i = 1; i < _deltaFrames; i++)
                {
                    float _ang = _startAng + _deltaRadias * i;
                    float _sinAng = 1 - Mathf.Cos(_ang);
                    float _k = (_sinAng - _sinSt) / _sinDis;
                    //float _d = Mathf.Sin(_deltaRadias * i);
                    float _x = _startP + _disP * _k;
                    Frames[_start + i].x = _x;
                }
                break;
            case FrameParaType.y:
                _startP = Frames[_start].y;
                _disP = Frames[_end].y - _startP;
                //_delDistance = _disP * _sinDis / 2;
                //_delDistance = _disP * _sinDis / 2;
                for (int i = 1; i < _deltaFrames; i++)
                {
                    float _ang = _startAng + _deltaRadias * i;
                    float _sinAng = 0;
                    if (_ang > HALFPI) _sinAng = 2 - Mathf.Sin(_ang);
                    else _sinAng = Mathf.Sin(_ang);
                    float _k = (_sinAng - _sinSt) / _sinDis;
                    //float _d = Mathf.Sin(_deltaRadias * i);
                    float _y = _startP + _disP * _k;
                    Frames[_start + i].y = _y;
                }
                break;
            case FrameParaType.a:
                _startP = Frames[_start].angle;
                _disP = Frames[_end].angle - _startP;
                //_delDistance = _disP * _sinDis / 2;
                //_delDistance = _disP * _sinDis / 2;
                for (int i = 1; i < _deltaFrames; i++)
                {
                    float _ang = _startAng + _deltaRadias * i;
                    float _sinAng = 0;
                    if (_ang > HALFPI) _sinAng = 2 - Mathf.Sin(_ang);
                    else _sinAng = Mathf.Sin(_ang);
                    float _k = (_sinAng - _sinSt) / _sinDis;
                    //float _d = Mathf.Sin(_deltaRadias * i);
                    float _a = _startP + _disP * _k;
                    Frames[_start + i].angle = _a;
                }
                break;
        }

    }

    float GetUniformAcceleration(float _start,float _end,float _startSpeed, int _count)
    {
        float _acceleration = ((_end-_start) / _count - _startSpeed)*2 / _count;//加速度

        return _acceleration;
    }

    float GetDecelerationCoefficient(int _deltaIndex,float _deltaDis)
    {
        return _deltaDis/(_deltaIndex*_deltaIndex);
    }

    public void AddPauseTime(int[] _index)
    {
        if (_index == null) { PauseTime = null;return; }

        if (_index.Length != 2)
        {
            Debug.LogWarning("AddPauseTime should leghth==2  "+ _index);
            return;
        }

        int[] _pause;

        if (_index[0] <= _index[1]) _pause = new int[2] { _index[0], _index[1] };
        else { _pause = new int[2] { _index[1], _index[0] }; }

        if(_pause[0]<0|| _pause[1] >= FramesCount)
        {
            Debug.LogWarning("AddPauseTime out of range   _index=" + _index);
            return;
        }

        PauseTime = _pause;
    }

    public bool IsTheIndexHaveKey(int _index,FrameParaType _pType)
    {
        if (_index < Frames.Count)
        {
            switch (_pType)
            {
                case FrameParaType.x:
                    if (Frames[_index].XKey == KeyType.NotKey) return false;
                    else return true;
                case FrameParaType.y:
                    if (Frames[_index].YKey == KeyType.NotKey) return false;
                    else return true;
                case FrameParaType.a:
                    if (Frames[_index].AKey == KeyType.NotKey) return false;
                    else return true;
            }
        }

        return false;
    }

    public bool IsTheIndexHaveKey(int _index, int _keyType)
    {
        if (_index < Frames.Count)
        {
            switch (_keyType)
            {
                case 0:
                    if (!Frames[_index].isKey) return false;
                    else return true;
                case 1:
                    if (Frames[_index].XKey == KeyType.NotKey) return false;
                    else return true;
                case 2:
                    if (Frames[_index].YKey == KeyType.NotKey) return false;
                    else return true;
                case 3:
                    if (Frames[_index].AKey == KeyType.NotKey) return false;
                    else return true;
            }
        }

        return false;
    }

    public KeyType GetIndexType(int _index,FrameParaType _type)
    {
        if (_index < Frames.Count)
        {
            switch (_type)
            {
                case FrameParaType.x:
                    return Frames[_index].XKey;
                case FrameParaType.y:
                    return Frames[_index].YKey;
                case FrameParaType.a:
                    return Frames[_index].AKey;
            }
        }

        Debug.LogWarning("GetIndexType out of Range  _index =" + _index);
        return KeyType.NotKey;

    }

    public void AddOutput(int _index)
    {
        for(int i = 0; i < OutData.Count; i++)
        {
            if (OutData[i].Index == _index)
            {
                Debug.LogWarning("can not add output,index=" + _index);
                return;
            }
        }

        OutData.Add(new FrameOutputData(_index));
    }

    public void DeleteOutput(int _index)
    {
        for (int i = 0; i < OutData.Count; i++)
        {
            if (OutData[i].Index == _index)
            {
                OutData.RemoveAt(i);
                return;
            }
        }

        Debug.LogWarning("can not delete output,index=" + _index);
    }

    public void MoveOutput(int _index,int _newIndex)
    {
        FrameOutputData _target=null;
        
        for (int i = 0; i < OutData.Count; i++)
        {
            if (OutData[i].Index == _newIndex)
            {
                return;
            }

            if (OutData[i].Index == _index)
            {
                _target= OutData[i];
            }
        }

        if (_target != null)
        {
            _target.SetIndex(_newIndex);
        }
    }


    [Serializable]
    public class EditorAminaComponentClipData
    {
        public int Index;
        public float x;
        public float y;
        public float angle;

        public bool isKey { get{ if (XKey != KeyType.NotKey) return true; if (YKey != KeyType.NotKey) return true; if (AKey != KeyType.NotKey) return true;return false; } }
        public KeyType XKey;
        public IFrameFillPara XfillPara;
        public KeyType YKey;
        public IFrameFillPara YfillPara;
        public KeyType AKey;
        public IFrameFillPara AfillPara;

        public EditorAminaComponentClipData(int _index)
        {
            Index = _index;
            XKey = YKey = AKey = KeyType.NotKey;
            XfillPara = YfillPara = AfillPara = null;
        }

        public EditorAminaComponentClipData(EditorAminaComponentClipData _other)
        {
            Index = _other.Index;
            x = _other.x;
            y = _other.y;
            angle = _other.angle;
            XKey = _other.XKey;
            YKey = _other.YKey;
            AKey = _other.AKey;
            if (_other.XfillPara != null) XfillPara = _other.XfillPara.Copy();
            else XfillPara = null;
            if (_other.YfillPara != null) YfillPara = _other.YfillPara.Copy();
            else YfillPara = null;
            if (_other.AfillPara != null) AfillPara = _other.AfillPara.Copy();
            else AfillPara = null;
        }

        public void SetKeyType(KeyType _type,FrameParaType _pType)
        {
            switch (_pType)
            {
                case FrameParaType.x:
                    XKey = _type;
                    break;
                case FrameParaType.y:
                    YKey = _type;
                    break;
                case FrameParaType.a:
                    AKey = _type;
                    break;
            }
        }

        public KeyType GetKeyType(FrameParaType _pType)
        {
            switch (_pType)
            {
                case FrameParaType.x:
                    return XKey;
                case FrameParaType.y:
                    return YKey;
                case FrameParaType.a:
                    return AKey;
                default:
                    Debug.LogWarning("Get key type wrong:Unexpected FrameParaType:"+_pType);
                    return KeyType.NotKey;
            }
        }

        public IFrameFillPara GetKeyFillPara(FrameParaType _pType)
        {
            switch (_pType)
            {
                case FrameParaType.x:
                    return XfillPara;
                case FrameParaType.y:
                    return YfillPara;
                case FrameParaType.a:
                    return AfillPara;
                default:
                    Debug.LogWarning("Get key fill para wrong:Unexpected FrameParaType:" + _pType);
                    return null;
            }
        }

    }

}

[Serializable]
public class FrameOutputData
{
    [SerializeField]
    public int Index { get { return index; } }
    private int index;
    public List<OutType> Out;

    public FrameOutputData() { Out = new List<OutType>(); }
    public FrameOutputData(FrameOutputData other)
    {
        index = other.Index;
        Out = new List<OutType>(other.Out);
    }

    public FrameOutputData(int _index)
    {
        index = _index;
        Out = new List<OutType>();
    }

    public void SetIndex(int _index)
    {
        index = _index;
    }

}

[Serializable]
public enum OutType
{
    onNormal = 0,
    onFast = 1,
    onUp =2,
    onUpFast=3,    
    onDown=4,
    onDownFast=5,
    attack=6,

}


[Serializable]
public enum KeyType
{
    NotKey=0,
    LinearKey=1,
    AccelerateKey=2,
    TrigonoKey=3,
    DecelerateKey=4,
    SineKey=5,
    CosineKey=6,
}

[Serializable]
public enum FrameParaType
{
    x,
    y,
    a,
}

[SerializeField]
public interface IFrameFillPara
{
    IFrameFillPara Copy();
}

[Serializable]
public class SineKeyFillPara : IFrameFillPara
{
    //public float DeltaDistance;
    public float StartAngle;
    public float EndAngle;

    public SineKeyFillPara(float _startAng,float _endAng)
    {
        //DeltaDistance = _delDis;
        StartAngle = _startAng;
        EndAngle = _endAng;
        StartAngle = Mathf.Max(0, StartAngle);
        StartAngle = Mathf.Min(1, StartAngle);
        EndAngle = Mathf.Max(0, EndAngle);
        EndAngle = Mathf.Min(1, EndAngle);

        if (StartAngle > EndAngle)
        {
            Debug.LogWarning("StartAngle > EndAngle");
            StartAngle = EndAngle;
        }

    }

    public IFrameFillPara Copy()
    {
        SineKeyFillPara _new = new SineKeyFillPara(StartAngle, EndAngle);
        return _new;
    }

}


public class EditorAminaActionClip
{
    public int ProtypeID;

    public EditorAminaComponentClip[] componentClips;

}

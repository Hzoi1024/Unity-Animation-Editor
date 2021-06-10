using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class AminaEditorUndo
{
    AminaEditor aminaEditor;
    List<AminaEditorUndoOperation> aminaEditorUndoOperations;
    int pointer;

    public AminaEditorUndo(AminaEditor ae)
    {
        aminaEditor = ae;
        aminaEditorUndoOperations = new List<AminaEditorUndoOperation>();
        pointer = -1;
    }
    
    public void Addkey(Vector3Int v3, KeyType _type,IFrameFillPara _fPara)
    {
        switch (v3.z)
        {
            case 0:
                List<AminaEditorUndoOperation> _addList = new List<AminaEditorUndoOperation>();
                _addList.Add(new AEUndoOperation_Addkey(aminaEditor,new Vector2Int(v3.x,v3.y), _type, FrameParaType.x, _fPara));
                _addList.Add(new AEUndoOperation_Addkey(aminaEditor, new Vector2Int(v3.x, v3.y), _type, FrameParaType.y, _fPara));
                _addList.Add(new AEUndoOperation_Addkey(aminaEditor, new Vector2Int(v3.x, v3.y), _type, FrameParaType.a, _fPara));
                AddOperation(new AEUndoOperation_Complex(aminaEditor, _addList));
                break;
            case 1:
                AddOperation(new AEUndoOperation_Addkey(aminaEditor, new Vector2Int(v3.x, v3.y), _type, FrameParaType.x, _fPara));
                break;
            case 2:
                AddOperation(new AEUndoOperation_Addkey(aminaEditor, new Vector2Int(v3.x, v3.y), _type, FrameParaType.y, _fPara));
                break;
            case 3:
                AddOperation(new AEUndoOperation_Addkey(aminaEditor, new Vector2Int(v3.x, v3.y), _type, FrameParaType.a, _fPara));
                break;
            default:
                Debug.LogWarning("v3.z out of range value="+ v3.z);
                break;

        }
    }

    public void DeleteKey(Vector3Int v3)
    {
        switch (v3.z)
        {
            case 0:
                List<AminaEditorUndoOperation> _delList = new List<AminaEditorUndoOperation>();
                _delList.Add(new AEUndoOperation_Deletekey(aminaEditor, new Vector2Int(v3.x, v3.y),FrameParaType.x));
                _delList.Add(new AEUndoOperation_Deletekey(aminaEditor, new Vector2Int(v3.x, v3.y), FrameParaType.y));
                _delList.Add(new AEUndoOperation_Deletekey(aminaEditor, new Vector2Int(v3.x, v3.y), FrameParaType.a));
                AddOperation(new AEUndoOperation_Complex(aminaEditor, _delList));
                break;
            case 1:
                AddOperation(new AEUndoOperation_Deletekey(aminaEditor, new Vector2Int(v3.x, v3.y), FrameParaType.x));
                break;
            case 2:
                AddOperation(new AEUndoOperation_Deletekey(aminaEditor, new Vector2Int(v3.x, v3.y), FrameParaType.y));
                break;
            case 3:
                AddOperation(new AEUndoOperation_Deletekey(aminaEditor, new Vector2Int(v3.x, v3.y), FrameParaType.a));
                break;
            default:
                Debug.LogWarning("v3.z out of range value=" + v3.z);
                break;

        }
    }

    public void AddWholeKey(int _index)
    {
        List<AminaEditorUndoOperation> _addList = new List<AminaEditorUndoOperation>();
        for(int i = 0; i < aminaEditor.Components.Count; i++)
        {
            Vector2Int k = new Vector2Int(i, _index);
            _addList.Add(new AEUndoOperation_Addkey(aminaEditor, k,KeyType.LinearKey,FrameParaType.x,null));
            _addList.Add(new AEUndoOperation_Addkey(aminaEditor, k, KeyType.LinearKey, FrameParaType.y, null));
            _addList.Add(new AEUndoOperation_Addkey(aminaEditor, k, KeyType.LinearKey, FrameParaType.a, null));
        }

        if (_addList.Count > 0)
        {
            AddOperation(new AEUndoOperation_Complex(aminaEditor, _addList));
        }
    }

    public void DeleteSelected()
    {
        List<AminaEditorUndoOperation> _delList = new List<AminaEditorUndoOperation>();


        for (int i = 0; i < aminaEditor.Components.Count; i++)
        {
            //Debug.Log("Components[i].SelectionKeyFrame.Count" + aminaEditor.Components[i].SelectionKeyFrame.Count);
            for (int j = 0; j < aminaEditor.Components[i].SelectionKeyFrame.Count; j++)
            {


                SelectedKeyData _k = aminaEditor.Components[i].SelectionKeyFrame[j];

                Vector2Int v2 = new Vector2Int(i, _k.Index);
                if(_k.x) _delList.Add(new AEUndoOperation_Deletekey(aminaEditor, v2, FrameParaType.x));
                if (_k.y) _delList.Add(new AEUndoOperation_Deletekey(aminaEditor, v2, FrameParaType.y));
                if (_k.a) _delList.Add(new AEUndoOperation_Deletekey(aminaEditor, v2, FrameParaType.a));


                //delList.Add(new AEUndoOperation_Deletekey(aminaEditor,k));
            }
        }

        if (_delList.Count == 0) return;

        aminaEditor.ClearSelectKey();
        AddOperation(new AEUndoOperation_Complex(aminaEditor, _delList));

    }

    public void PasteKey(int _keyPointer,List<CopyKeyData> _copies)
    {
        List<AminaEditorUndoOperation> _Addkeys = new List<AminaEditorUndoOperation>();

        foreach(CopyKeyData _kd in _copies)
        {
            Vector2Int v2 = new Vector2Int();
            v2.x = _kd.componentIndex; v2.y = _keyPointer + _kd.offset;

            _Addkeys.Add(new AEUndoOperation_Addkey(aminaEditor, v2,_kd.para, _kd.type, _kd.paraType,_kd.fillPara));
        }

        AddOperation(new AEUndoOperation_Complex(aminaEditor, _Addkeys));
    }

    public void MoveSelectedKey(int _delta)
    {
        List<List<MoveKeyData>> _list = new List<List<MoveKeyData>>();

        for(int i = 0; i < aminaEditor.Components.Count; i++)
        {
            _list.Add(new List<MoveKeyData>());

            for(int j=0;j< aminaEditor.Components[i].SelectionKeyFrame.Count; j++)
            {
                SelectedKeyData _s = aminaEditor.Components[i].SelectionKeyFrame[j];
                if (_s.Index + _delta >= 0)
                {
                    MoveKeyData _m = new MoveKeyData(i, _s.Index);
                    if (_s.x) _m.Selected.Add(FrameParaType.x);
                    if (_s.y) _m.Selected.Add(FrameParaType.y);
                    if (_s.a) _m.Selected.Add(FrameParaType.a);
                    _list[i].Add(_m);
                }
                //Vector2Int k = new Vector2Int( aminaEditor.Components[i].GetSelectKey(j),i);
                
            }
        }
        //AddOperation(new AEUndoOperation_MoveKeys(aminaEditor, _list, _delta));
        AddOperation(new AEUndoOperation_MoveKey(aminaEditor, _list, _delta));
    }

    public void MoveSelectedKey_Atlas(int _delta)
    {
        AEUndoOperation_AtlasMoveKey _move = new AEUndoOperation_AtlasMoveKey(aminaEditor, aminaEditor.atlas.SelectionKeyFrame, _delta);
        if (_move.IsCanMoveKeys())
        {
            AddOperation(_move);
        }
    }

    public void AddAtlasAlphaKey(int _index,int _alpha)
    {
        AddOperation(new AEUndoOperation_AtlasAddAlpha(aminaEditor, _index, _alpha));

    }

    public void AddAtlasSpriteKey(int _index, Sprite _sp)
    {
        AddOperation(new AEUndoOperation_AtlasAddSprite(aminaEditor, _index, _sp));
    }

    public void AddAtlasMultiSpritesKey(List<TempSpriteKey> _sp)
    {
        List<AminaEditorUndoOperation> _Addkeys = new List<AminaEditorUndoOperation>();

        for(int i = 0; i < _sp.Count; i++)
        {
            _Addkeys.Add(new AEUndoOperation_AtlasAddSprite(aminaEditor, _sp[i].Index, _sp[i].Sp));
        }

        AddOperation(new AEUndoOperation_Complex(aminaEditor,_Addkeys));

    }

    public void AddPauseTime(int _compIndex,int[] _pasueTime)
    {
        AddOperation(new AEUndoOperation_AddPauseTime(aminaEditor, _compIndex, _pasueTime));
    }

    public void UndoOperation()
    {
        if (pointer > 0)
        {
            aminaEditorUndoOperations[pointer].UndoOperation();
            pointer--;
            aminaEditor.Repaint();
        }

    }
    public void DoOperation()
    {
        if(pointer+1< aminaEditorUndoOperations.Count)
        {
            pointer++;
            aminaEditorUndoOperations[pointer].DoOperation();
            aminaEditor.Repaint();
            //Debug.Log("Do op");
        }
    }

    private void AddOperation(AminaEditorUndoOperation newOp)
    {
        if (pointer == -1 )
        {
            aminaEditorUndoOperations.Add(newOp);
            pointer++;
            newOp.FirstOperation();
        }else if(pointer == aminaEditorUndoOperations.Count - 1)
        {
            if (aminaEditorUndoOperations.Count > 30)
            {
                aminaEditorUndoOperations.RemoveAt(0);
                aminaEditorUndoOperations.Add(newOp);
                newOp.FirstOperation();
            }
            else
            {
                aminaEditorUndoOperations.Add(newOp);
                pointer++;
                newOp.FirstOperation();
            }
        }
        else
        {
            for(int i= aminaEditorUndoOperations.Count-1; i>pointer; i--)
            {
                aminaEditorUndoOperations.RemoveAt(i);
            }

            aminaEditorUndoOperations.Add(newOp);
            pointer++;
            newOp.FirstOperation();
        }


        aminaEditor.Repaint();
    }

}

public abstract class AminaEditorUndoOperation
{
    public AminaEditorUndoOperation(AminaEditor ae)
    {
        aminaEditor = ae;
    }
    protected AminaEditor aminaEditor;
    public abstract void DoOperation();
    public abstract void UndoOperation();
    public abstract void FirstOperation();
}

public class AEUndoOperation_Addkey : AminaEditorUndoOperation
{
    IAEUndoOperationHelper helper;

    Vector2Int index;

    FrameParaType paraType;

    public AEUndoOperation_Addkey(AminaEditor ae, Vector2Int _index,KeyType _type, FrameParaType _ptype, IFrameFillPara _fillPara) : base(ae)
    {
        index = _index;
        paraType = _ptype;

        float _value = 0;
        switch (_ptype)
        {
            case FrameParaType.x:
                _value = aminaEditor.Components[_index.x].ComponentGO.transform.localPosition.x;
                break;
            case FrameParaType.y:
                _value = aminaEditor.Components[_index.x].ComponentGO.transform.localPosition.y;
                break;
            case FrameParaType.a:
                _value = TransformUtils.GetInspectorRotation(aminaEditor.Components[_index.x].ComponentGO.transform).z;
                break;
        }
        
        if (!aminaEditor.Components[_index.x].clip.IsTheIndexHaveKey(_index.y, paraType))
        {
            helper = new AEUndoOperation_Addkey_NewAdd(_value, _type,_fillPara);
        }
        else
        {
            float _oldValue = 0;
            KeyType _oldType= KeyType.NotKey;
            IFrameFillPara _oldFillPara = null;
            switch (_ptype)
            {
                case FrameParaType.x:
                    _oldValue = aminaEditor.Components[index.x].clip.Frames[index.y].x;
                    _oldType = aminaEditor.Components[index.x].clip.Frames[index.y].XKey;
                    _oldFillPara = aminaEditor.Components[index.x].clip.Frames[index.y].XfillPara;
                    break;
                case FrameParaType.y:
                    _oldValue = aminaEditor.Components[index.x].clip.Frames[index.y].y;
                    _oldType = aminaEditor.Components[index.x].clip.Frames[index.y].YKey;
                    _oldFillPara = aminaEditor.Components[index.x].clip.Frames[index.y].YfillPara;
                    break;
                case FrameParaType.a:
                    _oldValue = aminaEditor.Components[index.x].clip.Frames[index.y].angle;
                    _oldType = aminaEditor.Components[index.x].clip.Frames[index.y].AKey;
                    _oldFillPara = aminaEditor.Components[index.x].clip.Frames[index.y].AfillPara;
                    break;
            }

            helper = new AEUndoOperation_AddKey_Coveredkey(_value, _type,_fillPara, _oldValue, _oldType, _oldFillPara);
        }
    }

    public AEUndoOperation_Addkey(AminaEditor ae, Vector2Int _index,float _value,KeyType _type, FrameParaType _ptype,IFrameFillPara _fillPara) : base(ae)
    {
        index = _index;
        paraType = _ptype;

        if (!aminaEditor.Components[_index.x].clip.IsTheIndexHaveKey(_index.y,paraType))
        {
            helper = new AEUndoOperation_Addkey_NewAdd(_value, _type, _fillPara);
        }
        else
        {
            float _oldValue = 0;
            KeyType _oldType = KeyType.NotKey;
            IFrameFillPara _oldFillPara = null;
            switch (_ptype)
            {
                case FrameParaType.x:
                    _oldValue = aminaEditor.Components[index.x].clip.Frames[index.y].x;
                    _oldType = aminaEditor.Components[index.x].clip.Frames[index.y].XKey;
                    _oldFillPara = aminaEditor.Components[index.x].clip.Frames[index.y].XfillPara;
                    break;
                case FrameParaType.y:
                    _oldValue = aminaEditor.Components[index.x].clip.Frames[index.y].y;
                    _oldType = aminaEditor.Components[index.x].clip.Frames[index.y].YKey;
                    _oldFillPara = aminaEditor.Components[index.x].clip.Frames[index.y].YfillPara;
                    break;
                case FrameParaType.a:
                    _oldValue = aminaEditor.Components[index.x].clip.Frames[index.y].angle;
                    _oldType = aminaEditor.Components[index.x].clip.Frames[index.y].AKey;
                    _oldFillPara = aminaEditor.Components[index.x].clip.Frames[index.y].AfillPara;
                    break;
            }
            helper = new AEUndoOperation_AddKey_Coveredkey(_value, _type, _fillPara, _oldValue, _oldType,_oldFillPara);
        }
    }

    public override void FirstOperation()
    {
        helper.DoOperation(aminaEditor, index,paraType);
    }

    public override void DoOperation()
    {
        //aminaEditor.Components[index.x].clip.AddKeyFrame(pos, ang, index.y);
        
        helper.DoOperation(aminaEditor, index, paraType);
    }

    public override void UndoOperation()
    {
        helper.UndoOperation(aminaEditor, index, paraType);
        //aminaEditor.Components[index.x].clip.DeleteKeyFrame(index.y);
    }
}

public interface IAEUndoOperationHelper
{
    void DoOperation(AminaEditor ae, Vector2Int index, FrameParaType _pType);
    void UndoOperation(AminaEditor ae, Vector2Int index, FrameParaType _pType);
}

public class AEUndoOperation_Addkey_NewAdd:IAEUndoOperationHelper
{
    float keyValue;
    //Vector2Int index;
    KeyType type;
    IFrameFillPara fillPara;

    public AEUndoOperation_Addkey_NewAdd(float _value,KeyType _type,IFrameFillPara _fillpara)
    {
        keyValue = _value;
        type = _type;
        fillPara = _fillpara;
    }

    public void DoOperation(AminaEditor ae,Vector2Int index,FrameParaType _pType)
    {
        //Debug.Log("AEUndoOperation_Addkey_NewAdd");
        switch (_pType)
        {
            case FrameParaType.x:
                ae.Components[index.x].clip.AddXKeyFrame(keyValue, index.y, type, fillPara);
                break;
            case FrameParaType.y:
                ae.Components[index.x].clip.AddYKeyFrame(keyValue, index.y, type, fillPara);
                break;
            case FrameParaType.a:
                ae.Components[index.x].clip.AddAKeyFrame(keyValue, index.y, type, fillPara);
                break;
        }
        //ae.Components[index.x].clip.AddKeyFrame(pos, ang,new EditorAminaComponentClip.Key(index.y,type));
    }

    public void UndoOperation(AminaEditor ae, Vector2Int index, FrameParaType _pType)
    {
        //Debug.Log("AEUndoOperation_Addkey_NewAdd Undo");
        switch (_pType)
        {
            case FrameParaType.x:
                ae.Components[index.x].clip.DeleteXKeyFrame(index.y);
                break;
            case FrameParaType.y:
                ae.Components[index.x].clip.DeleteYKeyFrame(index.y);
                break;
            case FrameParaType.a:
                ae.Components[index.x].clip.DeleteAKeyFrame(index.y);
                break;
        }
    }
}

public class AEUndoOperation_AddKey_Coveredkey : IAEUndoOperationHelper
{
    //Vector2Int index;

    float keyValue;
    KeyType type;
    IFrameFillPara fillPara;

    float oldKeyValue;
    KeyType oldType;
    IFrameFillPara oldFillPara;

    public AEUndoOperation_AddKey_Coveredkey(float _value,KeyType _type, IFrameFillPara _fillPara, float _oldValue,KeyType _oldType, IFrameFillPara _OldFillPara)
    {
        keyValue = _value;
        type = _type;
        fillPara = _fillPara;

        oldKeyValue = _oldValue;
        oldType = _oldType;
        oldFillPara = _OldFillPara;
    }

    public void DoOperation(AminaEditor ae, Vector2Int index,FrameParaType _pType)
    {
        switch (_pType)
        {
            case FrameParaType.x:
                ae.Components[index.x].clip.AddXKeyFrame(keyValue, index.y, type,fillPara);
                break;
            case FrameParaType.y:
                ae.Components[index.x].clip.AddYKeyFrame(keyValue, index.y, type, fillPara);
                break;
            case FrameParaType.a:
                ae.Components[index.x].clip.AddAKeyFrame(keyValue, index.y, type, fillPara);
                break;
        }
    }

    public void UndoOperation(AminaEditor ae, Vector2Int index, FrameParaType _pType)
    {
        switch (_pType)
        {
            case FrameParaType.x:
                ae.Components[index.x].clip.AddXKeyFrame(oldKeyValue, index.y, oldType,oldFillPara);
                break;
            case FrameParaType.y:
                ae.Components[index.x].clip.AddYKeyFrame(oldKeyValue, index.y, oldType, oldFillPara);
                break;
            case FrameParaType.a:
                ae.Components[index.x].clip.AddAKeyFrame(oldKeyValue, index.y, oldType, oldFillPara);
                break;
        }
    }
}

public class AEUndoOperation_Deletekey : AminaEditorUndoOperation
{
    float para;
    Vector2Int index;
    KeyType type;
    FrameParaType paraType;
    IFrameFillPara fillPara;

    public AEUndoOperation_Deletekey(AminaEditor ae, Vector2Int _index, FrameParaType _pType) : base(ae)
    {
        index = _index;
        paraType = _pType;
        switch (paraType)
        {
            case FrameParaType.x:
                para = aminaEditor.Components[index.x].clip.Frames[index.y].x;
                type = aminaEditor.Components[index.x].clip.Frames[index.y].XKey;
                fillPara = aminaEditor.Components[index.x].clip.Frames[index.y].XfillPara;
                break;
            case FrameParaType.y:
                para = aminaEditor.Components[index.x].clip.Frames[index.y].y;
                type = aminaEditor.Components[index.x].clip.Frames[index.y].YKey;
                fillPara = aminaEditor.Components[index.x].clip.Frames[index.y].YfillPara;
                break;
            case FrameParaType.a:
                para = aminaEditor.Components[index.x].clip.Frames[index.y].angle;
                type = aminaEditor.Components[index.x].clip.Frames[index.y].AKey;
                fillPara = aminaEditor.Components[index.x].clip.Frames[index.y].AfillPara;
                break;
        }
    }

    public override void FirstOperation()
    {
        switch (paraType)
        {
            case FrameParaType.x:
                aminaEditor.Components[index.x].clip.DeleteXKeyFrame(index.y);
                break;
            case FrameParaType.y:
                aminaEditor.Components[index.x].clip.DeleteYKeyFrame(index.y);
                break;
            case FrameParaType.a:
                aminaEditor.Components[index.x].clip.DeleteAKeyFrame(index.y);
                break;
        }
    }

    public override void DoOperation()
    {
        switch (paraType)
        {
            case FrameParaType.x:
                aminaEditor.Components[index.x].clip.DeleteXKeyFrame(index.y);
                break;
            case FrameParaType.y:
                aminaEditor.Components[index.x].clip.DeleteYKeyFrame(index.y);
                break;
            case FrameParaType.a:
                aminaEditor.Components[index.x].clip.DeleteAKeyFrame(index.y);
                break;
        }
    }

    public override void UndoOperation()
    {
        switch (paraType)
        {
            case FrameParaType.x:
                aminaEditor.Components[index.x].clip.AddXKeyFrame(para, index.y, type,fillPara);
                break;
            case FrameParaType.y:
                aminaEditor.Components[index.x].clip.AddYKeyFrame(para, index.y, type, fillPara);
                break;
            case FrameParaType.a:
                aminaEditor.Components[index.x].clip.AddAKeyFrame(para, index.y, type, fillPara);
                break;
        }
    }
}
#region
/*
public class AEUndoOperation_MoveKeys: AminaEditorUndoOperation
{
    int deltaIndex;
    List<List<MoveKeyData>> selectFrames;
    
    public AEUndoOperation_MoveKeys(AminaEditor ae, List<List<MoveKeyData>> _frames,int _delta):base(ae)
    {
        selectFrames = _frames;
        deltaIndex = _delta;
    }

    public override void FirstOperation()
    {
        if (deltaIndex < 0)
        {
            for (int i = 0; i < aminaEditor.Components.Count; i++)
            {
                for (int j = 0; j < selectFrames[i].Count; j++)
                {
                    MoveKeyData k = selectFrames[i][j];

                    foreach (FrameParaType _p in k.Selected)
                    {
                        aminaEditor.Components[k.ComponentIndex].clip.MoveXYAKey(k.FrameIndex, deltaIndex, _p);
                    }
                    aminaEditor.Components[i].SelectionKeyFrame[j].Index = aminaEditor.Components[i].SelectionKeyFrame[j].Index + deltaIndex;
                }
            }
        }
        else
        {
            for (int i = 0; i < aminaEditor.Components.Count; i++)
            {
                for (int j = selectFrames[i].Count - 1; j >= 0; j--)
                {
                    //Vector2Int k = selectFrames[i][j];
                    //aminaEditor.Components[k.y].clip.MoveKey(k.x, deltaIndex);
                    MoveKeyData k = selectFrames[i][j];

                    foreach (FrameParaType _p in k.Selected)
                    {
                        aminaEditor.Components[k.ComponentIndex].clip.MoveXYAKey(k.FrameIndex, deltaIndex, _p);
                    }
                    aminaEditor.Components[i].SelectionKeyFrame[j].Index = aminaEditor.Components[i].SelectionKeyFrame[j].Index + deltaIndex;
                }
            }
        }
    }

    public override void DoOperation()
    {
        if (deltaIndex < 0)
        {
            for(int i = 0; i < aminaEditor.Components.Count; i++)
            {
                for (int j = 0; j < selectFrames[i].Count; j++)
                {
                    MoveKeyData k = selectFrames[i][j];

                    foreach (FrameParaType _p in k.Selected)
                    {
                        aminaEditor.Components[k.ComponentIndex].clip.MoveXYAKey(k.FrameIndex, deltaIndex, _p);
                    }
                    //aminaEditor.Components[i].SelectionKeyFrame[j] = aminaEditor.Components[i].SelectionKeyFrame[j] + deltaIndex;
                }
            }
        }
        else
        {
            for (int i = 0; i < aminaEditor.Components.Count; i++)
            {
                for (int j = selectFrames[i].Count-1; j >=0; j--)
                {
                    MoveKeyData k = selectFrames[i][j];

                    foreach (FrameParaType _p in k.Selected)
                    {
                        aminaEditor.Components[k.ComponentIndex].clip.MoveXYAKey(k.FrameIndex, deltaIndex, _p);
                    }
                    //aminaEditor.Components[i].SelectionKeyFrame[j] = aminaEditor.Components[i].SelectionKeyFrame[j] + deltaIndex;
                }
            }
        }
    }

    public override void UndoOperation()
    {
        if (deltaIndex > 0)
        {
            for (int i = 0; i < aminaEditor.Components.Count; i++)
            {
                for (int j = 0; j < selectFrames[i].Count; j++)
                {
                    MoveKeyData k = selectFrames[i][j];

                    foreach (FrameParaType _p in k.Selected)
                    {
                        aminaEditor.Components[k.ComponentIndex].clip.MoveXYAKey(k.FrameIndex + deltaIndex, -deltaIndex, _p);
                    }

                    //aminaEditor.Components[k.y].clip.MoveKey(k.x+deltaIndex, -deltaIndex);
                    //aminaEditor.Components[i].SelectionKeyFrame[j] = aminaEditor.Components[i].SelectionKeyFrame[j] - deltaIndex;
                }
            }
        }
        else
        {
            for (int i = 0; i < aminaEditor.Components.Count; i++)
            {
                for (int j = selectFrames[i].Count - 1; j >= 0; j--)
                {
                    //Vector2Int k = selectFrames[i][j];
                    //aminaEditor.Components[k.y].clip.MoveKey(k.x+deltaIndex, -deltaIndex);
                    MoveKeyData k = selectFrames[i][j];

                    foreach (FrameParaType _p in k.Selected)
                    {
                        aminaEditor.Components[k.ComponentIndex].clip.MoveXYAKey(k.FrameIndex + deltaIndex, -deltaIndex, _p);
                    }
                    //aminaEditor.Components[i].SelectionKeyFrame[j] = aminaEditor.Components[i].SelectionKeyFrame[j] - deltaIndex;
                }
            }
        }
    }
}
*/
#endregion
public class AEUndoOperation_MoveKey : AminaEditorUndoOperation
{
    int deltaIndex;
    List<List<MoveKeyData>> selectFrames;
    //List<AEUndoOperation_Addkey> addkeyOps;
    List<AEUndoOperation_Deletekey> dleKeyOps;
    public AEUndoOperation_MoveKey(AminaEditor ae, List<List<MoveKeyData>> _frames, int _delta) : base(ae)
    {
        selectFrames = _frames;
        deltaIndex = _delta;
        dleKeyOps = new List<AEUndoOperation_Deletekey>();
    }

    public override void FirstOperation()
    {
        if (deltaIndex < 0)
        {
            for (int i = 0; i < aminaEditor.Components.Count; i++)
            {
                for (int j = 0; j < selectFrames[i].Count; j++)
                {
                    MoveKeyData k = selectFrames[i][j];

                    Vector2Int _newVec2 = new Vector2Int(k.ComponentIndex, k.FrameIndex + deltaIndex);


                    foreach (FrameParaType _p in k.Selected)
                    {
                       if(aminaEditor.Components[k.ComponentIndex].clip.IsTheIndexHaveKey(_newVec2.y, _p))
                       {
                            //Debug.Log("delete");
                            dleKeyOps.Add(new AEUndoOperation_Deletekey(aminaEditor, _newVec2, _p));
                       }
                        aminaEditor.Components[k.ComponentIndex].clip.MoveXYAKey(k.FrameIndex, deltaIndex, _p);
                    }
                    aminaEditor.Components[i].SelectionKeyFrame[j].Index = aminaEditor.Components[i].SelectionKeyFrame[j].Index + deltaIndex;
                }
            }
        }
        else
        {
            for (int i = 0; i < aminaEditor.Components.Count; i++)
            {
                for (int j = selectFrames[i].Count - 1; j >= 0; j--)
                {
                    //Vector2Int k = selectFrames[i][j];
                    //aminaEditor.Components[k.y].clip.MoveKey(k.x, deltaIndex);
                    MoveKeyData k = selectFrames[i][j];
                    Vector2Int _newVec2 = new Vector2Int(k.ComponentIndex, k.FrameIndex + deltaIndex);
                    foreach (FrameParaType _p in k.Selected)
                    {
                        if (aminaEditor.Components[k.ComponentIndex].clip.IsTheIndexHaveKey(_newVec2.y, _p))
                        {
                            //Debug.Log("delete");
                            dleKeyOps.Add(new AEUndoOperation_Deletekey(aminaEditor, _newVec2, _p));
                        }
                        aminaEditor.Components[k.ComponentIndex].clip.MoveXYAKey(k.FrameIndex, deltaIndex, _p);
                    }
                    aminaEditor.Components[i].SelectionKeyFrame[j].Index = aminaEditor.Components[i].SelectionKeyFrame[j].Index + deltaIndex;
                }
            }
        }
    }

    public override void DoOperation()
    {
        if (deltaIndex < 0)
        {
            for (int i = 0; i < aminaEditor.Components.Count; i++)
            {
                for (int j = 0; j < selectFrames[i].Count; j++)
                {
                    MoveKeyData k = selectFrames[i][j];

                    foreach (FrameParaType _p in k.Selected)
                    {
                        aminaEditor.Components[k.ComponentIndex].clip.MoveXYAKey(k.FrameIndex, deltaIndex, _p);
                    }
                    //aminaEditor.Components[i].SelectionKeyFrame[j] = aminaEditor.Components[i].SelectionKeyFrame[j] + deltaIndex;
                }
            }
        }
        else
        {
            for (int i = 0; i < aminaEditor.Components.Count; i++)
            {
                for (int j = selectFrames[i].Count - 1; j >= 0; j--)
                {
                    MoveKeyData k = selectFrames[i][j];

                    foreach (FrameParaType _p in k.Selected)
                    {
                        aminaEditor.Components[k.ComponentIndex].clip.MoveXYAKey(k.FrameIndex, deltaIndex, _p);
                    }
                    //aminaEditor.Components[i].SelectionKeyFrame[j] = aminaEditor.Components[i].SelectionKeyFrame[j] + deltaIndex;
                }
            }
        }
    }

    public override void UndoOperation()
    {
        if (deltaIndex > 0)
        {
            for (int i = 0; i < aminaEditor.Components.Count; i++)
            {
                for (int j = 0; j < selectFrames[i].Count; j++)
                {
                    MoveKeyData k = selectFrames[i][j];

                    foreach (FrameParaType _p in k.Selected)
                    {
                        aminaEditor.Components[k.ComponentIndex].clip.MoveXYAKey(k.FrameIndex + deltaIndex, -deltaIndex, _p);
                    }

                    //aminaEditor.Components[k.y].clip.MoveKey(k.x+deltaIndex, -deltaIndex);
                    //aminaEditor.Components[i].SelectionKeyFrame[j] = aminaEditor.Components[i].SelectionKeyFrame[j] - deltaIndex;
                }
            }
        }
        else
        {
            for (int i = 0; i < aminaEditor.Components.Count; i++)
            {
                for (int j = selectFrames[i].Count - 1; j >= 0; j--)
                {
                    //Vector2Int k = selectFrames[i][j];
                    //aminaEditor.Components[k.y].clip.MoveKey(k.x+deltaIndex, -deltaIndex);
                    MoveKeyData k = selectFrames[i][j];

                    foreach (FrameParaType _p in k.Selected)
                    {
                        aminaEditor.Components[k.ComponentIndex].clip.MoveXYAKey(k.FrameIndex + deltaIndex, -deltaIndex, _p);
                    }
                    //aminaEditor.Components[i].SelectionKeyFrame[j] = aminaEditor.Components[i].SelectionKeyFrame[j] - deltaIndex;
                }
            }
        }

        for(int i=0;i< dleKeyOps.Count; i++)
        {
            dleKeyOps[i].UndoOperation();
        }

    }
}

public class AEUndoOperation_AtlasMoveKey : AminaEditorUndoOperation
{
    int deltaIndex;
    List<SelectedKeyData> selectFrames;
    List<AminaEditorUndoOperation> keyOps;
    public AEUndoOperation_AtlasMoveKey(AminaEditor ae, List<SelectedKeyData> _frames, int _delta) : base(ae)
    {
        selectFrames = new List<SelectedKeyData>();
        for(int i = 0; i < _frames.Count; i++)
        {
            selectFrames.Add(new SelectedKeyData(_frames[i]));
        }
        selectFrames.Sort();
        deltaIndex = _delta;
        keyOps = new List<AminaEditorUndoOperation>();
    }

    /// <summary>
    /// 能否移动的判断都写在这里
    /// </summary>
    /// <returns></returns>
    public bool IsCanMoveKeys()
    {
        if(selectFrames.Count>0 && selectFrames[0].Index + deltaIndex >= 0)
        {
            SelectedKeyData _last = selectFrames[selectFrames.Count - 1];

            //最后一帧移动后不加新帧
            if (_last.Index + deltaIndex < aminaEditor.atlas.Frames.Count)
                return true;

            //最后一帧移动后加新帧  要保证移动帧的最后一帧有图片
            if (_last.sp) return true;

            if (_last.all)
            {
                if (aminaEditor.atlas.Frames[_last.Index].isKeySprite)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override void FirstOperation()
    {
        if (deltaIndex < 0)//向左移动  从左往右依次添加删除
        {
            for (int i = 0; i < selectFrames.Count; i++)
            {
                FirstOperationFunc(i,false);
            }
        }
        else//向左移动  从右往左依次添加删除
        {
            for (int i = selectFrames.Count-1; i >=0; i--)
            {
                FirstOperationFunc(i,true);
            }
        }
    }

    public override void DoOperation()
    {
        for(int i = 0; i < keyOps.Count; i++)
        {
            keyOps[i].DoOperation();
        }
    }

    public override void UndoOperation()
    {
        for (int i = keyOps.Count-1; i >=0; i--)
        {
            keyOps[i].UndoOperation();
        }

    }

    private void FirstOperationFunc(int _i,bool _isMoveRight)
    {
        if (selectFrames[_i].all)
        {
            AEUndoOperation_AtlasMoveWholeKey _amw = new AEUndoOperation_AtlasMoveWholeKey(aminaEditor, selectFrames[_i].Index + deltaIndex, selectFrames[_i].Index);
            _amw.FirstOperation();
            keyOps.Add(_amw);
        }        
        else
        {
            if(selectFrames[_i].sp && selectFrames[_i].alpha)
            {
                AEUndoOperation_AtlasMoveWholeKey _amw = new AEUndoOperation_AtlasMoveWholeKey(aminaEditor, selectFrames[_i].Index + deltaIndex, selectFrames[_i].Index);
                _amw.FirstOperation();
                keyOps.Add(_amw);
            }else if (selectFrames[_i].sp)
            {
                //添加图片
                AEUndoOperation_AtlasAddSprite _aas = new AEUndoOperation_AtlasAddSprite(aminaEditor,
                    selectFrames[_i].Index + deltaIndex, aminaEditor.atlas.Frames[selectFrames[_i].Index].facePos, aminaEditor.atlas.Frames[selectFrames[_i].Index].sprite, aminaEditor.atlas.Frames[selectFrames[_i].Index].isKeyFace);
                _aas.FirstOperation();
                keyOps.Add(_aas);
                //删除图片
                AEUndoOperation_AtlasDeleteSprite _ads = new AEUndoOperation_AtlasDeleteSprite(aminaEditor, selectFrames[_i].Index);
                _ads.FirstOperation();
                keyOps.Add(_ads);
            }else if(selectFrames[_i].alpha)
            {
                //添加alpha
                AEUndoOperation_AtlasAddAlpha _aaa = new AEUndoOperation_AtlasAddAlpha(aminaEditor,
                    selectFrames[_i].Index + deltaIndex, aminaEditor.atlas.Frames[selectFrames[_i].Index].Alpha);
                _aaa.FirstOperation();
                keyOps.Add(_aaa);
                //删除alpha
                AEUndoOperation_AtlasDeleteAlpha _ada = new AEUndoOperation_AtlasDeleteAlpha(aminaEditor, selectFrames[_i].Index);
                _ada.FirstOperation();
                keyOps.Add(_ada);
            }
        }
    }


}

public class AEUndoOperation_Complex : AminaEditorUndoOperation
{
    List<AminaEditorUndoOperation> operations;

    public AEUndoOperation_Complex(AminaEditor ae, List<AminaEditorUndoOperation> _Addkeys) : base(ae)
    {
        operations = _Addkeys;
    }

    public override void FirstOperation()
    {
        foreach (AminaEditorUndoOperation ami in operations)
        {
            ami.FirstOperation();
        }
    }

    public override void DoOperation()
    {
        foreach(AminaEditorUndoOperation ami in operations)
        {
            ami.DoOperation();
        }
    }

    public override void UndoOperation()
    {
        foreach (AminaEditorUndoOperation ami in operations)
        {
            ami.UndoOperation();
        }
    }

}

public class AEUndoOperation_AddPauseTime:AminaEditorUndoOperation
{
    public int CompIndex;
    public int[] OldPaues;
    public int[] Pause;

    public AEUndoOperation_AddPauseTime(AminaEditor ae,int _compIndex,int[] _pause):base(ae)
    {
        CompIndex = _compIndex;
        OldPaues = ae.Components[CompIndex].clip.PauseTime;
        Pause = _pause;
    }

    public override void FirstOperation()
    {
        aminaEditor.Components[CompIndex].clip.AddPauseTime(Pause);
    }

    public override void DoOperation()
    {
        aminaEditor.Components[CompIndex].clip.AddPauseTime(Pause);
    }

    public override void UndoOperation()
    {
        aminaEditor.Components[CompIndex].clip.AddPauseTime(OldPaues);
    }

}

public class AEUndoOperation_AtlasAddSprite: AminaEditorUndoOperation
{
    public int Index;
    public Sprite NewSprite;
    private bool isCoverd;
    private Sprite OldSprite;
    private Vector3 oldFacePos;
    private bool isAddFacePos;
    private bool isNewKeyFace;//新加的是不是key face，只用于移动关键帧
    private bool isOldKeyFace;
    private Vector3 newFacePos;

    public AEUndoOperation_AtlasAddSprite(AminaEditor ae, int _Index,Sprite _newSprite) : base(ae)
    {
        Index = _Index;
        NewSprite = _newSprite;
        Debug.Log("_Index="+ _Index);
        Debug.Log("Frames.Count=" + ae.atlas.Frames.Count);
        isAddFacePos = false;
        isNewKeyFace = false;
        if (_Index<ae.atlas.Frames.Count&& ae.atlas.Frames[_Index].isKeySprite)
        {
            isCoverd = true;
            OldSprite = ae.atlas.Frames[_Index].sprite;
            oldFacePos = ae.atlas.Frames[_Index].facePos;
            isOldKeyFace = ae.atlas.Frames[_Index].isKeyFace;
        }
        else
        {
            isCoverd = false;
            OldSprite = null;
            isOldKeyFace = false;
        }
    }

    public AEUndoOperation_AtlasAddSprite(AminaEditor ae, int _Index,Vector3 _face, Sprite _newSprite,bool _isKeyFace) : base(ae)
    {
        Index = _Index;
        NewSprite = _newSprite;
        Debug.Log("_Index=" + _Index);
        Debug.Log("Frames.Count=" + ae.atlas.Frames.Count);
        isAddFacePos = true;
        newFacePos = _face;
        isNewKeyFace = _isKeyFace;
        if (_Index < ae.atlas.Frames.Count && ae.atlas.Frames[_Index].isKeySprite)
        {
            isCoverd = true;
            OldSprite = ae.atlas.Frames[_Index].sprite;
            oldFacePos = ae.atlas.Frames[_Index].facePos;
            isOldKeyFace = ae.atlas.Frames[_Index].isKeyFace;
        }
        else
        {
            isCoverd = false;
            OldSprite = null;
            isOldKeyFace = false;
        }
    }

    public override void FirstOperation()
    {
        aminaEditor.atlas.AddNewSpriteKey(NewSprite,Index);
        if (isAddFacePos)
        {
            aminaEditor.atlas.AddNewFacePos(newFacePos, Index);
        }
        aminaEditor.atlas.Frames[Index].isKeyFace = isNewKeyFace;
    }

    public override void DoOperation()
    {
        aminaEditor.atlas.AddNewSpriteKey(NewSprite, Index);
        if (isAddFacePos)
        {
            aminaEditor.atlas.AddNewFacePos(newFacePos, Index);
        }
        aminaEditor.atlas.Frames[Index].isKeyFace = isNewKeyFace;
    }

    public override void UndoOperation()
    {
        if (isCoverd)
        {
            aminaEditor.atlas.AddNewSpriteKey(OldSprite, Index);
            aminaEditor.atlas.AddNewFacePos(oldFacePos, Index);
            aminaEditor.atlas.Frames[Index].isKeyFace = isOldKeyFace;
        }
        else
        {
            aminaEditor.atlas.DeleteSpriteKey(Index);
        }
    }
}

public class AEUndoOperation_AtlasDeleteSprite : AminaEditorUndoOperation
{
    public int Index;
    private Sprite OldSprite;
    AEUndoOperation_AtlasDeleteHelper helper;
    private int Alpha;
    private bool isLastPos;
    private Vector3 Pos;
    private bool isOldKeyFace;

    public AEUndoOperation_AtlasDeleteSprite(AminaEditor ae, int _Index) : base(ae)
    {
        Index = _Index;
        OldSprite = aminaEditor.atlas.Frames[Index].sprite;
        helper = new AEUndoOperation_AtlasDeleteHelper(ae, Index);
        Alpha = -1;
        isLastPos = false;
        if (Index==aminaEditor.atlas.Frames.Count-1)
        {
            if (aminaEditor.atlas.Frames[Index].isKeyAlpha)
            {
                Alpha = aminaEditor.atlas.Frames[Index].Alpha;
            }
            isLastPos = true;

        }
        Pos = aminaEditor.atlas.Frames[Index].facePos;
        isOldKeyFace = aminaEditor.atlas.Frames[Index].isKeyFace;
    }

    public override void FirstOperation()
    {
        aminaEditor.atlas.DeleteSpriteKey(Index);
    }

    public override void DoOperation()
    {
        aminaEditor.atlas.DeleteSpriteKey(Index);
    }

    public override void UndoOperation()
    {
        aminaEditor.atlas.AddNewSpriteKey(OldSprite, Index);
        aminaEditor.atlas.AddNewFacePos(Pos, Index);
        if (Alpha != -1) aminaEditor.atlas.AddNewAlphaKey(Alpha, Index);
        if(isLastPos) aminaEditor.atlas.AddFacePosKey(Pos, Index);
        aminaEditor.atlas.Frames[Index].isKeyFace = isOldKeyFace;
        helper.UndoNonSpriteKey();
    }
}

public class AEUndoOperation_AtlasAddAlpha : AminaEditorUndoOperation
{
    public int Index;
    public int NewAlpha;
    private bool isCoverd;
    private int OldAlpha;

    public AEUndoOperation_AtlasAddAlpha(AminaEditor ae, int _Index, int _newAlpha) : base(ae)
    {
        Index = _Index;
        NewAlpha = _newAlpha;

        if (Index< ae.atlas.Frames.Count && ae.atlas.Frames[_Index].isKeyAlpha)
        {
            isCoverd = true;
            OldAlpha = ae.atlas.Frames[_Index].Alpha;
        }
        else
        {
            isCoverd = false;
        }
    }

    public override void FirstOperation()
    {
        aminaEditor.atlas.AddNewAlphaKey(NewAlpha, Index);
    }

    public override void DoOperation()
    {
        aminaEditor.atlas.AddNewAlphaKey(NewAlpha, Index);
    }

    public override void UndoOperation()
    {
        if (isCoverd)
        {
            aminaEditor.atlas.AddNewAlphaKey(OldAlpha, Index);
        }
        else
        {
            aminaEditor.atlas.DeleteAlphaKey(Index);
        }
    }
}

public class AEUndoOperation_AtlasDeleteAlpha : AminaEditorUndoOperation
{
    public int Index;
    private int OldAlpha;

    public AEUndoOperation_AtlasDeleteAlpha(AminaEditor ae, int _Index) : base(ae)
    {
        Index = _Index;
        OldAlpha = ae.atlas.Frames[_Index].Alpha;
    }

    public override void FirstOperation()
    {
        aminaEditor.atlas.DeleteAlphaKey(Index);
    }

    public override void DoOperation()
    {
        aminaEditor.atlas.DeleteAlphaKey(Index);
    }

    public override void UndoOperation()
    {
        aminaEditor.atlas.AddNewAlphaKey(OldAlpha, Index);
    }
}

/// <summary>
/// 用于复原删除最后一帧时 复原最后一帧之前非sprite帧的数据
/// </summary>
public class AEUndoOperation_AtlasDeleteHelper
{
    bool isLastKey;
    int Index;
    List<NonSpriteData> datas;//从index往前倒序
    EditorAnimaClipAtlas_Container clip;

    public AEUndoOperation_AtlasDeleteHelper(AminaEditor ae,int _index)
    {
        if (ae.atlas.Frames.Count - 1 == _index)
        {
            isLastKey = true;
            Index = _index;

            datas = new List<NonSpriteData>();
            clip = ae.atlas;
            int i = Index-1;
            while (i >= 0 && !clip.Frames[i].isKeySprite)
            {
                if (clip.Frames[i].isKeyAlpha)
                {
                    NonSpriteData nsp = new NonSpriteData() { index = i, alhpa = clip.Frames[i].Alpha };
                    datas.Add(nsp);
                }
                i--;
            }
        }
        else
        {
            isLastKey = false;
        }
    }

    public void UndoNonSpriteKey()
    {
        if (isLastKey)
        {
            for(int i = 0; i < datas.Count; i++)
            {
                Debug.Log("复原");
                clip.AddNewAlphaKey(datas[i].alhpa, datas[i].index);
            }
        }
    }

    private class NonSpriteData
    {
        public int index;
        public int alhpa;
        //public Vector2 pos;
    }

}

public class AEUndoOperation_AtlasMoveWholeKey : AminaEditorUndoOperation
{
    public int NewIndex;
    public int OldIndex;
    public Sprite MovedSprite;
    public Sprite CoveredSprite;
    public int MovedAlpha;
    public int CoveredAlpha;
    public Vector3 MovedFace;
    public Vector3 CoverdFace;
    public bool MovedIsKeyFace;
    public bool CoverdIsKeyFace;

    AEUndoOperation_AtlasDeleteHelper helper;


    public AEUndoOperation_AtlasMoveWholeKey(AminaEditor ae, int _newIndex,int _oldIndex) : base(ae)
    {
        NewIndex = _newIndex;
        OldIndex = _oldIndex;
        helper = null;
        var _oldFrame = aminaEditor.atlas.Frames[OldIndex];

        if (_oldFrame.isKeySprite)
        {
            MovedSprite = _oldFrame.sprite;
            MovedFace = _oldFrame.facePos;
            helper = new AEUndoOperation_AtlasDeleteHelper(ae, OldIndex);
            MovedIsKeyFace = _oldFrame.isKeyFace;
        }
        else MovedSprite = null;

        if (NewIndex < aminaEditor.atlas.Frames.Count && aminaEditor.atlas.Frames[NewIndex].isKeySprite)
        {
            CoveredSprite = aminaEditor.atlas.Frames[NewIndex].sprite;
            CoverdFace = aminaEditor.atlas.Frames[NewIndex].facePos;
            CoverdIsKeyFace = aminaEditor.atlas.Frames[NewIndex].isKeyFace;
        }
        else CoveredSprite = null;

        if (_oldFrame.isKeyAlpha) MovedAlpha = _oldFrame.Alpha;
        else MovedAlpha = -1;

        if (NewIndex < aminaEditor.atlas.Frames.Count && aminaEditor.atlas.Frames[NewIndex].isKeyAlpha)
        {
            CoveredAlpha = aminaEditor.atlas.Frames[NewIndex].Alpha;
        }
        else CoveredAlpha = -1;
    }

    public override void FirstOperation()
    {
        if (MovedSprite != null)
        {
            aminaEditor.atlas.AddNewSpriteKey(MovedSprite,NewIndex);
            aminaEditor.atlas.AddNewFacePos(MovedFace, NewIndex);
            aminaEditor.atlas.Frames[NewIndex].isKeyFace = MovedIsKeyFace;
            aminaEditor.atlas.DeleteSpriteKey(OldIndex);
        }

        if (MovedAlpha != -1)
        {
            aminaEditor.atlas.AddNewAlphaKey(MovedAlpha, NewIndex);

            if (OldIndex < aminaEditor.atlas.Frames.Count)
            {
                aminaEditor.atlas.DeleteAlphaKey(OldIndex);
            }
        }
    }

    public override void DoOperation()
    {
        if (MovedSprite != null)
        {
            aminaEditor.atlas.AddNewSpriteKey(MovedSprite, NewIndex);
            aminaEditor.atlas.AddNewFacePos(MovedFace, NewIndex);
            aminaEditor.atlas.Frames[NewIndex].isKeyFace = MovedIsKeyFace;
            aminaEditor.atlas.DeleteSpriteKey(OldIndex);
        }

        if (MovedAlpha != -1)
        {
            aminaEditor.atlas.AddNewAlphaKey(MovedAlpha, NewIndex);

            if (OldIndex < aminaEditor.atlas.Frames.Count)
            {
                aminaEditor.atlas.DeleteAlphaKey(OldIndex);
            }
        }
    }

    public override void UndoOperation()
    {
        if (MovedSprite != null)
        {
            if (CoveredSprite != null)
            {
                aminaEditor.atlas.AddNewSpriteKey(CoveredSprite, NewIndex);
                aminaEditor.atlas.AddNewFacePos(CoverdFace, NewIndex);
            }
            else
            {
                aminaEditor.atlas.DeleteSpriteKey(NewIndex);
            }

            aminaEditor.atlas.AddNewSpriteKey(MovedSprite, OldIndex);
            aminaEditor.atlas.AddNewFacePos(MovedFace, OldIndex);
            aminaEditor.atlas.Frames[OldIndex].isKeyFace = MovedIsKeyFace;
            if (helper!=null) helper.UndoNonSpriteKey();
        }
        

        if (MovedAlpha != -1)
        {
            if (CoveredAlpha != -1)
            {
                aminaEditor.atlas.AddNewAlphaKey(CoveredAlpha, NewIndex);
            }
            else
            {
                if(MovedSprite == null || CoveredSprite != null)
                {
                    aminaEditor.atlas.DeleteAlphaKey(NewIndex);
                }
                
            }

            aminaEditor.atlas.AddNewAlphaKey(MovedAlpha, OldIndex);
        }


    }

}

public class MoveKeyData
{
    public int ComponentIndex;
    public int FrameIndex;
    public List<FrameParaType> Selected;

    public MoveKeyData(int _ComponentIndex, int _FrameIndex)
    {
        ComponentIndex = _ComponentIndex;
        FrameIndex = _FrameIndex;
        Selected = new List<FrameParaType>();
    }
}

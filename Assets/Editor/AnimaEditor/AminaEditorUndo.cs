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
    List<AEUndoOperation_Addkey> addkeyOps;
    List<AEUndoOperation_Deletekey> dleKeyOps;
    public AEUndoOperation_MoveKey(AminaEditor ae, List<List<MoveKeyData>> _frames, int _delta) : base(ae)
    {
        aminaEditor = ae;
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

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
    
    public void Addkey(Vector2Int v2, KeyType _type)
    {
        AddOperation(new AEUndoOperation_Addkey(aminaEditor, v2, _type));
    }

    public void DeleteKey(Vector2Int v2)
    {
        AddOperation(new AEUndoOperation_Deletekey(aminaEditor, v2));
    }

    public void AddWholeKey(int _index)
    {
        List<AminaEditorUndoOperation> _addList = new List<AminaEditorUndoOperation>();
        for(int i = 0; i < aminaEditor.Components.Count; i++)
        {
            Vector2Int k = new Vector2Int(i, _index);
            _addList.Add(new AEUndoOperation_Addkey(aminaEditor, k,KeyType.LinearKey));
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
            for (int j = 0; j < aminaEditor.Components[i].SelectionKeyFrameCount; j++)
            {
                Vector2Int k = new Vector2Int(i, aminaEditor.Components[i].GetSelectKey(j));
                _delList.Add(new AEUndoOperation_Deletekey(aminaEditor,k));
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

            _Addkeys.Add(new AEUndoOperation_Addkey(aminaEditor, v2, _kd.frame.pos, _kd.frame.angle,_kd.type));

        }

        AddOperation(new AEUndoOperation_Complex(aminaEditor, _Addkeys));
    }

    public void MoveSelectedKey(int _delta)
    {
        List<List<Vector2Int>> _list = new List<List<Vector2Int>>();

        for(int i = 0; i < aminaEditor.Components.Count; i++)
        {
            _list.Add(new List<Vector2Int>());

            for(int j=0;j< aminaEditor.Components[i].SelectionKeyFrameCount; j++)
            {
                Vector2Int k = new Vector2Int( aminaEditor.Components[i].GetSelectKey(j),i);
                _list[i].Add(k);
            }
        }

        AddOperation(new AEUndoOperation_MoveKeys(aminaEditor, _list, _delta));
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


    public AEUndoOperation_Addkey(AminaEditor ae, Vector2Int _index,KeyType _type) : base(ae)
    {
        index = _index;
        Transform _t = aminaEditor.Components[_index.x].ComponentGO.transform;
        Vector2 _pos = _t.localPosition;
        float _ang = TransformUtils.GetInspectorRotation(_t).z;
        if (!aminaEditor.Components[_index.x].clip.IsTheIndexHaveKey(_index.y))
        {
            helper = new AEUndoOperation_Addkey_NewAdd(_pos, _ang, _type);
        }
        else
        {
            Vector2 _oldPos = aminaEditor.Components[index.x].clip.frames[index.y].pos;
            float _oldAng = aminaEditor.Components[index.x].clip.frames[index.y].angle;
            KeyType _oldType = aminaEditor.Components[index.x].clip.GetIndexType(index.y);
            helper = new AEUndoOperation_AddKey_Coveredkey(_pos, _ang, _type, _oldPos, _oldAng, _oldType);
        }
    }

    public AEUndoOperation_Addkey(AminaEditor ae, Vector2Int _index, Vector2 _pos,float _ang,KeyType _type) : base(ae)
    {
        index = _index;
        if (!aminaEditor.Components[_index.x].clip.IsTheIndexHaveKey(_index.y))
        {
            helper = new AEUndoOperation_Addkey_NewAdd(_pos, _ang, _type);
        }
        else
        {
            Vector2 _oldPos = aminaEditor.Components[index.x].clip.frames[index.y].pos;
            float _oldAng = aminaEditor.Components[index.x].clip.frames[index.y].angle;
            KeyType _oldType = aminaEditor.Components[index.x].clip.GetIndexType(index.y);
            helper = new AEUndoOperation_AddKey_Coveredkey(_pos, _ang, _type, _oldPos, _oldAng, _oldType);
        }
    }

    public override void FirstOperation()
    {
        helper.DoOperation(aminaEditor, index);
    }

    public override void DoOperation()
    {
        //aminaEditor.Components[index.x].clip.AddKeyFrame(pos, ang, index.y);
        
        helper.DoOperation(aminaEditor, index);
    }

    public override void UndoOperation()
    {
        helper.UndoOperation(aminaEditor, index);
        //aminaEditor.Components[index.x].clip.DeleteKeyFrame(index.y);
    }
}

public interface IAEUndoOperationHelper
{
    void DoOperation(AminaEditor ae, Vector2Int index);
    void UndoOperation(AminaEditor ae, Vector2Int index);
}

public class AEUndoOperation_Addkey_NewAdd:IAEUndoOperationHelper
{
    Vector2 pos;
    float ang;
    //Vector2Int index;
    KeyType type;

    public AEUndoOperation_Addkey_NewAdd(Vector2 _pos,float _ang,KeyType _type)
    {
        pos = _pos;
        ang = _ang;
        type = _type;
    }

    public void DoOperation(AminaEditor ae,Vector2Int index)
    {
        //Debug.Log("AEUndoOperation_Addkey_NewAdd");
        ae.Components[index.x].clip.AddKeyFrame(pos, ang,new EditorAminaComponentClip.Key(index.y,type));
    }

    public void UndoOperation(AminaEditor ae, Vector2Int index)
    {
        //Debug.Log("AEUndoOperation_Addkey_NewAdd Undo");
        ae.Components[index.x].clip.DeleteKeyFrame(index.y);
    }
}

public class AEUndoOperation_AddKey_Coveredkey : IAEUndoOperationHelper
{
    //Vector2Int index;

    Vector2 pos;
    float ang;
    KeyType type;

    Vector2 oldPos;
    float oldAng;
    KeyType oldType;

    public AEUndoOperation_AddKey_Coveredkey(Vector2 _pos,float _ang,KeyType _type,Vector2 _oldPos,float _oldAng,KeyType _oldType)
    {
        oldPos = _oldPos;
        oldAng = _oldAng;
        oldType = _oldType;

        pos = _pos;
        ang = _ang;
        type = _type;
    }

    public void DoOperation(AminaEditor ae, Vector2Int _index)
    {
        ae.Components[_index.x].clip.AddKeyFrame(pos, ang,new EditorAminaComponentClip.Key(_index.y,type));
    }

    public void UndoOperation(AminaEditor ae, Vector2Int _index)
    {
        ae.Components[_index.x].clip.AddKeyFrame(oldPos, oldAng,new EditorAminaComponentClip.Key(_index.y,oldType));
    }
}

public class AEUndoOperation_Deletekey : AminaEditorUndoOperation
{
    Vector2 pos;
    float ang;
    Vector2Int index;
    KeyType type;

    public AEUndoOperation_Deletekey(AminaEditor ae, Vector2Int _index) : base(ae)
    {
        index = _index;
        AminaFrame _t = aminaEditor.Components[index.x].clip.frames[index.y];
        type = aminaEditor.Components[index.x].clip.GetIndexType(index.y);

        pos = _t.pos;
        ang = _t.angle;
    }

    public override void FirstOperation()
    {
        aminaEditor.Components[index.x].clip.DeleteKeyFrame(index.y);
    }

    public override void DoOperation()
    {
        aminaEditor.Components[index.x].clip.DeleteKeyFrame(index.y);
    }

    public override void UndoOperation()
    {
        aminaEditor.Components[index.x].clip.AddKeyFrame(pos, ang,new EditorAminaComponentClip.Key(index.y,type));
    }
}

public class AEUndoOperation_MoveKeys: AminaEditorUndoOperation
{
    int deltaIndex;
    List<List<Vector2Int>> selectFrames;
    
    public AEUndoOperation_MoveKeys(AminaEditor ae, List<List<Vector2Int>> _frames,int _delta):base(ae)
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
                    Vector2Int k = selectFrames[i][j];
                    aminaEditor.Components[k.y].clip.MoveKey(k.x, deltaIndex);
                    aminaEditor.Components[i].SelectionKeyFrame[j] = aminaEditor.Components[i].SelectionKeyFrame[j] + deltaIndex;
                }
            }
        }
        else
        {
            for (int i = 0; i < aminaEditor.Components.Count; i++)
            {
                for (int j = selectFrames[i].Count - 1; j >= 0; j--)
                {
                    Vector2Int k = selectFrames[i][j];
                    aminaEditor.Components[k.y].clip.MoveKey(k.x, deltaIndex);
                    aminaEditor.Components[i].SelectionKeyFrame[j] = aminaEditor.Components[i].SelectionKeyFrame[j] + deltaIndex;
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
                    Vector2Int k = selectFrames[i][j];
                    aminaEditor.Components[k.y].clip.MoveKey(k.x, deltaIndex);
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
                    Vector2Int k = selectFrames[i][j];
                    aminaEditor.Components[k.y].clip.MoveKey(k.x, deltaIndex);
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
                    Vector2Int k = selectFrames[i][j];
                    aminaEditor.Components[k.y].clip.MoveKey(k.x+deltaIndex, -deltaIndex);
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
                    Vector2Int k = selectFrames[i][j];
                    aminaEditor.Components[k.y].clip.MoveKey(k.x+deltaIndex, -deltaIndex);
                    //aminaEditor.Components[i].SelectionKeyFrame[j] = aminaEditor.Components[i].SelectionKeyFrame[j] - deltaIndex;
                }
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

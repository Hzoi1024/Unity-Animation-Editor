using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

[CreateAssetMenu(fileName = "CharactorAnimationEditorConfigs", menuName = "Custom/角色动画配置")]
public class EditorCharactorAnimationConfigs : ScriptableObject
{
    private List<AnimaDataCharactor> charactors;
    public List<AnimaDataCharactor> Charactors { get { return charactors; } }
}

public enum AnimaCharactorType
{
    Def,//纯动画
    Idle,
    Move,
    Atk,
    UdrAtk,//受击
    Spl,//特殊动作
    Dead,//死亡
    Die,//死亡动画
}




/// <summary>
/// 纯动画的so  用来在动画编辑器中存储纯动画
/// </summary>
public class AnimaDataOnlyScritable : ScriptableObject,IComparable<AnimaDataOnlyScritable>,IEquatable<AnimaDataOnlyScritable>
{
    public int ID;
    public EditorAnimaClipAtlas Clip;
    public string Name;

    public AnimaDataOnlyScritable(int _id)
    {
        ID = _id;
        Clip = new EditorAnimaClipAtlas();
        Name = "NewAnima";
    }

    public int CompareTo(AnimaDataOnlyScritable _other)
    {
        if (_other == null)
            return 1;
        else
            return this.ID.CompareTo(_other.ID);
    }

    public bool Equals(AnimaDataOnlyScritable _other)
    {
        if (_other == null)
            return false;
        else
            return (this.ID.Equals(_other.ID));
    }
}



[Serializable]
public class EditorAnimaClipAtlas
{
    public int FPS;
    public AnimaCharactorType CharactorType;
    public List<EditorSpriteFrame> Frames;
    public List<FrameOutputData_Atlas> Output;
    public EditorAnimaClipAtlas()
    {
        FPS = 30;
        Frames = new List<EditorSpriteFrame>();
        Output = new List<FrameOutputData_Atlas>();
    }

    public EditorAnimaClipAtlas(EditorAnimaClipAtlas _e)
    {
        FPS = _e.FPS;
        CharactorType = _e.CharactorType;
        Frames = new List<EditorSpriteFrame>();
        Output = new List<FrameOutputData_Atlas>();
        for (int i = 0; i < _e.Frames.Count; i++)
        {
            EditorSpriteFrame _new = new EditorSpriteFrame(_e.Frames[i]);
            Frames.Add(_new);
        }

        for (int i = 0; i < _e.Output.Count; i++)
        {
            FrameOutputData_Atlas _new = new FrameOutputData_Atlas(_e.Output[i]);
            Output.Add(_new);
        }

    }

    public EditorAnimaClipAtlas(AnimaCharactorType _type)
    {
        CharactorType = _type;
        FPS = 30;
        Frames = new List<EditorSpriteFrame>();
        Output = new List<FrameOutputData_Atlas>();
    }

    public AnimaClip ToAnimaClip()
    {
        AnimaClip ac = new AnimaClip();
        ac.FPS = FPS;
        ac.CharactorType = CharactorType;
        ac.Frames = new SpriteFrame[Frames.Count];
        for(int i = 0; i < Frames.Count; i++)
        {
            ac.Frames[i] = Frames[i].ToSpriteFrame();
        }

        if (Output.Count > 0)
        {
            ac.Event = new AnimaClipEvent[Output.Count];
            for (int i = 0; i < Output.Count; i++)
            {
                ac.Event[i] = Output[i].ToAnimaClipEvent();
            }
        }
        else { ac.Event = null; }



        return ac;
    }
}

[Serializable]
public class EditorSpriteFrame:IComparable<EditorSpriteFrame>,IEquatable<EditorSpriteFrame>
{
    public int index;
    public Sprite sprite;
    public int alpha;
    public Vector3 facePos;

    public EditorSpriteFrame() { }

    public EditorSpriteFrame(EditorSpriteFrame _esf)
    {
        index = _esf.index;
        sprite = _esf.sprite;
        alpha = _esf.alpha;
        facePos = _esf.facePos;
    }

    public SpriteFrame ToSpriteFrame()
    {
        SpriteFrame s = new SpriteFrame();
        s.index = index;s.sprite = sprite;s.alpha = alpha;s.facePos = facePos;
        return s;
    }

    public int CompareTo(EditorSpriteFrame _other)
    {
        if (_other == null)
            return 1;
        else
            return this.index.CompareTo(_other.index);
    }

    public bool Equals(EditorSpriteFrame _other)
    {
        if (_other == null)
            return false;
        else
        {
            bool _result = this.index.Equals(_other.index);
            if (_result) Debug.LogWarning("EditorSpriteFrame Equals true");
            return _result;
        }
            
    }

}



/// <summary>
/// 角色动画数据
/// </summary>
[Serializable]
public class AnimaDataCharactor
{
    public int ChID;//角色ID
    public AnimaClip[] Clips;
    public bool isFace;//是否外挂脸部
}

[Serializable]
public class AnimaDataClip
{
    public int ID;
    public AnimaClip Clip;
}


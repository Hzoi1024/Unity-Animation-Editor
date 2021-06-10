using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
/// <summary>
/// 角色动画的so
/// </summary>
[Serializable]
public class AnimaDataCharactorScritable : ScriptableObject, IComparable<AnimaDataCharactorScritable>, IEquatable<AnimaDataCharactorScritable>
{
    public int ChID;
    public List<EditorAnimaClipAtlas> Clips;
    public string ChName;
    public bool isFace;

    public AnimaDataCharactorScritable()
    {
        Clips = new List<EditorAnimaClipAtlas>();
    }

    public void Init(int _ID, string _name)
    {
        ChID = _ID;
        ChName = _name;
    }

    public AnimaDataCharactor ToAnimaDataCharactor()
    {
        AnimaDataCharactor result = new AnimaDataCharactor();
        result.ChID = ChID;
        result.Clips = new AnimaClip[Clips.Count];
        result.isFace = isFace;
        for (int i = 0; i < Clips.Count; i++)
        {
            result.Clips[i] = Clips[i].ToAnimaClip();
        }
        return result;
    }

    public int CompareTo(AnimaDataCharactorScritable _other)
    {
        if (_other == null)
            return 1;
        else
            return this.ChID.CompareTo(_other.ChID);
    }

    /*public override bool Equals(object other)
    {
        if (other == null) return false;
        AnimaDataCharactorScritable obj = other as AnimaDataCharactorScritable;
        if (obj == null) return false;
        return Equals(obj);
    }*/

    public bool Equals(AnimaDataCharactorScritable _other)
    {
        if (_other == null)
            return false;
        else
            return (this.ChID.Equals(_other.ChID));
    }

    public static List<EditorAnimaClipAtlas> GetClipsFromTexture(Texture2D _t2d)
    {
        //TextureImporter _t = new TextureImporter();

        string _path = AssetDatabase.GetAssetPath(_t2d);
        UnityEngine.Object[] _all = AssetDatabase.LoadAllAssetsAtPath(_path);
        EditorAnimaClipAtlas _atkClip = new EditorAnimaClipAtlas(AnimaCharactorType.Atk);
        EditorAnimaClipAtlas _idleClip = new EditorAnimaClipAtlas(AnimaCharactorType.Idle);
        EditorAnimaClipAtlas _walk = new EditorAnimaClipAtlas(AnimaCharactorType.Move);
        EditorAnimaClipAtlas _udrAtk = new EditorAnimaClipAtlas(AnimaCharactorType.UdrAtk);
        EditorAnimaClipAtlas _die = new EditorAnimaClipAtlas(AnimaCharactorType.Die);
        EditorAnimaClipAtlas _dead = new EditorAnimaClipAtlas(AnimaCharactorType.Dead);
        EditorAnimaClipAtlas _special = new EditorAnimaClipAtlas(AnimaCharactorType.Spl);

        for (int i = 0; i < _all.Length; i++)
        {
            Sprite _s = _all[i] as Sprite;
            if (_s != null)
            {
                string[] _strs = _s.name.Split('_');

                if (_strs.Length == 3)
                {
                    EditorSpriteFrame _esf = new EditorSpriteFrame();
                    _esf.alpha = 100;
                    _esf.index = Convert.ToInt32(_strs[2]);
                    Debug.Log(_s.name + ":" + _esf.index);
                    _esf.sprite = _s;
                    switch (_strs[1])
                    {
                        case "Atk":
                            _atkClip.Frames.Add(_esf);
                            break;
                        case "Idl":
                            _idleClip.Frames.Add(_esf);
                            break;
                        case "Mov":
                            _walk.Frames.Add(_esf);
                            break;
                        case "UdrAtk":
                            _udrAtk.Frames.Add(_esf);
                            break;
                        case "Die":
                            _die.Frames.Add(_esf);
                            break;
                        case "Dead":
                            _dead.Frames.Add(_esf);
                            break;
                        case "Spc":
                            _special.Frames.Add(_esf);
                            break;
                        default:
                            Debug.LogError("Unexpected middle name:" + _s.name);
                            break;
                    }
                }
                else
                {
                    Debug.LogError(_s.name + "命名中的下划线数量不为3个");
                }
            }
        }

        List<EditorAnimaClipAtlas> _result = new List<EditorAnimaClipAtlas>();
        if (_atkClip.Frames.Count > 0) { _atkClip.Frames.Sort(); _result.Add(_atkClip); }
        if (_idleClip.Frames.Count > 0) { _idleClip.Frames.Sort(); _result.Add(_idleClip); }
        if (_walk.Frames.Count > 0) { _walk.Frames.Sort(); _result.Add(_walk); }
        if (_udrAtk.Frames.Count > 0) { _udrAtk.Frames.Sort(); _result.Add(_udrAtk); }
        if (_die.Frames.Count > 0) { _die.Frames.Sort(); _result.Add(_die); }
        if (_dead.Frames.Count > 0) { _dead.Frames.Sort(); _result.Add(_dead); }
        if (_special.Frames.Count > 0) { _special.Frames.Sort(); _result.Add(_special); }

        if (_result.Count > 0) return _result;
        else
        {
            Debug.LogWarning("载入失败");
            return null;
        }
    }

    public static void SetClipsFacePos(Texture2D _t2d, List<EditorAnimaClipAtlas> _clips, Color _left, Color _right)
    {
        List<EditorAnimaClipAtlas> _faceClips = GetClipsFromTexture(_t2d);

        if (_faceClips.Count != _clips.Count)
        {
            Debug.LogError("face clips count not equal atlas clips count");
            return;
        }

        for (int i = 0; i < _faceClips.Count; i++)
        {
            if (_faceClips[i].CharactorType == _clips[i].CharactorType)
            {
                for (int k = 0; k < _faceClips[i].Frames.Count; k++)
                {
                    _clips[i].Frames[k].facePos = EditorSpriteFrame_Container.GetFacePos(_faceClips[i].Frames[k].sprite, _left, _right);

                }
            }
            else
            {
                Debug.LogWarning("faceClip and atlas clips CharactorType are not same");
            }
        }

        Debug.Log("SetClipsFacePos");
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


public class EditorDataBilnary:Editor
{
    
    [MenuItem("Tools/动画二进制生成")]
    public static void BundleEditorData()
    {
        List<AminaComponentClipData>  allClips = new List<AminaComponentClipData>();

        AminaComponentClipDataContainer container = new AminaComponentClipDataContainer();
        container.Dict = new Dictionary<int, AminaComponentClipData>();

        string[] _all = Directory.GetFiles(AminaEditorPath.FRAMEPATH, "*.asset", SearchOption.AllDirectories);
        for (int i = 0; i < _all.Length; i++)
        {
            EditorAminaComponentClip _new = AssetDatabase.LoadAssetAtPath<EditorAminaComponentClip>(_all[i]);
            if (_new != null)
            {
                //allClips.Add(EditorDataToClipData(_new));
                if (container.Dict.ContainsKey(_new.ID))
                {
                    Debug.LogWarning("repeat ID:" + _new.ID);
                    continue;
                }
                container.Dict.Add(_new.ID, EditorDataToClipData(_new));
            }
        }

        string path = Application.dataPath + "/Editor/AB_Res/Animations/";
        if (Directory.Exists(path)) Directory.Delete(path, true);
        Directory.CreateDirectory(path);

        IFormatter f = new BinaryFormatter();
        Stream s = new FileStream(path+ "ComponentClips.bytes", FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);
        f.Serialize(s, container);
        s.Close();
        Debug.LogWarning("anima data finished");
    }

    public static AminaComponentClipData EditorDataToClipData(EditorAminaComponentClip _eac)
    {
        AminaComponentClipData _new = new AminaComponentClipData();

        _new.ID = _eac.ID;
        _new.CompID = _eac.CompID;
        if(_eac.PauseTime!=null && _eac.PauseTime.Length == 2)
        {
            if (_eac.PauseTime[0] <= _eac.PauseTime[1] && _eac.PauseTime[1]< _eac.FramesCount)
            {
                _eac.PauseTime = new int[2] { _eac.PauseTime[0], _eac.PauseTime[1] };
            } else Debug.LogWarning("EditorAminaComponentClip pause time not right  id="+_eac.ID);
        }
        else
        {
            _eac.PauseTime = null;
        }

        int _maxFrames = _eac.Frames.Count - _eac.AdvanceEnd;
        _new.frames = new AminaFrame[_maxFrames];
        for (int i=0;i< _maxFrames; i++)
        {
            AminaFrame _af = new AminaFrame();
            _af.x = _eac.Frames[i].x;
            _af.y = _eac.Frames[i].y;
            _af.angle = _eac.Frames[i].angle;
            _new.frames[i] = _af;
        }

        if(_eac.OutData!=null && _eac.OutData.Count > 0)
        {
            _new.Event = new List<AnimaEventData>();

            for(int i=0;i< _eac.OutData.Count; i++)
            {
                AnimaEventData _aed = new AnimaEventData();
                _aed.Index = _eac.OutData[i].Index;
                _aed.Output = new List<int>();
                for(int j=0;j< _eac.OutData[i].Out.Count; j++)
                {
                    _aed.Output.Add((int)_eac.OutData[i].Out[j]);
                }
                _new.Event.Add(_aed);
            }
        }
        else
        {
            _new.Event = null;
        }

        return _new;
    }


}

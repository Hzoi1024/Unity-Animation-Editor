using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class FramLoadManager
{
    AminaComponentClipDataContainer container;

    private static FramLoadManager instance;
    public static FramLoadManager Instance
    {
        get
        {
            if (instance == null) instance = new FramLoadManager();
            return instance;
        }
    }

    private FramLoadManager() { LoadContainer(); }

    public AminaComponentClipData GetAminaFrame(int id)
    {
        AminaComponentClipData r;
        if(container.Dict.TryGetValue(id, out r))
        {
            return r;
        }
        Debug.LogWarning("not have the id,id=" + id);
        return null;
    }

    private void LoadContainer()
    {
        IFormatter f = new BinaryFormatter();
        TextAsset t = Resources.Load<TextAsset>("AnimationClips");
        Stream s = new MemoryStream(t.bytes);
        System.Object obj = f.Deserialize(s);
        container = obj as AminaComponentClipDataContainer;
    }

}

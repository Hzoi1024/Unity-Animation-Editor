using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 播放单位
/// </summary>
[Serializable]
public class AnimaActionData
{
    public int ID;
    public int ProtypeID;
    public int blank;//播放前的过渡帧
    public bool isLoop;
    AminaComponentClipData[] Clips;
}


/// <summary>
/// 存储单位
/// </summary>
[Serializable]
public class AminaComponentClipData
{
    public int ID;
    public int CompID;//部件id
    public int[] Pause;
    public List<AnimaEventData> Event;
    //public int blank;//第一个关键帧之前的空白帧数量 用于计算过渡
    public AminaFrame[] frames;
}

[Serializable]
public class AminaComponentClipDataContainer
{
    public Dictionary<int, AminaComponentClipData> Dict;
}

[Serializable]
public struct AnimaEventData
{
    public int Index;
    public List<int> Output;
}

[Serializable]
public class AminaFrame
{
    [SerializeField]
    public float x;
    public float y;
    public float angle;
}

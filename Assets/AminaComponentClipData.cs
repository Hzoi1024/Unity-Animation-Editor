using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 播放单位
/// </summary>
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
public class AminaComponentClipData
{
    public int ID;
    public int CompID;//部件id
    //public int blank;//第一个关键帧之前的空白帧数量 用于计算过渡
    public AminaFrame[] frames;
}

public class AminaFrame
{
    public Vector2 pos;
    public float angle;
}

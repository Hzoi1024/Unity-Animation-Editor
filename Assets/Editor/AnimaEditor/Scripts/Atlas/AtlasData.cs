using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



[Serializable]
public class AnimaClip
{
    public int FPS;
    public AnimaCharactorType CharactorType;
    public AnimaClipEvent[] Event;
    public SpriteFrame[] Frames;
}

public class AnimaClipEvent
{
    public int index;
    public int[] eventType;
}

[Serializable]
public class SpriteFrame
{
    public int index;
    public Sprite sprite;
    public int alpha;
    public Vector3 facePos;
}

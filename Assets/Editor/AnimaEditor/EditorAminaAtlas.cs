﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[CreateAssetMenu(fileName = "EditorAminaAtlas", menuName = "Custom/EditorAminaAtlas")]
public class EditorAminaAtlas : ScriptableObject
{
    public int ID;
    public SpriteFrame[] sprites;
    public int FPS;
}

[Serializable]
public class SpriteFrame
{
    public int index;
    public Sprite sprite;
    public float alpha;
}

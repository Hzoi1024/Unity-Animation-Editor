using System.Collections;
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



using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AminaEditorConfig",menuName = "Custom/AminaEditorConfig")]
public class AminaEditorConfigSO : ScriptableObject
{
    public Texture2D White;
    public Texture2D Blue;
    public Texture2D PlayTex;
    public Texture2D PlayingTex;

    public GameObject Humanoid;


}

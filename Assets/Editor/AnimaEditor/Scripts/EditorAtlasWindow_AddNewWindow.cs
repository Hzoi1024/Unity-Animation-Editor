using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorAtlasWindow_AddNewWindow : EditorWindow
{
    EditorAtlasWindow atlasWindow;

    int UIHeight=20;
    int border=10;
    int buttonWidth = 80;
    int atlasArea = 70;

    int newID;
    string newName;
    Texture2D atlas;
    Texture2D newAtlas;
    Texture2D oldFaceCorrectedTex;
    Texture2D faceCorrectedTex;
    Color leftFaceColor;
    Color rightFaceColor;

    bool isFace;

    List<EditorAnimaClipAtlas> _clips;

    public void Init(EditorAtlasWindow _atlasWindow,int _newID)
    {
        atlasWindow = _atlasWindow;
        minSize = new Vector2(250, 230);
        newID = _newID;
        newName = "New Charactor";
        atlas = null;
        newAtlas = null;
        oldFaceCorrectedTex = null;
        faceCorrectedTex = null;
        _clips = null;
        isFace = false;

        leftFaceColor = new Color(1, 0, 1, 1);
        rightFaceColor = new Color(0, 1, 1, 1);

    }

    private void OnGUI()
    {
        int uiY = 5;
        int labelWidth = 40;
        uiY += border;
        GUI.Label(new Rect(5, uiY, labelWidth, UIHeight), "ID");
        int textX = 5 + labelWidth + 10;
        newID = EditorGUI.IntField(new Rect(textX, uiY, 60, UIHeight), newID);

        uiY += (border + UIHeight);
        GUI.Label(new Rect(5, uiY, labelWidth, UIHeight), "Name");
        newName = GUI.TextField(new Rect(textX, uiY, position.width - (textX + 10), UIHeight), newName);

        uiY += (border + UIHeight);

        newAtlas = (Texture2D)EditorGUI.ObjectField(new Rect(position.width / 2 - atlasArea / 2, uiY, atlasArea, atlasArea), atlas, typeof(Texture2D),false);
        if (newAtlas != atlas && newAtlas!=null)
        {
            _clips = AnimaDataCharactorScritable.GetClipsFromTexture(newAtlas);
            newName = newAtlas.name;
        }
        atlas = newAtlas;

        uiY += (border + atlasArea + 10);

        if (atlas != null)
        {
            isFace = EditorGUI.ToggleLeft(new Rect(5, uiY, 60, UIHeight), "isFace", isFace);

            if (isFace)
            {
                minSize = new Vector2(250, 330);
                uiY += (border + UIHeight);
                leftFaceColor = EditorGUI.ColorField(new Rect(25, uiY, position.width - 50, UIHeight), "Left", leftFaceColor);
                uiY += (border + UIHeight);
                rightFaceColor = EditorGUI.ColorField(new Rect(25, uiY, position.width - 50, UIHeight), "Right", rightFaceColor);
                uiY += (border + UIHeight);

                faceCorrectedTex = (Texture2D)EditorGUI.ObjectField(new Rect(80, uiY+1, 120, 18), oldFaceCorrectedTex, typeof(Texture2D), false);

                if (faceCorrectedTex != oldFaceCorrectedTex && faceCorrectedTex != null)
                {
                    AnimaDataCharactorScritable.SetClipsFacePos(faceCorrectedTex, _clips, leftFaceColor, rightFaceColor);
                }
                oldFaceCorrectedTex = faceCorrectedTex;
            }

            uiY += (border + UIHeight);

        }
        else
        {
            isFace = false;
        }


        if (GUI.Button(new Rect(position.width/2-buttonWidth/2, uiY, buttonWidth, UIHeight), "Create"))
        {
            AnimaDataCharactorScritable _new = ScriptableObject.CreateInstance<AnimaDataCharactorScritable>();
            _new.Init(newID, newName);
            if (_clips != null)
            {
                _new.Clips = _clips;
            }
            _new.isFace = isFace;
            AssetDatabase.CreateAsset(_new, AminaEditorPath.ATLASCHARACTORPATH + newID.ToString() + " " + newName + ".asset");

            atlasWindow.LoadConfig(EditorAtlasWindowLoadType.Charactor);
            Close();
        }
        /*
        if (GUI.Button(new Rect(position.width*3 / 4 - wideBtn / 2, uiY, wideBtn, UIHeight), "Load Packer"))
        {

        }*/
    }

    private void OnDestroy()
    {
        Debug.Log("EditorAtlasWindow_AddNewWindow close");
        atlasWindow.isSonWindow = false;
        atlasWindow.Focus();
    }
}

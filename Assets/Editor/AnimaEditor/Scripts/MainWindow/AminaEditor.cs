using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Timers;
using System;

public class AminaEditorPath
{
    public static readonly string CLIPPATH= "Assets/Editor/AnimaEditor/Res/Clips/";
    public static readonly string FRAMEPATH = "Assets/Editor/AnimaEditor/Res/Frames/";
    public static readonly string ATLASPATH = "Assets/Editor/AnimaEditor/Res/Atlas/";

    public static readonly string ATLASCHARACTORPATH = "Assets/Editor/AnimaEditor/Res/Atlas/Charactor/";
    public static readonly string ATLASANIMA = "Assets/Editor/AnimaEditor/Res/Atlas/Anima/";
}

public enum OriginAminType
{
    Defualt = 0,
    Human = 1,
    SpriteFrames=2,
}

public partial class AminaEditor : EditorWindow
{
    [MenuItem("Tools/动画编辑器")]
    public static void ShowWindow()
    {
        //EditorWindow window = new EditorWindow();
        AminaEditor aminaEditor = GetWindow<AminaEditor>(false);
        aminaEditor.minSize = new Vector2(40, 10);
        //aminaEditor.maxSize = new Vector2(1024, 900);
        aminaEditor.Show();
        aminaEditor.titleContent = new GUIContent("动画编辑器");

    }

    public AminaEditor_LoadFrames aminaEditor_LoadFrames;
    public AminaEditor_SaveFrames aminaEditor_SaveFrames;
    public EditorAnimaClip clip;
    public AminaEditorConfigSO scriptableObject;
    //EditorAminaComponentClip componentClip;
    public List<AminaEditorComponetData> Components;
    //public AminaEditorAtlas atlas;
    bool isChScript;
    public AnimaCharactorType AnimaChType;
    public EditorAnimaClipAtlas_Container atlas;
    public AnimaDataCharactorScritable_Container chScript;
    

    int componentParaCount;
    int atlasParaCount;

    List<CopyKeyData> copies;
    AminaEditorUndo editorUndo;

    GameObject protype;
    Vector2 rightScrollPos;
    Vector2 leftScrollPos;
    public Vector2 scrollPosition = Vector2.zero;

    Rect LeftRootBlock;//编辑器左边部分的整体划分
    Rect RightRootBlock;

    Rect aminaTopBlock;
    Rect frameEditorBlock;

    Rect LeftComponentEditBlock;

    float LeftRootBlockWidth = 400;
    Vector2 scrollView = Vector2.zero;

    float MiddleControlBlockWidth;

    bool isDragging;
    int draggingControl;

    //bool isDraggingMiddleControl = false;
    //bool isDraggingaminaTopBlock = false;//拖拽播放红轴
    int outPutOffset;
    Vector2Int outPutSelect;
    FrameOutputDataEditorContainer outputDataEditorContainer;
    SpriteFrameContainer spriteFrameContainer;

    //bool isDraggingKeyFrames = false;//拖拽关键帧
    int draggingKeyFramesBase;
    int draggingKeyFramesOffset;

    //bool isDraggingSelectingRect = false;
    Vector2Int SelectRectStart;
    Vector2Int SelectRectEnd;
    Rect SelectRect;

    bool isShowWindow;
    int WindowControl;

    float conponentClipBoxHeight = 80;//部件动画片段编辑box高度

    float topBlockHeight;
    float frameBlockWidth;//动画编辑部分一格的宽度

    float keySqureRadius=5;//帧方块半径

    int currentAminaPointerIndex = -1; //红帧指向

    bool isPlaying;
    GUIStyle NilStyle;
    GUIStyle LabelStyle;
    GUIStyle AtlasAlphaInputNumStyle;//alpha值输入框

    GUIContent playTex;
    GUIContent PlayingTex;
    GUIContent buttonTex;
    GUIContent buttonATex;

    int maxFrameNum;
    float realMaxFrameNum;

    int[] tempPauseTime;
    int pauseComponetIndex;
    bool isDrawingPause;

    float frameTime = 1f / 30f;
    float deltaTime;
    float playTime;
    double lastStartUpTime;
    bool isAtlasDragSprites;
    List<Sprite> AtlasDragSprites;


    OriginAminType originDisplay;

    private void Update()
    {
        if (isPlaying)
        {
            deltaTime = (float)(EditorApplication.timeSinceStartup - lastStartUpTime);
            lastStartUpTime= EditorApplication.timeSinceStartup;

            playTime += deltaTime;

            if (playTime > frameTime)
            {
                currentAminaPointerIndex++;
                if (currentAminaPointerIndex > realMaxFrameNum)
                {
                    currentAminaPointerIndex = 1;
                }
                RefreshProtypeAmina(currentAminaPointerIndex - 1);

                playTime = 0;
                Repaint();
            }
        }
    }
    /*
    private void OnInspectorUpdate()
    {
        if (atlas != null && atlas.Examin())
        {
            Repaint();
            GetRightBlockFrameMaxNumber();
        }
    }*/

    void OnEnable()
    {
        //scriptableObject = AssetDatabase.LoadAssetAtPath<AminaEditorConfigSO>("Assets/Editor/AnimaEditor" + "/AminaEditorConfig.asset");
        //scriptableObject = EditorGUIUtility.Load("AminaEditorConfig.asset") as AminaEditorConfigSO;
        scriptableObject = AssetDatabase.LoadAssetAtPath<AminaEditorConfigSO>("Assets/Editor/AnimaEditor/Res/AminaEditorConfig.asset");
        originDisplay = OriginAminType.Defualt;
        //isDraggingaminaTopBlock = false;
        //isDraggingKeyFrames = false;
        //isDraggingSelectingRect = false;
        lastStartUpTime = EditorApplication.timeSinceStartup;
        frameTime = 1f / 30f;
        playTex = new GUIContent(scriptableObject.PlayTex);
        PlayingTex = new GUIContent(scriptableObject.PlayingTex);
        buttonTex = new GUIContent(scriptableObject.Button);
        buttonATex = new GUIContent(scriptableObject.ButtonActive);

        isPlaying = false;
        NilStyle = new GUIStyle();
        NilStyle.alignment = TextAnchor.MiddleCenter;

        //NilStyle.border = new RectOffset(1,1,1,1);
        Debug.Log("onenable");

        GetRightBlockFrameMaxNumber();
        isShowWindow = false;
        editorUndo = new AminaEditorUndo(this);
        LabelStyle = new GUIStyle();
        LabelStyle.alignment = TextAnchor.MiddleLeft;
        LabelStyle.clipping = TextClipping.Clip;
        isDragging = false;
        isDrawingPause = false;
        pauseComponetIndex = -1;

        outPutSelect = new Vector2Int(-1, -1);
        outPutOffset = 0;
        spriteFrameContainer = ScriptableObject.CreateInstance<SpriteFrameContainer>();
        outputDataEditorContainer = ScriptableObject.CreateInstance<FrameOutputDataEditorContainer>();
        clip = ScriptableObject.CreateInstance<EditorAnimaClip>();
        isAtlasDragSprites = false;
        
    }
    

    


    Rect TrigonoInputWindowRect = new Rect(600, 100, 200, 200);
    Rect TrigonoInputWindow_EnterButton = new Rect(20, 100, 160, 30);
    Rect TrigonoInputWindow_ExitButton = new Rect(20, 150, 160, 30);
    Vector3Int TrigonoInputWindowIndex;
    //float SineInput_delDis=0;
    float SineInput_startAngle = 0;
    float SineInput_endAngl = 0;
    int AdvanceEndInput = 0;

    void SineWindow(int unusedWindowID)
    {
        //SineInput_delDis = EditorGUILayout.FloatField("Delta Distance:", SineInput_delDis);
        SineInput_startAngle = EditorGUILayout.FloatField("Start Angle:", SineInput_startAngle);
        SineInput_endAngl = EditorGUILayout.FloatField("End Angle:", SineInput_endAngl);

        if (GUI.Button(TrigonoInputWindow_EnterButton, "Add Key"))
        {
            Debug.Log(" _startAngle=" + SineInput_startAngle + " _endAngle=" + SineInput_endAngl);

            SineKeyFillPara _newSinePara = new SineKeyFillPara(SineInput_startAngle, SineInput_endAngl);
            AddkeySine(TrigonoInputWindowIndex, _newSinePara);
            isShowWindow = false;
        }
        if(GUI.Button(TrigonoInputWindow_ExitButton, "Cancel"))
        {
            isShowWindow = false;
        }
        GUI.DragWindow();
    }

    void CosineWindow(int unusedWindowID)
    {

        //SineInput_delDis = EditorGUILayout.FloatField("Delta Distance:", SineInput_delDis);
        SineInput_startAngle = EditorGUILayout.FloatField("Start Angle:", SineInput_startAngle);
        SineInput_endAngl = EditorGUILayout.FloatField("End Angle:", SineInput_endAngl);

        if (GUI.Button(TrigonoInputWindow_EnterButton, "Add Key"))
        {
            Debug.Log(" _startAngle=" + SineInput_startAngle + " _endAngle=" + SineInput_endAngl);

            SineKeyFillPara _newSinePara = new SineKeyFillPara(SineInput_startAngle, SineInput_endAngl);
            AddkeyCosine(TrigonoInputWindowIndex, _newSinePara);
            isShowWindow = false;
        }
        if (GUI.Button(TrigonoInputWindow_ExitButton, "Cancel"))
        {
            isShowWindow = false;
        }
        GUI.DragWindow();
    }

    void SetAdvanceEndWindow(int unusedWindowID)
    {
        //SineInput_delDis = EditorGUILayout.FloatField("Delta Distance:", SineInput_delDis);
        AdvanceEndInput = EditorGUILayout.IntField("Start Angle:", AdvanceEndInput);

        if (GUI.Button(TrigonoInputWindow_EnterButton, "Set"))
        {
            AdvanceEndInput = Mathf.Max(0, AdvanceEndInput);

            for (int i = 0; i < Components.Count; i++)
            {
                if (Components[i].clip.FramesCount > 0)
                {
                    Components[i].clip.AdvanceEnd = AdvanceEndInput;
                }
            }

            GetRightBlockFrameMaxNumber();

            isShowWindow = false;
        }
        if (GUI.Button(TrigonoInputWindow_ExitButton, "Cancel"))
        {
            isShowWindow = false;
        }
        GUI.DragWindow();
    }

    public void CallSinKeyWindow(object k)
    {
        WindowControl = 1;
        TrigonoInputWindowIndex = (Vector3Int)k;
        isShowWindow = true;
    }

    public void CallCosKeyWindow(object k)
    {
        WindowControl = 2;
        TrigonoInputWindowIndex = (Vector3Int)k;
        isShowWindow = true;
    }

    public void CallSetAdvanceEndWindow()
    {
        WindowControl = 3;
        isShowWindow = true;
    }

    public void AddkeySine(Vector3Int _v3,SineKeyFillPara _sineFill)
    {
        Debug.Log("AddkeySine");
        editorUndo.Addkey(_v3, KeyType.SineKey, _sineFill);
        GetRightBlockFrameMaxNumber();
    }

    public void AddkeyCosine(Vector3Int _v3, SineKeyFillPara _sineFill)
    {
        Debug.Log("AddkeyCosine");
        editorUndo.Addkey(_v3, KeyType.CosineKey, _sineFill);
        GetRightBlockFrameMaxNumber();
    }

    public void Addkey(object k)
    {
        Vector3Int v3 = (Vector3Int)k;
        editorUndo.Addkey(v3,KeyType.LinearKey,null);
        GetRightBlockFrameMaxNumber();
    }

    public void AddkeyAccelerate(object k)
    {
        Vector3Int v3 = (Vector3Int)k;
        /*Transform _t = Components[v2.x].ComponentGO.transform;
        Vector2 _pos = _t.localPosition;
        float _ang = TransformUtils.GetInspectorRotation(_t).z;

        Components[v2.x].clip.AddKeyFrameAcceleration(_pos, _ang, v2.y);*/
        editorUndo.Addkey(v3, KeyType.AccelerateKey,null);
        GetRightBlockFrameMaxNumber();
    }

    public void AddkeyTrigono(object k)
    {
        Vector3Int v3 = (Vector3Int)k;
        editorUndo.Addkey(v3, KeyType.TrigonoKey,null);
        GetRightBlockFrameMaxNumber();
    }

    public void AddkeyDecelerate(object k)
    {
        Vector3Int v3 = (Vector3Int)k;
        editorUndo.Addkey(v3, KeyType.DecelerateKey, null);
        GetRightBlockFrameMaxNumber();
    }

    public void DeleteKey(object k)
    {
        Vector3Int v3 = (Vector3Int)k;
        editorUndo.DeleteKey(v3);
        GetRightBlockFrameMaxNumber();
    }

    public void AddPauseOrder(object k)
    {
        Vector3Int v3 = (Vector3Int)k;
        pauseComponetIndex = v3.x;
        tempPauseTime = new int[2] { v3.y, v3.y };
        isDrawingPause = true;
        DraggingOn(5);
    }

    public void AddPause(int _end)
    {
        //int[] _pause = new int[2] { tempPauseTime, _end-1 };
        if(tempPauseTime[0]<Components[pauseComponetIndex].clip.FramesCount&& tempPauseTime[1] < Components[pauseComponetIndex].clip.FramesCount)
        {
            editorUndo.AddPauseTime(pauseComponetIndex, tempPauseTime);
        }
        isDrawingPause = false;
        pauseComponetIndex = -1;
    }

    public void DelPause(object k)
    {
        int _compIndex = (int)k;
        //int[] _pause = new int[2] { tempPauseTime, _end-1 };
        editorUndo.AddPauseTime(_compIndex, null);
    }

    public void AddOut(object k)
    {
        Vector3Int v3 = (Vector3Int)k;
        Components[v3.x].clip.AddOutput(v3.y);
        Repaint();
    }

    public void DelOut(object k)
    {
        Vector3Int v3 = (Vector3Int)k;
        Components[v3.x].clip.DeleteOutput(v3.y);
        Repaint();
    }

    public void MoveOutput(int _comp,int _oldIndex,int _newIndex)
    {
        Components[_comp].clip.MoveOutput(_oldIndex, _newIndex);

    }

    public void CopyKey()
    {
        if (Components.Count > 0)
        {
            List<CopyKeyData> _cs = new List<CopyKeyData>(); ;

            int _offset = -10;

            for(int i=0;i< Components.Count; i++)
            {
                for (int j = 0; j < Components[i].SelectionKeyFrame.Count; j++)
                {
                    SelectedKeyData _skd = Components[i].SelectionKeyFrame[j];
                    int k = _skd.Index;

                    if (_offset == -10) { _offset = k; } else { _offset = Mathf.Min(k, _offset); }

                    if (_skd.x)
                    {
                        CopyKeyData _ck = new CopyKeyData();
                        _ck.componentIndex = i;
                        _ck.offset = k;
                        _ck.para = Components[i].clip.Frames[k].x;
                        _ck.paraType = FrameParaType.x;
                        _ck.type = Components[i].clip.Frames[k].XKey;
                        _ck.fillPara = Components[i].clip.Frames[k].XfillPara;
                        _cs.Add(_ck);
                    }
                    if (_skd.y)
                    {
                        CopyKeyData _ck = new CopyKeyData();
                        _ck.componentIndex = i;
                        _ck.offset = k;
                        _ck.para = Components[i].clip.Frames[k].y;
                        _ck.paraType = FrameParaType.y;
                        _ck.type = Components[i].clip.Frames[k].YKey;
                        _ck.fillPara = Components[i].clip.Frames[k].YfillPara;
                        _cs.Add(_ck);
                    }
                    if (_skd.a)
                    {
                        CopyKeyData _ck = new CopyKeyData();
                        _ck.componentIndex = i;
                        _ck.offset = k;
                        _ck.para = Components[i].clip.Frames[k].angle;
                        _ck.paraType = FrameParaType.a;
                        _ck.type = Components[i].clip.Frames[k].AKey;
                        _ck.fillPara = Components[i].clip.Frames[k].AfillPara;
                        _cs.Add(_ck);
                    }



                }
            }

            if (_cs.Count > 0)
            {
                copies = _cs;

                foreach(CopyKeyData _c in copies)
                {
                    _c.offset -= _offset;
                }
            }
        }

        
    }

    void PasteKey()
    {
        if (copies.Count > 0)
        {
            editorUndo.PasteKey(currentAminaPointerIndex-1, copies);
        }
        GetRightBlockFrameMaxNumber();
    }

    void MoveSelectedKey(int _deltaIndex)
    {
        editorUndo.MoveSelectedKey(_deltaIndex);
        GetRightBlockFrameMaxNumber();
    }

    void MoveSelectedKey_Atlas(int _deltaIndex)
    {
        //editorUndo.MoveSelectedKey(_deltaIndex);
        //Debug.LogWarning("移动" + _deltaIndex + "帧");
        editorUndo.MoveSelectedKey_Atlas(_deltaIndex);
        GetRightBlockFrameMaxNumber();
    }


    void DeleteSelectedkey()
    {
        if(Components.Count!=0) editorUndo.DeleteSelected();
        GetRightBlockFrameMaxNumber();
    }

    void AddOneWholeKey()
    {
        if (Components != null)
        {
            editorUndo.AddWholeKey(currentAminaPointerIndex - 1);
            GetRightBlockFrameMaxNumber();
        }
    }

    void ProtypeRecycle()
    {
        Components = new List<AminaEditorComponetData>();
        
        if (protype != null)
        {
            DestroyImmediate(protype);
            protype = null;
            clip = null;
        }
    }

    void ChangePlayBool()
    {
        isPlaying = !isPlaying;
        Repaint();
    }

    void ProtypeInit(OriginAminType protypeType)
    {
        ProtypeRecycle();
        copies = new List<CopyKeyData>();
        ClearSelectKey();
        switch (protypeType)
        {
            case OriginAminType.Human:
                protype = Instantiate(scriptableObject.Humanoid,Vector3.zero,Quaternion.identity,Camera.main.transform);
                //if(protype!=null) head = protype.transform.Find("SP/Head").gameObject;
                AminaEditorComponetData _head = new AminaEditorComponetData(1,protype);
                AminaEditorComponetData _body = new AminaEditorComponetData(2, protype);
                AminaEditorComponetData _fHand = new AminaEditorComponetData(3, protype);
                AminaEditorComponetData _bHand = new AminaEditorComponetData(4, protype);
                AminaEditorComponetData _fLeg = new AminaEditorComponetData(5, protype);
                AminaEditorComponetData _bLeg = new AminaEditorComponetData(6, protype);
                Components.Add(_head);
                Components.Add(_body);
                Components.Add(_fHand);
                Components.Add(_bHand);
                Components.Add(_fLeg);
                Components.Add(_bLeg);
                atlas = null;
                break;
            case OriginAminType.SpriteFrames:
                protype = new GameObject("SpriteFrame");
                protype.transform.position = Vector3.zero;
                protype.AddComponent<SpriteRenderer>();
                GameObject face = GameObject.Instantiate(protype,protype.transform);
                face.name = "face";
                face.transform.SetParent(protype.transform);
                //SpriteRenderer _sr = protype.AddComponent<SpriteRenderer>();
                isChScript = false;
                atlas = new EditorAnimaClipAtlas_Container(this,protype,face);

                break;
            default:
                atlas = null;
                break;
        }

        GetRightBlockFrameMaxNumber();
    }

    void DrawLeftComponent()
    {
        conponentClipBoxHeight = 30;
        float labelWidth = 20;
        componentParaCount = 0;
        if (Components != null)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                float _col = (float)(0.9 - 0.06 * ((i+ componentParaCount) % 2));

                EditorGUI.DrawRect(new Rect(0, topBlockHeight + conponentClipBoxHeight * (i+ componentParaCount), LeftRootBlockWidth, conponentClipBoxHeight), new Color(_col, _col, _col));
                
                Components[i].showComponentPara = EditorGUI.Foldout(new Rect(20, topBlockHeight + conponentClipBoxHeight * (i + componentParaCount), labelWidth, conponentClipBoxHeight), Components[i].showComponentPara, Components[i].labelStr);

                if (GUI.Button(new Rect(LeftRootBlockWidth - 30, topBlockHeight + conponentClipBoxHeight * (i + componentParaCount) + 4, conponentClipBoxHeight - 8, conponentClipBoxHeight - 8), "S"))
                {
                    //System.IO.FileInfo
                    EditorAminaComponentClip _saveEAC = ScriptableObject.CreateInstance<EditorAminaComponentClip>();
                    _saveEAC.Init(Components[i].clip);
                    if (aminaEditor_SaveFrames == null)
                    {
                        aminaEditor_SaveFrames = GetWindow<AminaEditor_SaveFrames>(false, "Save Frames", true);
                        aminaEditor_SaveFrames.minSize = new Vector2(250, 250);
                        aminaEditor_SaveFrames.position = new Rect(position.x + LeftRootBlock.width, position.y, 250, 250);
                        
                    }
                    aminaEditor_SaveFrames.Init(this, _saveEAC);

                    //AssetDatabase.CreateAsset(_saveEAC, AminaEditorPath.FRAMEPATH+i.ToString()+"jiba.asset");
                }

                //Components[i].clip =(EditorAminaComponentClip)EditorGUI.ObjectField(new Rect(LeftRootBlockWidth - 200, topBlockHeight + conponentClipBoxHeight * (i + componentParaCount) + 4, 100, conponentClipBoxHeight - 8), Components[i].clip, typeof(EditorAminaComponentClip), false);

                if (GUI.Button(new Rect(LeftRootBlockWidth - 60, topBlockHeight + conponentClipBoxHeight * (i + componentParaCount) + 4, conponentClipBoxHeight - 8, conponentClipBoxHeight - 8), "L"))
                {
                    if (aminaEditor_LoadFrames == null)
                    {
                        aminaEditor_LoadFrames = GetWindow<AminaEditor_LoadFrames>(false, Components[i].componentName+ " Load Frames",true);
                        aminaEditor_LoadFrames.minSize = new Vector2(250, 250);
                        aminaEditor_LoadFrames.position = new Rect(position.x+LeftRootBlock.width, position.y, 250, 250);
                    }
                    aminaEditor_LoadFrames.Init(this, Components[i].componentID);
                }

                if (Components[i].showComponentPara)
                {
                    componentParaCount ++;
                    _col = (float)(0.9 - 0.06 * ((i + componentParaCount) % 2));
                    EditorGUI.DrawRect(new Rect(0, topBlockHeight + conponentClipBoxHeight * (i + componentParaCount), LeftRootBlockWidth, conponentClipBoxHeight), new Color(_col, _col, _col));
                    GUI.Label(new Rect(40, topBlockHeight + conponentClipBoxHeight * (i + componentParaCount), 200, conponentClipBoxHeight), Components[i].componentName+ ".vector.x", LabelStyle);

                    componentParaCount++;
                    _col = (float)(0.9 - 0.06 * ((i + componentParaCount) % 2));
                    EditorGUI.DrawRect(new Rect(0, topBlockHeight + conponentClipBoxHeight * (i + componentParaCount), LeftRootBlockWidth, conponentClipBoxHeight), new Color(_col, _col, _col));
                    GUI.Label(new Rect(40, topBlockHeight + conponentClipBoxHeight * (i + componentParaCount), 200, conponentClipBoxHeight), Components[i].componentName + ".vector.y", LabelStyle);

                    componentParaCount++;
                    _col = (float)(0.9 - 0.06 * ((i + componentParaCount) % 2));
                    EditorGUI.DrawRect(new Rect(0, topBlockHeight + conponentClipBoxHeight * (i + componentParaCount), LeftRootBlockWidth, conponentClipBoxHeight), new Color(_col , _col, _col));
                    GUI.Label(new Rect(40, topBlockHeight + conponentClipBoxHeight * (i + componentParaCount), 200, conponentClipBoxHeight), Components[i].componentName + ".angle", LabelStyle);
                }

            }
        }

        if (atlas != null)
        {
            int t = 0;

            if (Components != null)
            {
                t = Components.Count + componentParaCount;
            }
            float _col = (float)(0.9 - 0.06 * (t % 2));

            atlasParaCount = 0;
            EditorGUI.DrawRect(new Rect(0, topBlockHeight + conponentClipBoxHeight * (t + atlasParaCount), LeftRootBlockWidth, conponentClipBoxHeight), new Color(_col, _col, _col));

            if (GUI.Button(new Rect(LeftRootBlockWidth - 60, topBlockHeight + conponentClipBoxHeight * (t + atlasParaCount) + 4, conponentClipBoxHeight - 8, conponentClipBoxHeight - 8), "L"))
            {
                AminaEditor_LoadAtlas _AminaEditor_LoadAtlas = GetWindow<AminaEditor_LoadAtlas>(false, "Load Atlas", true);
                _AminaEditor_LoadAtlas.minSize = new Vector2(250, 250);
                _AminaEditor_LoadAtlas.position = new Rect(position.x + LeftRootBlock.width, position.y, 250, 250);
                _AminaEditor_LoadAtlas.Init(this);

            }
            atlas.showAtlasPara = EditorGUI.Foldout(new Rect(20, topBlockHeight + conponentClipBoxHeight 
                * (t + atlasParaCount), labelWidth, conponentClipBoxHeight), atlas.showAtlasPara, atlas.AtlasName);
            if (atlas.showAtlasPara)
            {
                int _alphaNum = 0;
                Vector3 _faceVec = Vector3.zero;

                bool _isKeyFace = false;

                AtlasAlphaInputNumStyle = new GUIStyle(GUI.skin.textField);
                AtlasAlphaInputNumStyle.normal.textColor = Color.gray;
                if (currentAminaPointerIndex - 1 < atlas.Frames.Count && currentAminaPointerIndex>0)
                {
                    _alphaNum = atlas.Frames[currentAminaPointerIndex - 1].Alpha;

                    if (atlas.Frames[currentAminaPointerIndex - 1].isKeyAlpha)
                        AtlasAlphaInputNumStyle.normal.textColor = Color.black;

                    _faceVec = atlas.Frames[currentAminaPointerIndex - 1].facePos;

                    _isKeyFace = atlas.Frames[currentAminaPointerIndex - 1].isKeyFace;
                }

                atlasParaCount++;
                _col = (float)(0.9 - 0.06 * ((t + atlasParaCount) % 2));
                EditorGUI.DrawRect(new Rect(0, topBlockHeight + conponentClipBoxHeight * (t + atlasParaCount), LeftRootBlockWidth, conponentClipBoxHeight), new Color(_col, _col, _col));
                GUI.Label(new Rect(40, topBlockHeight + conponentClipBoxHeight * (t + atlasParaCount), 200, conponentClipBoxHeight), atlas.AtlasName + ".sprite", LabelStyle);

                atlasParaCount++;
                _col = (float)(0.9 - 0.06 * ((t + atlasParaCount) % 2));
                EditorGUI.DrawRect(new Rect(0, topBlockHeight + conponentClipBoxHeight * (t + atlasParaCount), LeftRootBlockWidth, conponentClipBoxHeight), new Color(_col, _col, _col));
                GUI.Label(new Rect(40, topBlockHeight + conponentClipBoxHeight * (t + atlasParaCount), 200, conponentClipBoxHeight), atlas.AtlasName + ".alpha", LabelStyle);
                
                int _inputAlpha = EditorGUI.DelayedIntField(new Rect(140, topBlockHeight + conponentClipBoxHeight * (t + atlasParaCount)+5, 40, conponentClipBoxHeight-10), _alphaNum, AtlasAlphaInputNumStyle);
                
                if (_inputAlpha!= _alphaNum)
                {
                    editorUndo.AddAtlasAlphaKey(currentAminaPointerIndex - 1, _inputAlpha);
                }

                atlasParaCount++;
                _col = (float)(0.9 - 0.06 * ((t + atlasParaCount) % 2));
                EditorGUI.DrawRect(new Rect(0, topBlockHeight + conponentClipBoxHeight * (t + atlasParaCount), LeftRootBlockWidth, conponentClipBoxHeight), new Color(_col, _col, _col));
                GUI.Label(new Rect(40, topBlockHeight + conponentClipBoxHeight * (t + atlasParaCount), 200, conponentClipBoxHeight), atlas.AtlasName + ".face", LabelStyle);

                /*
                GUIStyle _textStl = GUI.skin.textField;
                Color _colBuffer = _textStl.normal.textColor;

                if (_isKeyFace) _textStl.normal.textColor = Color.black;
                else _textStl.normal.textColor = Color.gray;

                Vector3 _newFaceVec = EditorGUI.Vector3Field(new Rect(140, topBlockHeight + conponentClipBoxHeight * (t + atlasParaCount) + 5, 200, conponentClipBoxHeight - 10), "", _faceVec);

                _textStl.normal.textColor = _colBuffer;*/

                string _faceStr = "x:" + _faceVec.x.ToString("f3") + " y:" + _faceVec.y.ToString("f3") + " a:" + _faceVec.z.ToString("f3");

                GUI.Label(new Rect(140, topBlockHeight + conponentClipBoxHeight * (t + atlasParaCount) + 5, 250, conponentClipBoxHeight - 10), _faceStr);

            }

        }

    }

    void DrawRightComponent()
    {
        if (Components != null)
        {
            for (int i = 0; i < Components.Count+  componentParaCount; i++)
            {
                float _col = (float)(0.9 - 0.06 * (i % 2));

                EditorGUI.DrawRect(new Rect(RightRootBlock.x, topBlockHeight + conponentClipBoxHeight *i, RightRootBlock.width, conponentClipBoxHeight), new Color(_col, _col, _col));
                //GUI.Box(new Rect(0, topBlockHeight, LeftRootBlockWidth, conponentClipBoxHeight), new GUIContent("11111111obj", scriptableObject.HeadSprite));
                
                //GUI.Label(new Rect(20, topBlockHeight + conponentClipBoxHeight * i, labelWidth, conponentClipBoxHeight), Components[i].labelStr);


            }
        }

        if(atlas != null)
        {
            int t = 0;
            if (Components != null)
            {
                t = Components.Count+ componentParaCount;
            }

            for(int i = 0; i < atlasParaCount+1; i++)
            {
                float _col = (float)(0.9 - 0.06 * ((t+i) % 2));

                EditorGUI.DrawRect(new Rect(RightRootBlock.x, topBlockHeight + conponentClipBoxHeight * (t+i), RightRootBlock.width, conponentClipBoxHeight), new Color(_col, _col, _col));
            }


        }

    }

    void DrawKeyFrame()
    {
        keySqureRadius = 5;
        int _verticalIndex = 0;

        if (Components != null)
        {

            for (int i = 0; i < Components.Count; i++)
            {
                float _dY = topBlockHeight + conponentClipBoxHeight * (_verticalIndex + 0.5f);

                bool _isShow = Components[i].showComponentPara;

                if (isDrawingPause&&i== pauseComponetIndex)
                {
                    int _l,_r;
                    if(tempPauseTime[0]<= tempPauseTime[1])
                    {
                        _l = tempPauseTime[0]; _r = tempPauseTime[1];
                    }
                    else
                    {
                        _l = tempPauseTime[1]; _r = tempPauseTime[0];
                    }

                    EditorGUI.DrawRect(new Rect(RightRootBlock.x + frameBlockWidth * (_l + 1) - 8, topBlockHeight + conponentClipBoxHeight * i + 8, frameBlockWidth * (_r - _l) + 16, conponentClipBoxHeight - 16), Color.yellow);
                }
                else
                {
                    if (Components[i].clip.PauseTime != null&& Components[i].clip.PauseTime.Length==2)
                    {
                        EditorGUI.DrawRect(new Rect(RightRootBlock.x + frameBlockWidth * (Components[i].clip.PauseTime[0] + 1) - 8, topBlockHeight + conponentClipBoxHeight * i + 8, frameBlockWidth * (Components[i].clip.PauseTime[1] - Components[i].clip.PauseTime[0]) + 16, conponentClipBoxHeight - 16), Color.yellow);
                    }
                }

                if (Components[i].clip.OutData.Count > 0)
                {
                    for(int j=0;j< Components[i].clip.OutData.Count; j++)
                    {
                        FrameOutputData _f = Components[i].clip.OutData[j];

                        if (i==outPutSelect.x && _f.Index == outPutSelect.y)
                        {
                            GUI.Box(new Rect(RightRootBlock.x + frameBlockWidth * (_f.Index + 1+outPutOffset) - 6, topBlockHeight + conponentClipBoxHeight * i + 2, 12, 6), buttonATex, NilStyle);
                        }
                        else
                        {
                            GUI.Box(new Rect(RightRootBlock.x + frameBlockWidth * (_f.Index + 1) - 6, topBlockHeight + conponentClipBoxHeight * i + 2, 12, 6), buttonTex, NilStyle);
                        }
                    }
                    
                }

                if (Components[i].clip.AdvanceEnd > 0)
                {
                    int _end = Components[i].clip.GeneralKeysIndex[Components[i].clip.GeneralKeysIndex.Count - 1]- Components[i].clip.AdvanceEnd;
                    EditorGUI.DrawRect(new Rect(RightRootBlock.x + frameBlockWidth * (_end + 1) - 2, topBlockHeight + conponentClipBoxHeight * i, 4, conponentClipBoxHeight), Color.gray);
                }

                for (int j = 0; j < Components[i].clip.GeneralKeysIndex.Count; j++)
                {
                    int _index = Components[i].clip.GeneralKeysIndex[j];
                    SelectedKeyData _skd = Components[i].GetSelectedData(_index);
                    bool _isAllSelected = false;
                    bool _isXSelect = false;
                    bool _isYSelect = false;
                    bool _isASelect = false;


                    if (_skd != null)
                    {
                        _isAllSelected = _skd.all;
                        _isXSelect = _skd.x;
                        _isYSelect = _skd.y;
                        _isASelect = _skd.a;
                    }
                    
                    Color _c = GetKeyFrameColor(KeyType.LinearKey, _isAllSelected);

                    float _dX;

                    if (_isAllSelected)
                    {
                        _dX = RightRootBlock.x + frameBlockWidth * (_index + 1 + draggingKeyFramesOffset);
                    }
                    else
                    {
                        _dX = RightRootBlock.x + frameBlockWidth * (_index + 1);
                    }
                    EditorGUI.DrawRect(new Rect(_dX - keySqureRadius, _dY - keySqureRadius, 2 * keySqureRadius, 2 * keySqureRadius), _c);
                    
                    if (_isShow)
                    {
                        KeyType _xType = Components[i].clip.Frames[_index].XKey;
                        if(_xType!= KeyType.NotKey)
                        {
                            Color _c1 = GetKeyFrameColor(_xType, _isXSelect);

                            if (_isXSelect)
                            {
                                _dX = RightRootBlock.x + frameBlockWidth * (_index + 1 + draggingKeyFramesOffset);
                            }
                            else
                            {
                                _dX = RightRootBlock.x + frameBlockWidth * (_index + 1);
                            }
                            EditorGUI.DrawRect(new Rect(_dX - keySqureRadius, _dY - keySqureRadius + conponentClipBoxHeight, 2 * keySqureRadius, 2 * keySqureRadius), _c1);
                        }
                        
                        KeyType _yType = Components[i].clip.Frames[_index].YKey;
                        if (_yType != KeyType.NotKey)
                        {
                            Color _c1 = GetKeyFrameColor(_yType, _isYSelect);

                            if (_isYSelect)
                            {
                                _dX = RightRootBlock.x + frameBlockWidth * (_index + 1 + draggingKeyFramesOffset);
                            }
                            else
                            {
                                _dX = RightRootBlock.x + frameBlockWidth * (_index + 1);
                            }
                            EditorGUI.DrawRect(new Rect(_dX - keySqureRadius, _dY - keySqureRadius +2 * conponentClipBoxHeight, 2 * keySqureRadius, 2 * keySqureRadius), _c1);
                        }

                        KeyType _aType = Components[i].clip.Frames[_index].AKey;
                        if (_aType != KeyType.NotKey)
                        {
                            Color _c1 = GetKeyFrameColor(_aType, _isASelect);

                            if (_isASelect)
                            {
                                _dX = RightRootBlock.x + frameBlockWidth * (_index + 1 + draggingKeyFramesOffset);
                            }
                            else
                            {
                                _dX = RightRootBlock.x + frameBlockWidth * (_index + 1);
                            }
                            EditorGUI.DrawRect(new Rect(_dX - keySqureRadius, _dY - keySqureRadius + 3 * conponentClipBoxHeight, 2 * keySqureRadius, 2 * keySqureRadius), _c1);
                        }
                        
                    }
                }
                if (_isShow) _verticalIndex += 3;
                 _verticalIndex++;


                //RightRootBlock.x + frameBlockWidth * i
            }
        }

        if (atlas != null&&atlas.Frames!=null)
        {
            bool _isAtlasShow= atlas.showAtlasPara;


            float _dY = topBlockHeight + conponentClipBoxHeight * (_verticalIndex + 0.5f);
            float _dX;
            List<EditorSpriteFrame_Container> _frames = atlas.Frames;
            for (int i=0;i< _frames.Count; i++)
            {
                if (_frames[i].isKeyFrame)
                {
                    SelectedKeyData _skd = atlas.GetSelectedData(i);

                    Color _col;

                    if (_skd.all)
                    {
                        _col = Color.blue;
                        _dX = RightRootBlock.x + frameBlockWidth * (i + 1 + draggingKeyFramesOffset);
                    }
                    else
                    {
                        _col = Color.black;
                        _dX = RightRootBlock.x + frameBlockWidth * (i + 1);
                    }
                    EditorGUI.DrawRect(new Rect(_dX - keySqureRadius, _dY - keySqureRadius, 2 * keySqureRadius, 2 * keySqureRadius), _col);


                    if (_isAtlasShow)
                    {
                        if (_frames[i].isKeySprite)
                        {
                            if (_skd.sp)
                            {
                                _col = Color.blue;
                                _dX = RightRootBlock.x + frameBlockWidth * (i + 1 + draggingKeyFramesOffset);
                            }
                            else
                            {
                                _col = Color.black;
                                _dX = RightRootBlock.x + frameBlockWidth * (i + 1);
                            }

                            EditorGUI.DrawRect(new Rect(_dX - keySqureRadius, _dY - keySqureRadius+ conponentClipBoxHeight, 2 * keySqureRadius, 2 * keySqureRadius), _col);

                            if (atlas.isFace)
                            {
                                if (_frames[i].isKeyFace)
                                {
                                    _col = Color.black;
                                }
                                else
                                {
                                    _col = Color.gray;
                                }
                                EditorGUI.DrawRect(new Rect(_dX - keySqureRadius, _dY - keySqureRadius + conponentClipBoxHeight*3, 2 * keySqureRadius, 2 * keySqureRadius), _col);
                            }
                        }

                        if (_frames[i].isKeyAlpha)
                        {
                            if (_skd.alpha)
                            {
                                _col = Color.blue;
                                _dX = RightRootBlock.x + frameBlockWidth * (i + 1 + draggingKeyFramesOffset);
                            }
                            else
                            {
                                _col = Color.black;
                                _dX = RightRootBlock.x + frameBlockWidth * (i + 1);
                            }

                            EditorGUI.DrawRect(new Rect(_dX - keySqureRadius, _dY - keySqureRadius + 2* conponentClipBoxHeight, 2 * keySqureRadius, 2 * keySqureRadius), _col);
                        }
                    }
                }

                
            }

            if (atlas.isHaveTempKeys)
            {
                List<Vector2Int> _temp;
                Color _col;
                atlas.GetTempKeys(out _temp,out _col);

                int _delta = atlas.TemkeyIndexDelta;

                for (int i = 0; i < _temp.Count; i++)
                {
                    _dY = topBlockHeight + conponentClipBoxHeight * (_temp[i].y + 0.5f);
                    _dX= RightRootBlock.x + frameBlockWidth * (_temp[i].x + 1+ _delta); ;

                    EditorGUI.DrawRect(new Rect(_dX - keySqureRadius, _dY - keySqureRadius, 2 * keySqureRadius, 2 * keySqureRadius), _col);
                }
            }
        }
    }

    void DrawSelectRect()
    {
        if (isDragging && (draggingControl==3 || draggingControl == 103) )
        {
            EditorGUI.DrawRect(SelectRect, new Color(0.4f, 0.4f, 0.7f, 0.6f));
        }
    }

    void RectSelect(int _rectX1, int _rectX2, int _rectY1, int _rectY2)
    {
        if (_rectY1 == _rectY2) return;
        int _y1, _y2, _x1, _x2;

        if (_rectY1 > _rectY2)
        {
            _y1 = _rectY2; _y2 = _rectY1;
        }
        else
        {
            _y1 = _rectY1; _y2 = _rectY2;
        }

        if (_rectX1 > _rectX2)
        {
            _x1 = _rectX2-1; _x2 = _rectX1 - 1;
        }
        else
        {
            _x1 = _rectX1 - 1; _x2 = _rectX2 - 1;
        }

        if(originDisplay == OriginAminType.SpriteFrames)
        {
            for (int i = _y1; i < _y2; i++)
            {
                for(int j = _x1; j <= _x2; j++)
                {
                    atlas.SelectionKeyFrameAdd(j, i);
                }
            }
        }
        else
        {
            for (int i = _y1; i < _y2; i++)
            {
                //int _count = Components[i].clip.keys.Count;
                Vector2Int _v2 = GetComponentIndexAndParaIndex(i);
                //Debug.Log("Select Rect " + _v2);
                int _count = Components[_v2.x].clip.GeneralKeysIndex.Count;

                for (int j = 0; j < _count; j++)
                {
                    int k = Components[_v2.x].clip.GeneralKeysIndex[j];
                    if (k >= _x1)
                    {
                        if (k > _x2) break;
                        Components[_v2.x].SelectionKeyFrameAdd(k, _v2.y);
                    }
                }
            }
        }
    }

    void DraggingOn(int _controlID)
    {
        isDragging = true;
        draggingControl = _controlID;
    }

    void DraggingOff()
    {
        isDragging = false;
        draggingControl = 0;
    }


    void GetRightBlockFrameMaxNumber()
    {
        int _max = 0;

        if (Components != null)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                _max = Mathf.Max(_max, Components[i].clip.FramesCount- Components[i].clip.AdvanceEnd);
            }
        }

        if (atlas != null&& atlas.Frames!=null)
        {
            _max = Mathf.Max(_max, atlas.Frames.Count);
            Debug.Log("atlasmax=" + _max);
        }

        realMaxFrameNum = _max;
        if (realMaxFrameNum > 0.5f) { realMaxFrameNum += 0.5f; }
        maxFrameNum = Mathf.Max(_max+20, 40);
    }

    void RefreshProtypeAmina(int _index)
    {
        //Debug.Log(_index);
        if (Components != null)
        {
            for(int i = 0; i < Components.Count; i++)
            {
                if (Components[i].clip.FramesCount > _index)
                {
                    EditorAminaComponentClip.EditorAminaComponentClipData _af = Components[i].clip.Frames[_index];

                    Components[i].ComponentGO.transform.localPosition = new Vector2(_af.x,_af.y);
                    Components[i].ComponentGO.transform.eulerAngles = new Vector3(0, 0, _af.angle);
                }
            }
        }

        if (atlas != null)
        {
            atlas.GetSprite(_index);
        }
    }

    public void ClearSelectKey()
    {
        bool isRepaint = false;
        foreach(AminaEditorComponetData _aecd in Components)
        {
            if (_aecd.SelectionKeyFrame.Count > 0)
            {
                _aecd.ClearSelectionKeyFrame();
                isRepaint = true;
            }            
        }
        if (isRepaint) Repaint();
    }

    public void ClearSelectKey_atlas()
    {
        bool isRepaint = false;
        if (atlas.SelectionKeyFrame.Count > 0)
        {
            atlas.ClearSelectionKeyFrame();
            isRepaint = true;
        }

        if (isRepaint) Repaint();
    }

    void ScanNextFrame()
    {
        if (currentAminaPointerIndex < realMaxFrameNum-1)
        {
            currentAminaPointerIndex++;
            RefreshProtypeAmina(currentAminaPointerIndex - 1);
            Repaint();
        }
    }

    void ScanPreFrame()
    {
        if (currentAminaPointerIndex > 1)
        {
            currentAminaPointerIndex--;
            RefreshProtypeAmina(currentAminaPointerIndex - 1);
            Repaint();
        }
    }

    public Vector3Int GetKeyVec(int _y,int _frameIndex)
    {
        int _componentIndex=0;
        int _paraIndex=0;
        int k = 0;
        for (int i = 0; i < Components.Count; i++)
        {
            k++;
            int j = 0;
            if (Components[i].showComponentPara)
            {
                j = 3;
            }

            if (_y < k+j)
            {
                _componentIndex = i;

                _paraIndex = _y-k+1;

                return new Vector3Int(_componentIndex, _frameIndex, _paraIndex);
            }
            k += j;
        }

        Debug.LogWarning("can not get KeyVec  intput _y=" + _y);
        return Vector3Int.zero;
    }

    /// <summary>
    /// 根据选择的编辑器行数返回具体是哪个部件和该部件的那些参数的行数
    /// </summary>
    /// <param name="_y"></param>
    /// <returns></returns>
    public Vector2Int GetComponentIndexAndParaIndex(int _y)
    {
        int _componentIndex = 0;
        int _paraIndex = 0;
        int k = 0;
        for (int i = 0; i < Components.Count; i++)
        {
            k++;
            int j = 0;
            if (Components[i].showComponentPara)
            {
                j = 3;
            }

            if (_y < k + j)
            {
                _componentIndex = i;

                _paraIndex = _y - k + 1;

                return new Vector2Int(_componentIndex, _paraIndex);
            }
            k += j;
        }

        Debug.LogWarning("can not get KeyVec  intput _y=" + _y);
        return Vector2Int.zero;
    }


    public int GetComponentIndex(int _calInt)
    {
        int _limit=0;
        for (int i = 0; i < Components.Count; i++)
        {
            _limit++;
            if (Components[i].showComponentPara)
            {
                _limit += 3;
            }

            if (_calInt < _limit)
            {
                return i;
            }

        }

        Debug.LogWarning("GetComponentIndex input out of index limit="+_limit);

        return 0;
    }

    Color GetKeyFrameColor(KeyType _type,bool _isSelected)
    {
        Color _col = new Color();
        //Debug.Log("select key"+ _key);
        if (_isSelected)
        {
            switch (_type)
            {
                case KeyType.LinearKey:
                    _col = Color.blue;
                    break;
                case KeyType.AccelerateKey:
                    _col = new Color(0.4f, 0.8f, 0.4f);
                    break;
                case KeyType.TrigonoKey:
                case KeyType.CosineKey:
                    _col = new Color(0.3f, 0.7f, 0.7f);
                    break;
                case KeyType.DecelerateKey:
                    _col = new Color(0.4f, 0.4f, 0.8f);
                    break;
                case KeyType.SineKey:
                    _col = new Color(0.7f, 0.3f, 0.7f);
                    break;
            }
        }
        else
        {
            //Debug.Log("select key"+ _key);
            switch (_type)
            {
                case KeyType.LinearKey:
                    _col = Color.black;
                    break;
                case KeyType.AccelerateKey:
                    _col = new Color(0.4f, 0.2f, 0);
                    break;
                case KeyType.TrigonoKey:
                case KeyType.CosineKey:
                    _col = new Color(0.3f, 0.0f, 0.0f);
                    break;
                case KeyType.DecelerateKey:
                    _col = new Color(0.2f, 0.4f, 0);
                    break;
                case KeyType.SineKey:
                    _col = new Color(0f,0.3f,0.0f);
                    break;
            }
        }
        return _col;
    }

    public void LoadClip(int _compID,EditorAminaComponentClip _other)
    {
        //Debug.Log("other frames count="+_other.Frames.Count);
        Selection.activeObject = null;
        for (int i = 0; i < Components.Count; i++)
        {
            if (Components[i].componentID == _compID)
            {
                ClearSelectKey();
                Components[i].clip.Load(_other);
                Components[i].labelStr = Components[i].componentName + _other.name;
                GetRightBlockFrameMaxNumber();
                return;
            }
        }
        //outputDataEditorContainer.outputData = null;
        Debug.LogWarning("LoadClip failed,don't have this compID=" + _compID);
    }

    public void LoadEditorAminaClip(EditorAnimaClip _eac)
    {
        clip = _eac;
        string[] _all = System.IO.Directory.GetFiles(AminaEditorPath.FRAMEPATH, "*.asset", System.IO.SearchOption.AllDirectories);
        List<int> _cId = new List<int>(_eac.compFramesID);
        if (_cId.Count > 0)
        {
            for (int i = 0; i < _all.Length; i++)
            {
                EditorAminaComponentClip _new = AssetDatabase.LoadAssetAtPath<EditorAminaComponentClip>(_all[i]);
                if (_new != null)
                {
                    for(int j=0;j< _cId.Count; j++)
                    {
                        if (_new.ID == _cId[j])
                        {
                            LoadClip(_new.CompID, _new);
                            _cId.RemoveAt(j);
                            if (_cId.Count == 0)
                            {
                                return;
                            }
                            break;
                        }
                    }
                }
            }
        }
    }

    public void LoadOnlyAtlas(AnimaDataOnlyScritable _atlas)
    {
        AnimaChType = AnimaCharactorType.Def;
        atlas.LoadEditorAnimaClipAtlas(_atlas.Clip);
        Selection.activeObject = _atlas;
        GetRightBlockFrameMaxNumber();
    }

    public void LoadCharactorAtlas(AnimaDataCharactorScritable _atlas)
    {
        Debug.Log("LoadCharactorAtlas");
        chScript = new AnimaDataCharactorScritable_Container();
        chScript.LoadAnimaDataCharactorScritable(_atlas, protype);
        isChScript = true;
        AnimaChType = AnimaCharactorType.Def;
        if (chScript.Clips.Count > 0)
        {
            Debug.Log("dadad");
            for (int i=0;i< chScript.Clips.Count; i++)
            {
                if (chScript.Clips[i].Frames.Count > 0)
                {
                    AnimaChType = chScript.Clips[i].CharactorType;
                    atlas.LoadEditorAnimaClipAtlas(chScript.Clips[i],chScript.isFace);
                    Debug.Log("载入动画" + AnimaChType);
                    break;
                }
            }
        }




        Selection.activeObject = _atlas;
        GetRightBlockFrameMaxNumber();
    }

    public void AtlasGUILeft()
    {
        if (isChScript)
        {
            AnimaCharactorType _choseChType = (AnimaCharactorType)EditorGUI.EnumPopup(new Rect(180, 8, 100, topBlockHeight), AnimaChType);
            if (_choseChType != AnimaChType)
            {
                //atlas.LoadEditorAnimaClipAtlas()

                AnimaChType = _choseChType;
                bool isNewEmptyClip=true;
                for(int i=0;i< chScript.Clips.Count; i++)
                {
                    if(chScript.Clips[i].CharactorType== AnimaChType)
                    {
                        atlas.LoadEditorAnimaClipAtlas(chScript.Clips[i], chScript.isFace);
                        isNewEmptyClip = false;
                        break;
                    }
                }
                if (isNewEmptyClip)
                {
                    atlas.LoadEditorAnimaClipAtlas(null);
                }

                GetRightBlockFrameMaxNumber();
                Repaint();
            }
        }
        
    }

    //public List<Sprite> AtlasDragSprites(Event _e,Rect _r)
    public bool AtlasMouseDragSprites(Event _e)
    {
        //Debug.Log("AtlasDragSprites");
        switch (_e.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (!isAtlasDragSprites)
                {
                    Debug.Log("AtlasDragSprites");

                    Vector2 frameEditPos = Rect.PointToNormalized(frameEditorBlock, _e.mousePosition);
                    int _frameIndex = Convert.ToInt16(frameEditPos.x * aminaTopBlock.width / frameBlockWidth) - 1;

                    isAtlasDragSprites = true;

                    AtlasDragSprites = new List<Sprite>();
                    //bool _isAllSprites = true;

                    for (int i = 0; i < DragAndDrop.objectReferences.Length; ++i)
                    {
                        //TextureImporter textureImporter = new TextureImporter();

                        Debug.Log(DragAndDrop.objectReferences[i].GetType());
                        Sprite _s = null;
                        System.Type _t = DragAndDrop.objectReferences[i].GetType();
                        if (_t == typeof(Texture2D))
                        {
                            UnityEngine.Object _o = DragAndDrop.objectReferences[i];

                            string _path = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[i]);

                            UnityEngine.Object _sp = AssetDatabase.LoadAssetAtPath(_path, typeof(Sprite));
                            if (_sp != null)
                            {
                                AtlasDragSprites.Add(_sp as Sprite);
                            }
                        }
                        else
                        {
                            _s = DragAndDrop.objectReferences[i] as Sprite;
                            if (_s != null)
                            {
                                AtlasDragSprites.Add(_s);
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }

                    if (AtlasDragSprites.Count > 0)
                    {
                        List<TempSpriteKey> _k = new List<TempSpriteKey>();

                        int _start = Mathf.Max(_frameIndex,0);
                        

                        for (int i=0;i< AtlasDragSprites.Count; i++)
                        {
                            TempSpriteKey _t = new TempSpriteKey();
                            _t.Index = _start + i;
                            Debug.Log("t.index=" + _t.Index);
                            _t.Sp = AtlasDragSprites[i];
                            _k.Add(_t);
                        }
                        atlas.SetTempSpriteKeys(_k, _start);
                    }
                }
                else
                {
                    if (atlas.isHaveTempKeys)
                    {
                        Vector2 frameEditPos = Rect.PointToNormalized(frameEditorBlock, _e.mousePosition);
                        int _frameIndex = Convert.ToInt16(frameEditPos.x * aminaTopBlock.width / frameBlockWidth) - 1;
                        _frameIndex= Mathf.Max(_frameIndex, 0);
                        atlas.SetTemkeyIndexCurrent(_frameIndex);
                    }
                }
                
                if (_e.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();
                    Event.current.Use();
                    atlas.TempkeysToAddNewSprite();
                    atlas.ClearTempKeys();

                    return true;
                }
                Event.current.Use();



                break;
            default:
                //isAtlasDragSprites = false;
                break;
        }
        
        return false;
    }

    public void AddSpriteKey(int _index,Sprite _sp)
    {
        editorUndo.AddAtlasSpriteKey(_index, _sp);
        GetRightBlockFrameMaxNumber();
    }

    public void AddSpriteKey(List<TempSpriteKey> _sp)
    {
        editorUndo.AddAtlasMultiSpritesKey(_sp);
        GetRightBlockFrameMaxNumber();
    }

}

public class SelectedKeyData : IComparable<SelectedKeyData>, IEquatable<SelectedKeyData>
{
    public int Index;
    public bool all;
    public bool x;
    public bool y;
    public bool a;

    //atlas字段
    public bool sp;
    public bool alpha;
    public bool face;

    public SelectedKeyData(int _index)
    {
        Index = _index;
        x = y = a = sp = alpha= face = false;
    }

    public SelectedKeyData(SelectedKeyData _other)
    {
        Index = _other.Index;
        all = _other.all;
        x = _other.x;
        y = _other.y;
        a = _other.a;
        sp = _other.sp;
        alpha = _other.alpha;
        face = _other.face;
    }

    public int CompareTo(SelectedKeyData _other)
    {
        if (_other == null)
            return 1;
        else
            return this.Index.CompareTo(_other.Index);
    }

    public bool Equals(SelectedKeyData _other)
    {
        if (_other == null)
            return false;
        else
        {
            bool _result = this.Index.Equals(_other.Index);
            if (_result) Debug.LogWarning("SelectedKeyData Equals true");
            return _result;
        }
    }

}

public class CopyKeyData
{
    public int offset;
    public int componentIndex;
    public FrameParaType paraType;
    public KeyType type;
    public float para;
    public IFrameFillPara fillPara;
}

public class FrameOutputDataEditorContainer:Editor
{
    public FrameOutputData outputData;
    public FrameOutputData_Atlas outputData_atlas;
}

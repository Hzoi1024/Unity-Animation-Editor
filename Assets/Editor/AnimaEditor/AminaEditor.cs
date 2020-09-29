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
}


public class AminaEditor : EditorWindow
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

    public AminaEditorConfigSO scriptableObject;
    //EditorAminaComponentClip componentClip;
    public List<AminaEditorComponetData> Components;
    int componentParaCount;

    List<CopyKeyData> copies;
    AminaEditorUndo editorUndo;

    GameObject protype;
    Vector2 rightScrollPos;
    Vector2 leftScrollPos;
    public Vector2 scrollPosition = Vector2.zero;

    Rect LeftRootBlock;//编辑器左边部分的整体划分
    Rect RightRootBlock;
    Rect frameEditorBlock;

    Rect LeftComponentEditBlock;

    float LeftRootBlockWidth = 400;
    Vector2 scrollView = Vector2.zero;

    float MiddleControlBlockWidth;

    bool isDragging;
    int draggingControl;

    //bool isDraggingMiddleControl = false;
    //bool isDraggingaminaTopBlock = false;//拖拽播放红轴


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

    GUIContent playTex;
    GUIContent PlayingTex;
    int maxFrameNum;
    float realMaxFrameNum;



    float frameTime = 1f / 30f;
    float deltaTime;
    float playTime;
    double lastStartUpTime;

    public enum OriginAminType
    {
        Defualt = 0,
        Human = 1,
    }
    OriginAminType originDisplay;

    void OnInspectorUpdate()
    {
        
    }

    void OnHierarchyChange()
    {
        //Debug.Log("OnHierarchyChange");
    }

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

        isDragging = false;
        

    }
    

    void OnGUI()
    {
        topBlockHeight = 30;
        Event eventCurrent = Event.current;


        OriginAminType choseOriginAminType;
        choseOriginAminType = (OriginAminType)EditorGUI.EnumPopup(new Rect(0, 0, 100, topBlockHeight),originDisplay);
        if (choseOriginAminType != originDisplay)
        {
            ProtypeInit(choseOriginAminType);
        }
        originDisplay = choseOriginAminType;

        LeftRootBlock = new Rect(0, topBlockHeight, LeftRootBlockWidth, position.height);


        Rect playBox = new Rect(LeftRootBlockWidth- 60, 3, 24, 24);

        if (isPlaying)
        {
            GUI.Box(playBox, PlayingTex, NilStyle);
        }
        else
        {
            GUI.Box(playBox, playTex, NilStyle);
        }

        //scrollView = GUI.BeginScrollView(LeftRootBlock, scrollView, new Rect(0, 0, LeftRootBlockWidth, 300));
        //scriptableObject = AssetDatabase.LoadAssetAtPath<AminaEditorConfigSO>("Assets/Editor" + "/AminaEditorConfig.asset");
        //GUI.EndScrollView();

        float _scrollHeight=0;

        if (Components != null)
        {
            _scrollHeight = (Components.Count + componentParaCount) * conponentClipBoxHeight;
        }


        Rect _leftEditBlock=new Rect(0, topBlockHeight, LeftRootBlockWidth,position.height- topBlockHeight-15);


        leftScrollPos = new Vector2(0, rightScrollPos.y);
        leftScrollPos = GUI.BeginScrollView(_leftEditBlock, leftScrollPos, new Rect(0, topBlockHeight, LeftRootBlockWidth-20, _scrollHeight));

        DrawLeftComponent();
        if (eventCurrent.button == 0 && eventCurrent.type == EventType.MouseDown)
        {
            if (Components != null)
            {
                LeftComponentEditBlock = new Rect(0, topBlockHeight, LeftRootBlock.width, conponentClipBoxHeight * (Components.Count+componentParaCount));

                if (LeftComponentEditBlock.Contains(eventCurrent.mousePosition))
                {
                    Vector2 _leftEditPos = Rect.PointToNormalized(LeftComponentEditBlock, eventCurrent.mousePosition);

                    int _leftIndex = (int)(_leftEditPos.y * (Components.Count + componentParaCount));

                    int _i = GetComponentIndex(_leftIndex);

                    Selection.activeGameObject = Components[_i].ComponentGO;
                }
                eventCurrent.Use();
            }
        }


        GUI.EndScrollView();

        

        MiddleControlBlockWidth = 5;
        Rect MiddleControlBlock = new Rect(LeftRootBlockWidth, 0, MiddleControlBlockWidth, position.height);
        GUI.Box(MiddleControlBlock, "");

        EditorGUIUtility.AddCursorRect(MiddleControlBlock, MouseCursor.ResizeHorizontal);
        
        ////////////////////////
        //右边
        ////////////////////////

        //maxFrameNum = GetRightBlockFrameMaxNumber();
        frameBlockWidth = 30;
        //float _aminaWidth = position.width - LeftRootBlockWidth - MiddleControlBlockWidth;
        float _aminaWidth = maxFrameNum * frameBlockWidth;
        Rect _aminaTopBlock = new Rect(LeftRootBlockWidth + MiddleControlBlockWidth, 0, _aminaWidth, topBlockHeight);
        RightRootBlock = new Rect(_aminaTopBlock.x, 0, _aminaTopBlock.width, position.height);
        Rect _scrollViewRect = new Rect(_aminaTopBlock.x, topBlockHeight, position.width - LeftRootBlockWidth - MiddleControlBlockWidth, position.height - topBlockHeight);
        Rect _scrollTrueRect = new Rect(_aminaTopBlock.x, topBlockHeight, _aminaTopBlock.width, _scrollHeight);
        //第一个rect是 卷轴可视范围  第二个rect是卷轴内实际显示范围  如果第二个rect超过第一个则会出现卷轴
        rightScrollPos.y = leftScrollPos.y;


        //EditorGUI.DrawRect(_aminaTopBlock, new Color(0.85f, 0.85f, 0.85f));

        //================
        //frameBlockWidth = _aminaWidth / 32;


        if (isDragging)
        {
            if (draggingControl == 4)
            {
                Rect WindowRect = new Rect(new Vector2(0, 0), position.size);
                LeftRootBlockWidth = position.width * Rect.PointToNormalized(WindowRect, eventCurrent.mousePosition).x;
                Repaint();
                if (eventCurrent.type == EventType.MouseUp)
                {
                    //isDraggingMiddleControl = false;
                    DraggingOff();
                }
            }
        }else if(eventCurrent.button == 0 && eventCurrent.isMouse)
        {

            if (eventCurrent.type== EventType.MouseDown&& playBox.Contains(eventCurrent.mousePosition))
            {
                ChangePlayBool();
                eventCurrent.Use();
            }

            if (_aminaTopBlock.Contains(eventCurrent.mousePosition))
            {
                float _aminaMouseXIndexF = (Rect.PointToNormalized(_aminaTopBlock, eventCurrent.mousePosition).x * _aminaTopBlock.width+ rightScrollPos.x) / frameBlockWidth;
                int newAminaPointerIndex = Convert.ToInt16(_aminaMouseXIndexF);
                //Debug.Log("currentAminaPointerIndex:" + currentAminaPointerIndex);
                if (newAminaPointerIndex < 1)
                {
                    newAminaPointerIndex = 1;
                }

                if (newAminaPointerIndex != currentAminaPointerIndex)
                {
                    currentAminaPointerIndex = newAminaPointerIndex;
                    RefreshProtypeAmina(currentAminaPointerIndex - 1);
                    eventCurrent.Use();
                    Repaint();
                }

                if (eventCurrent.type == EventType.MouseDrag)
                {
                    //isDraggingaminaTopBlock = true;
                    DraggingOn(2);
                }
            }else if (MiddleControlBlock.Contains(eventCurrent.mousePosition))
            {
                if (eventCurrent.type == EventType.MouseDrag)
                {
                    //isDraggingaminaTopBlock = true;
                    DraggingOn(4);
                }
            }
        }
        

        /////////////////////////////////
        //编辑区域
        /////////////////////////////////
        rightScrollPos = GUI.BeginScrollView(_scrollViewRect, rightScrollPos, _scrollTrueRect, false, false);

        DrawRightComponent();
        DrawKeyFrame();


        EditorGUI.DrawRect(new Rect(frameEditorBlock.x + realMaxFrameNum * frameBlockWidth, frameEditorBlock.y, frameEditorBlock.width - realMaxFrameNum * frameBlockWidth, RightRootBlock.height), new Color(0.8f, 0.8f, 0.8f, 0.7f));


        DrawSelectRect();
        if (isDragging)
        {
            switch (draggingControl)
            {
                case 1://拖拽关键帧
                    Vector2 frameEditPos = Rect.PointToNormalized(frameEditorBlock, eventCurrent.mousePosition);
                    //wantsMouseMove = true;
                    int _aminaMouseXIndexF = Convert.ToInt16(frameEditPos.x * _aminaTopBlock.width / frameBlockWidth) - 1;
                    if (_aminaMouseXIndexF < 0)
                    {
                        _aminaMouseXIndexF = 0;
                    }

                    draggingKeyFramesOffset = _aminaMouseXIndexF - draggingKeyFramesBase;

                    Repaint();

                    if (eventCurrent.rawType == EventType.MouseUp)
                    {
                        //isDraggingKeyFrames = false;

                        if (draggingKeyFramesOffset != 0)
                        {
                            MoveSelectedKey(draggingKeyFramesOffset);

                            draggingKeyFramesOffset = 0;
                        }

                        DraggingOff();
                    }
                    break;
                case 2://拖拽上方时间轴
                    float _aminaMouseXIndex = Rect.PointToNormalized(_aminaTopBlock, eventCurrent.mousePosition).x * _aminaTopBlock.width / frameBlockWidth;
                    int newAminaPointerIndex = Convert.ToInt16(_aminaMouseXIndex);
                    //Debug.Log("currentAminaPointerIndex:" + currentAminaPointerIndex);
                    if (newAminaPointerIndex < 1)
                    {
                        newAminaPointerIndex = 1;
                    }

                    if (newAminaPointerIndex != currentAminaPointerIndex)
                    {
                        currentAminaPointerIndex = newAminaPointerIndex;
                        RefreshProtypeAmina(currentAminaPointerIndex - 1);
                        //eventCurrent.Use();
                        Repaint();
                    }

                    if (eventCurrent.rawType == EventType.MouseUp)
                    {
                        //isDraggingaminaTopBlock = false;
                        DraggingOff();
                    }
                    break;
                case 3://拖拽方形选择框
                    Vector2 _frameEditPos = Rect.PointToNormalized(frameEditorBlock, eventCurrent.mousePosition);
                    //wantsMouseMove = true;
                    int _aminaMouseXIndexFR = Convert.ToInt16(_frameEditPos.x * _aminaTopBlock.width / frameBlockWidth);
                    if (_aminaMouseXIndexFR < 0)
                    {
                        _aminaMouseXIndexFR = 0;
                    }

                    int _conponentIndex = Convert.ToInt16(_frameEditPos.y * (Components.Count + componentParaCount));
                    _conponentIndex = Mathf.Max(0, _conponentIndex);
                    _conponentIndex = Mathf.Min(Components.Count + componentParaCount, _conponentIndex);

                    if (SelectRectStart.x != _aminaMouseXIndexFR)
                    {
                        SelectRect = new Rect(RightRootBlock.x + frameBlockWidth * SelectRectStart.x, topBlockHeight + conponentClipBoxHeight * (SelectRectStart.y), (_aminaMouseXIndexFR - SelectRectStart.x) * frameBlockWidth, conponentClipBoxHeight * (_conponentIndex - SelectRectStart.y));
                    }
                    else
                    {
                        SelectRect = new Rect(RightRootBlock.x + frameBlockWidth * SelectRectStart.x - 4, topBlockHeight + conponentClipBoxHeight * (SelectRectStart.y), 8, conponentClipBoxHeight * (_conponentIndex - SelectRectStart.y));
                    }


                    if (eventCurrent.rawType == EventType.MouseUp)
                    {
                        //isDraggingSelectingRect = false;
                        RectSelect(SelectRectStart.x, _aminaMouseXIndexFR, SelectRectStart.y, _conponentIndex);
                        DraggingOff();

                    }
                    Repaint();
                    break;
                    //4是拖拽中间
            }
        }else
        {
            if (Components != null)
            {
                frameEditorBlock = new Rect(_aminaTopBlock.x, topBlockHeight, _aminaTopBlock.width, conponentClipBoxHeight * (Components.Count + componentParaCount));

                if (eventCurrent.type == EventType.MouseDown&& eventCurrent.button==1)
                {
                    if (frameEditorBlock.Contains(eventCurrent.mousePosition))
                    {

                        
                        Vector2 frameEditPos = Rect.PointToNormalized(frameEditorBlock, eventCurrent.mousePosition);

                        int conponentIndex = (int)(frameEditPos.y * (Components.Count+componentParaCount));
                        //Debug.Log("conponentIndex:" + conponentIndex);
                        int _frameIndex = Convert.ToInt16(frameEditPos.x * _aminaTopBlock.width / frameBlockWidth) - 1;
                        if (_frameIndex < 0) { _frameIndex = 0; }
                        //Debug.Log("frameIndex:" + _frameIndex);
                        var menu = new GenericMenu();
                        Vector3Int _addkeyPara = GetKeyVec(conponentIndex, _frameIndex);
                        menu.AddItem(new GUIContent("Add key"), false, Addkey, _addkeyPara);
                        menu.AddItem(new GUIContent("Add key Accelerate"), false, AddkeyAccelerate, _addkeyPara);
                        menu.AddItem(new GUIContent("Add key Trigonote"), false, AddkeyTrigono, _addkeyPara);
                        menu.AddItem(new GUIContent("Add key Decelerate"), false, AddkeyDecelerate, _addkeyPara);
                        menu.AddItem(new GUIContent("Add key Sine"), false, CallSinKeyWindow, _addkeyPara);
                        menu.AddItem(new GUIContent("Add key Cosine"), false, CallCosKeyWindow, _addkeyPara);
                        if (Components[_addkeyPara.x].clip.IsTheIndexHaveKey(_frameIndex, _addkeyPara.z))
                        {
                            menu.AddItem(new GUIContent("Delete key"), false, DeleteKey, _addkeyPara);
                            //menu.AddItem(new GUIContent("Copy"), false, CopyOneKey, _addkeyPara);
                        }
                        else
                        {
                            menu.AddDisabledItem(new GUIContent("Delete key"), false);
                            //menu.AddDisabledItem(new GUIContent("Copy"), false);

                        }
                        /*if (copies == null || copies.Count == 0)
                        {
                            menu.AddDisabledItem(new GUIContent("Paste"), false);
                        }
                        else
                        {
                            menu.AddItem(new GUIContent("Paste"), false, PasteKey, _addkeyPara);
                        }*/
                        menu.ShowAsContext();
                        eventCurrent.Use();
                    }
                }


            }

            if (eventCurrent.button == 0  && eventCurrent.isMouse)
            {
                if (Components != null)
                {
                    if (frameEditorBlock.Contains(eventCurrent.mousePosition))
                    {//点击关键帧
                        Vector2 frameEditPos = Rect.PointToNormalized(frameEditorBlock, eventCurrent.mousePosition);
                        int conponentIndex = (int)(frameEditPos.y * (Components.Count+componentParaCount));
                        //Debug.Log("conponentIndex:" + conponentIndex);
                        int _frameIndex = Convert.ToInt16(frameEditPos.x * _aminaTopBlock.width / frameBlockWidth) - 1;

                        Vector3Int _para = GetKeyVec(conponentIndex, _frameIndex);

                        if (_frameIndex < 0) { _frameIndex = 0; }

                        if (Components[_para.x].clip.IsTheIndexHaveKey(_frameIndex, _para.z))
                        {
                            float _dY = topBlockHeight + conponentClipBoxHeight * (conponentIndex + 0.5f);
                            float _dX = RightRootBlock.x + frameBlockWidth * (_frameIndex + 1);
                            Rect _keyRect = new Rect(_dX - keySqureRadius, _dY - keySqureRadius, 2 * keySqureRadius, 2 * keySqureRadius);
                            if (_keyRect.Contains(eventCurrent.mousePosition))
                            {
                                /*if (!Components[_para.x].SelectionKeyFrameContains(_frameIndex))
                                {
                                    ClearSelectKey();
                                    draggingKeyFramesOffset = 0;
                                    Components[_para.x].SelectionKeyFrameAdd(_frameIndex);
                                    Repaint();
                                    eventCurrent.Use();
                                }*/
                                if (eventCurrent.type == EventType.MouseDown&& !Components[_para.x].SelectionKeyContained(_frameIndex, _para.z)){
                                    ClearSelectKey();
                                    draggingKeyFramesOffset = 0;
                                    Components[_para.x].SelectionKeyFrameAdd(_frameIndex, _para.z);
                                    Repaint();
                                    eventCurrent.Use();
                                }
                                

                                if (eventCurrent.type == EventType.MouseDrag)
                                {
                                    //isDraggingKeyFrames = true;
                                    DraggingOn(1);
                                    draggingKeyFramesBase = _frameIndex;
                                }
                            }
                            else
                            {
                                ClearSelectKey();

                                if (eventCurrent.type == EventType.MouseDrag)
                                {
                                    //isDraggingSelectingRect = true;
                                    DraggingOn(3);
                                    SelectRectStart = new Vector2Int(_frameIndex+1,Convert.ToInt16(frameEditPos.y * (Components.Count+componentParaCount)));
                                }
                                

                            }
                        }
                        else
                        {
                            if (eventCurrent.type == EventType.MouseDrag)
                            {
                                //isDraggingSelectingRect = true;
                                DraggingOn(3);
                                SelectRectStart = new Vector2Int(_frameIndex + 1, Convert.ToInt16(frameEditPos.y * (Components.Count + componentParaCount)));
                                //SelectRectEnd = new Vector2Int(conponentIndex, _frameIndex);
                            }
                            ClearSelectKey();
                        }
                    }
                }
            }

            if (eventCurrent.rawType == EventType.KeyUp)
            {
                switch (eventCurrent.keyCode)
                {
                    case KeyCode.Z:
                        ClearSelectKey();
                        editorUndo.UndoOperation();
                        GetRightBlockFrameMaxNumber();
                        break;
                    case KeyCode.X:
                        ClearSelectKey();
                        editorUndo.DoOperation();
                        GetRightBlockFrameMaxNumber();
                        break;
                    case KeyCode.C:
                        CopyKey();
                        break;
                    case KeyCode.V:
                        PasteKey();
                        break;
                    case KeyCode.D:
                        if (eventCurrent.shift) ScanNextFrame();
                        else DeleteSelectedkey();
                        break;
                    case KeyCode.A:
                        if (eventCurrent.shift) ScanPreFrame();
                        else AddOneWholeKey();
                        break;
                    case KeyCode.F:
                        ChangePlayBool();
                        break;
                }
                eventCurrent.Use();
            }


        }
        
        




        GUI.EndScrollView();
        EditorGUI.DrawRect(_aminaTopBlock, new Color(0.85f, 0.85f, 0.85f));
        //画帧线
        float framelineX;

        GL.Begin(GL.LINES);
        GL.Color(new Color(0.66f, 0.66f, 0.66f));
        framelineX = RightRootBlock.x + frameBlockWidth - rightScrollPos.x;

        if (framelineX > RightRootBlock.x)
        {
            GL.Vertex(new Vector2(framelineX, RightRootBlock.y));
            GL.Vertex(new Vector2(framelineX, RightRootBlock.yMax - 15));
        }

        for (int i = 2; i < maxFrameNum; i++)
        {
            framelineX = RightRootBlock.x + frameBlockWidth * i - rightScrollPos.x;

            if (framelineX > RightRootBlock.x)
            {
                float _k;
                if ((i - 1) % 5 == 0)
                {
                    _k = 10f;
                }
                else
                {
                    _k = 5f;
                }
                GL.Vertex(new Vector2(framelineX, topBlockHeight - _k));
                GL.Vertex(new Vector2(framelineX, RightRootBlock.yMax- 15));
            }
        }
        GL.End();




        framelineX = RightRootBlock.x + frameBlockWidth * currentAminaPointerIndex - rightScrollPos.x;
        if (framelineX > RightRootBlock.x)
        {
            //画一条红线
            GL.Begin(GL.LINES);
            GL.Color(Color.red);


            //Debug.Log("framelineX"+framelineX);
            GL.Vertex(new Vector2(framelineX, RightRootBlock.y));
            GL.Vertex(new Vector2(framelineX, RightRootBlock.yMax - 15));
            GL.End();
        }
        for (int i = 1; i < maxFrameNum; i = i + 5)
        {
            float _labelLeft = _aminaTopBlock.x + frameBlockWidth * i - rightScrollPos.x;
            if (_labelLeft > RightRootBlock.x)
            {
                GUI.Label(new Rect(_labelLeft, 0, 30, 25), (i - 1).ToString());
            }

        }
        //================
        //label必须放在GL后面
        if (isShowWindow)
        {
            BeginWindows();
            switch (WindowControl)
            {
                case 1:
                    // All GUI.Window or GUILayout.Window must come inside here
                    TrigonoInputWindowRect = GUILayout.Window(1, TrigonoInputWindowRect, SineWindow, "Add Sine key");
                    break;
                case 2:
                    // All GUI.Window or GUILayout.Window must come inside here
                    TrigonoInputWindowRect = GUILayout.Window(2, TrigonoInputWindowRect, CosineWindow, "Add Cosine key");
                    break;
                default:
                    Debug.LogWarning("WindowControl wrong ! WindowControl=" + WindowControl);
                    isShowWindow = false;
                    break;
            }
            

            EndWindows();
        }
        
    }


    Rect TrigonoInputWindowRect = new Rect(600, 100, 200, 200);
    Rect TrigonoInputWindow_EnterButton = new Rect(20, 100, 160, 30);
    Rect TrigonoInputWindow_ExitButton = new Rect(20, 150, 160, 30);
    Vector3Int TrigonoInputWindowIndex;
    //float SineInput_delDis=0;
    float SineInput_startAngle = 0;
    float SineInput_endAngl = 0;

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
        GetRightBlockFrameMaxNumber();
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
                break;
            default:
                break;
        }

    }

    void DrawLeftComponent()
    {
        conponentClipBoxHeight = 30;
        float labelWidth = 20;

        if (Components != null)
        {
            componentParaCount = 0;

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
    }

    void DrawKeyFrame()
    {
        keySqureRadius = 5;

        if (Components != null)
        {
            int _verticalIndex = 0;

            for (int i = 0; i < Components.Count; i++)
            {
                float _dY = topBlockHeight + conponentClipBoxHeight * (_verticalIndex + 0.5f);

                bool _isShow = Components[i].showComponentPara;

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
    }

    void DrawSelectRect()
    {
        if (isDragging && draggingControl==3)
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
                    Components[_v2.x].SelectionKeyFrameAdd(k,_v2.y);
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
                _max = Mathf.Max(_max, Components[i].clip.FramesCount);
            }
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
        Debug.Log("other frames count="+_other.Frames.Count);

        for(int i = 0; i < Components.Count; i++)
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
        Debug.LogWarning("LoadClip failed,don't have this compID=" + _compID);
    }

    public class AminaEditorComponetData
    {
        public GameObject ComponentGO;
        public int componentID;
        public EditorAminaComponentClip clip;
        public string componentName;
        public string labelStr;
        public List<SelectedKeyData> SelectionKeyFrame;
        public bool showComponentPara;
        public AminaEditorComponetData(int _componentID, GameObject protype)
        {
            componentID = _componentID;
            showComponentPara = false;
            switch (_componentID)
            {
                case 1://头                    
                    ComponentGO = protype.transform.Find("Sp/Head").gameObject;
                    componentName = "Head:";
                    break;
                case 2://身体
                    ComponentGO = protype.transform.Find("Sp/Body").gameObject;
                    componentName = "Body:";
                    break;
                case 3://前手
                    ComponentGO = protype.transform.Find("Sp/FHand").gameObject;
                    componentName = "FHand:";
                    break;
                case 4://后手
                    ComponentGO = protype.transform.Find("Sp/BHand").gameObject;
                    componentName = "BHand:";
                    break;
                case 5://前脚
                    ComponentGO = protype.transform.Find("Sp/FLeg").gameObject;
                    componentName = "FLeg:";
                    break;
                case 6://后脚
                    ComponentGO = protype.transform.Find("Sp/BLeg").gameObject;
                    componentName = "BLeg:";
                    break;

            }
            SelectionKeyFrame = new List<SelectedKeyData>();
            labelStr = componentName + "New Frames";
            //clip = new EditorAminaComponentClip();
            clip = ScriptableObject.CreateInstance<EditorAminaComponentClip>();
            clip.Init(0, componentID, ComponentGO.transform.localPosition, ComponentGO.transform.localEulerAngles.z);
        }

        public void ClearSelectionKeyFrame()
        {
            SelectionKeyFrame = new List<SelectedKeyData>();
        }

        public bool SelectionKeyContained(int _index,int _paraIndex)
        {
            if (clip.Frames.Count > _index)
            {
                foreach (SelectedKeyData _s in SelectionKeyFrame)
                {
                    if (_s.Index == _index)
                    {
                        switch (_paraIndex)
                        {
                            case 0:
                                if (_s.all) return true;
                                return false;
                            case 1:
                                if (_s.x) return true;
                                return false;
                            case 2:
                                if (_s.y) return true;
                                return false;
                            case 3:
                                if (_s.a) return true;
                                return false;
                            default:
                                Debug.LogWarning("SelectionKeyContained wrong  _paraIndex int =" + _paraIndex);
                                return false;
                        }
                    }
                }
            }
            return false;
        }

        public void SelectionKeyFrameAdd(int _index, int _paraType)
        {
            if (clip.Frames.Count>_index&& clip.Frames[_index].isKey)
            {


                foreach (SelectedKeyData _s in SelectionKeyFrame)
                {
                    if (_s.Index == _index)
                    {
                        switch (_paraType)
                        {
                            case 0:
                                if (clip.Frames[_index].XKey != KeyType.NotKey) _s.x = true;
                                if (clip.Frames[_index].YKey != KeyType.NotKey) _s.y = true;
                                if (clip.Frames[_index].AKey != KeyType.NotKey) _s.a = true;
                                _s.all = true;
                                //_s.all =_s.x = _s.y = _s.a = true;
                                break;
                            case 1:
                                if (clip.Frames[_index].XKey != KeyType.NotKey) _s.x = true;
                                break;
                            case 2:
                                if (clip.Frames[_index].YKey != KeyType.NotKey) _s.y = true;
                                break;
                            case 3:
                                if (clip.Frames[_index].AKey != KeyType.NotKey) _s.a = true;
                                break;
                            default:
                                Debug.LogWarning("SelectionKeyFrameAdd wrong  paraType int =" + _paraType);
                                break;
                        }
                        return;
                    }
                }

            SelectedKeyData _newSD = new SelectedKeyData(_index);
            switch (_paraType)
            {
                case 0:
                    _newSD.all = true;
                    if (clip.Frames[_index].XKey != KeyType.NotKey) _newSD.x = true;
                    if (clip.Frames[_index].YKey != KeyType.NotKey) _newSD.y = true;
                    if (clip.Frames[_index].AKey != KeyType.NotKey) _newSD.a = true;
                    break;
                case 1:
                    if (clip.Frames[_index].XKey != KeyType.NotKey) _newSD.x = true;
                    break;
                case 2:
                    if (clip.Frames[_index].YKey != KeyType.NotKey) _newSD.y = true;
                    break;
                case 3:
                    if (clip.Frames[_index].AKey != KeyType.NotKey) _newSD.a = true;
                    break;
                default:
                    Debug.LogWarning("SelectionKeyFrameAdd wrong  paraType int =" + _paraType);
                    break;
            }

            SelectionKeyFrame.Add(_newSD);
            }
        }


        public SelectedKeyData GetSelectedData(int _index)
        {
            for (int i = 0; i < SelectionKeyFrame.Count; i++)
            {
                if (SelectionKeyFrame[i].Index == _index)
                {
                    return SelectionKeyFrame[i];
                }
            }
            return null;
        }
    }

    /*
        public int GetSelectKey(int _index)
        {
            return SelectionKeyFrame[_index];
        }*/



}

public class SelectedKeyData
{
    public int Index;
    public bool all;
    public bool x;
    public bool y;
    public bool a;

    public SelectedKeyData(int _index)
    {
        Index = _index;
        x = y = a = false;
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

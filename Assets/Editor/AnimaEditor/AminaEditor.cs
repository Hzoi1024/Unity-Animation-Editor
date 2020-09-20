using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Timers;
using System;


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
    

    AminaEditorConfigSO scriptableObject;
    EditorAminaComponentClip componentClip;
    public List<AminaEditorComponetData> Components;
    List<CopyKeyData> copies;
    AminaEditorUndo editorUndo;

    GameObject protype;
    Vector2 scrollPos;
    public Vector2 scrollPosition = Vector2.zero;

    Rect LeftRootBlock;//编辑器左边部分的整体划分
    Rect RightRootBlock;
    Rect frameEditorBlock;

    float LeftRootBlockWidth = 400;
    Vector2 scrollView = Vector2.zero;

    float MiddleControlBlockWidth;
    bool isDraggingMiddleControl = false;
    bool isDraggingaminaTopBlock = false;//拖拽播放红轴


    bool isDraggingKeyFrames = false;//拖拽关键帧
    int draggingKeyFramesBase;
    int draggingKeyFramesOffset;

    bool isDraggingSelectingRect = false;
    Vector2Int SelectRectStart;
    Vector2Int SelectRectEnd;
    Rect SelectRect;
    
    float conponentClipBoxHeight = 80;//部件动画片段编辑box高度

    float topBlockHeight;
    float frameBlockWidth;//动画编辑部分一格的宽度

    float keySqureRadius=5;//帧方块半径

    int currentAminaPointerIndex = -1; //红帧指向

    bool isPlaying;
    GUIStyle NilStyle;
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
        scriptableObject = EditorGUIUtility.Load("AminaEditorConfig.asset") as AminaEditorConfigSO;
        originDisplay = OriginAminType.Defualt;
        isDraggingaminaTopBlock = false;
        isDraggingKeyFrames = false;
        isDraggingSelectingRect = false;
        lastStartUpTime = EditorApplication.timeSinceStartup;
        frameTime = 1f / 30f;
        playTex = new GUIContent(scriptableObject.PlayTex);
        PlayingTex = new GUIContent(scriptableObject.PlayingTex);
        isPlaying = false;
        NilStyle = new GUIStyle();
        NilStyle.alignment = TextAnchor.MiddleCenter;
        //NilStyle.border = new RectOffset(1,1,1,1);
        Debug.Log("onenable");
        Debug.Log("cos Pi:" + Mathf.Cos(Mathf.PI));

        GetRightBlockFrameMaxNumber();

        editorUndo = new AminaEditorUndo(this);

    }

    void OnGUI()
    {
        topBlockHeight = 30;
        Event eventCurrent = Event.current;


        OriginAminType choseOriginAminType;
        choseOriginAminType = (OriginAminType)EditorGUI.EnumPopup(
            new Rect(0, 0, 100, topBlockHeight),
            originDisplay);
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

        if(eventCurrent.button == 0 && eventCurrent.type == EventType.MouseDown)
        {
            if (playBox.Contains(eventCurrent.mousePosition))
            {
                ChangePlayBool();
                eventCurrent.Use();
            }
        }

        
        //scrollView = GUI.BeginScrollView(LeftRootBlock, scrollView, new Rect(0, 0, LeftRootBlockWidth, 300));
        //scriptableObject = AssetDatabase.LoadAssetAtPath<AminaEditorConfigSO>("Assets/Editor" + "/AminaEditorConfig.asset");
        //GUI.EndScrollView();

        DrawComponent();

        MiddleControlBlockWidth = 5;
        Rect MiddleControlBlock = new Rect(LeftRootBlockWidth, 0, MiddleControlBlockWidth, position.height);
        GUI.Box(MiddleControlBlock, "");

        EditorGUIUtility.AddCursorRect(MiddleControlBlock, MouseCursor.ResizeHorizontal);
        if (!isDraggingMiddleControl)
        {
            if (MiddleControlBlock.Contains(eventCurrent.mousePosition))
            {
                if (eventCurrent.type == EventType.MouseDown)
                {
                    isDraggingMiddleControl = true;
                }
            }
        }
        else
        {
            Rect WindowRect = new Rect(new Vector2(0, 0), position.size);
            LeftRootBlockWidth = position.width * Rect.PointToNormalized(WindowRect, eventCurrent.mousePosition).x;

            if (eventCurrent.type == EventType.MouseUp)
            {
                isDraggingMiddleControl = false;
            }

        }

        ////////////////////////
        //右边
        ////////////////////////

        //maxFrameNum = GetRightBlockFrameMaxNumber();
        frameBlockWidth = 30;
        //float _aminaWidth = position.width - LeftRootBlockWidth - MiddleControlBlockWidth;
        float _aminaWidth = maxFrameNum * frameBlockWidth;
        Rect _aminaTopBlock = new Rect(LeftRootBlockWidth + MiddleControlBlockWidth, 0, _aminaWidth, topBlockHeight);
        RightRootBlock = new Rect(_aminaTopBlock.x, 0, _aminaTopBlock.width, position.height);
        Rect _scrollViewRect = new Rect(_aminaTopBlock.x, 0, position.width - LeftRootBlockWidth - MiddleControlBlockWidth, position.height - topBlockHeight);
        Rect _scrollTrueRect = new Rect(_aminaTopBlock.x, 0, _aminaTopBlock.width, _scrollViewRect.height - 50);
        //第一个rect是 卷轴可视范围  第二个rect是卷轴内实际显示范围  如果第二个rect超过第一个则会出现卷轴
        scrollPos = GUI.BeginScrollView(_scrollViewRect, scrollPos, _scrollTrueRect, false, false);

        //EditorGUI.DrawRect(_aminaTopBlock, new Color(0.85f, 0.85f, 0.85f));

        //================
        //frameBlockWidth = _aminaWidth / 32;

        EditorGUI.DrawRect(_aminaTopBlock, new Color(0.85f, 0.85f, 0.85f));


        //画帧线
        float framelineX;

        GL.Begin(GL.LINES);
        GL.Color(new Color(0.66f, 0.66f, 0.66f));
        framelineX = RightRootBlock.x + frameBlockWidth;
        GL.Vertex(new Vector2(framelineX, RightRootBlock.y));
        GL.Vertex(new Vector2(framelineX, RightRootBlock.yMax));

        for (int i = 2; i < maxFrameNum; i++)
        {
            framelineX = RightRootBlock.x + frameBlockWidth * i;
            float _k;
            if ((i-1) % 5 == 0)
            {
                _k = 10f;
            }
            else
            {
                _k = 5f;
            }
            GL.Vertex(new Vector2(framelineX, topBlockHeight - _k));
            GL.Vertex(new Vector2(framelineX, RightRootBlock.yMax));
        }
        GL.End();

        DrawKeyFrame();


        EditorGUI.DrawRect(new Rect(frameEditorBlock.x+realMaxFrameNum*frameBlockWidth,frameEditorBlock.y, frameEditorBlock.width- realMaxFrameNum * frameBlockWidth,RightRootBlock.height),new Color(0.8f, 0.8f, 0.8f, 0.7f));


        DrawSelectRect();



        //画一条红线
        GL.Begin(GL.LINES);
        GL.Color(Color.red);

        framelineX = RightRootBlock.x + frameBlockWidth * currentAminaPointerIndex;
        //Debug.Log("framelineX"+framelineX);
        GL.Vertex(new Vector2(framelineX, RightRootBlock.y));
        GL.Vertex(new Vector2(framelineX, RightRootBlock.yMax));
        GL.End();
        

        if (isDraggingKeyFrames)
        {
            //frameEditorBlock = new Rect(_aminaTopBlock.x, topBlockHeight, _aminaTopBlock.width, conponentClipBoxHeight * Components.Count);

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
                isDraggingKeyFrames = false;

                if (draggingKeyFramesOffset != 0)
                {
                    MoveSelectedKey(draggingKeyFramesOffset);

                    draggingKeyFramesOffset = 0;
                }
            }
            //Debug.Log(frameEditPos.x);
        } else
        if (isDraggingaminaTopBlock)
        {
            float _aminaMouseXIndexF = Rect.PointToNormalized(_aminaTopBlock, eventCurrent.mousePosition).x * _aminaTopBlock.width / frameBlockWidth;
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
                //eventCurrent.Use();
                Repaint();
            }

            if (eventCurrent.rawType == EventType.MouseUp)
            {
                //wantsMouseMove = false;
                //wantsMouseEnterLeaveWindow = false;
                isDraggingaminaTopBlock = false;
            }
        }else if (isDraggingSelectingRect)
        {
            Vector2 frameEditPos = Rect.PointToNormalized(frameEditorBlock, eventCurrent.mousePosition);
            //wantsMouseMove = true;
            int _aminaMouseXIndexF = Convert.ToInt16(frameEditPos.x * _aminaTopBlock.width / frameBlockWidth);
            if (_aminaMouseXIndexF < 0)
            {
                _aminaMouseXIndexF = 0;
            }

            int _conponentIndex = Convert.ToInt16(frameEditPos.y * Components.Count);
            _conponentIndex = Mathf.Max(0, _conponentIndex);
            _conponentIndex = Mathf.Min(Components.Count, _conponentIndex);

            if(SelectRectStart.x!= _aminaMouseXIndexF)
            {
                SelectRect = new Rect(RightRootBlock.x + frameBlockWidth * SelectRectStart.x, topBlockHeight + conponentClipBoxHeight * (SelectRectStart.y), (_aminaMouseXIndexF - SelectRectStart.x) * frameBlockWidth, conponentClipBoxHeight * (_conponentIndex - SelectRectStart.y));
            }
            else
            {
                SelectRect = new Rect(RightRootBlock.x + frameBlockWidth * SelectRectStart.x-4, topBlockHeight + conponentClipBoxHeight * (SelectRectStart.y), 8, conponentClipBoxHeight * (_conponentIndex - SelectRectStart.y));
            }
            
            
            if (eventCurrent.rawType == EventType.MouseUp)
            {
                isDraggingSelectingRect = false;
                RectSelect(SelectRectStart.x, _aminaMouseXIndexF, SelectRectStart.y, _conponentIndex);

            }
            Repaint();
        }
        else
        {
            if (Components != null)
            {
                frameEditorBlock = new Rect(_aminaTopBlock.x, topBlockHeight, _aminaTopBlock.width, conponentClipBoxHeight * Components.Count);


                if (eventCurrent.type == EventType.ContextClick)
                {
                    if (frameEditorBlock.Contains(eventCurrent.mousePosition))
                    {
                        Vector2 frameEditPos = Rect.PointToNormalized(frameEditorBlock, eventCurrent.mousePosition);

                        int conponentIndex = (int)(frameEditPos.y * Components.Count);
                        //Debug.Log("conponentIndex:" + conponentIndex);
                        int _frameIndex = Convert.ToInt16(frameEditPos.x * _aminaTopBlock.width / frameBlockWidth) - 1;
                        if (_frameIndex < 0) { _frameIndex = 0; }
                        //Debug.Log("frameIndex:" + _frameIndex);
                        var menu = new GenericMenu();
                        Vector2Int _addkeyPara = new Vector2Int(conponentIndex, _frameIndex);
                        menu.AddItem(new GUIContent("Add key"), false, Addkey, _addkeyPara);
                        menu.AddItem(new GUIContent("Add key Accelerate"), false, AddkeyAccelerate, _addkeyPara);
                        menu.AddItem(new GUIContent("Add key Trigonote"), false, AddkeyTrigono, _addkeyPara);

                        if (Components[conponentIndex].clip.IsTheIndexHaveKey(_frameIndex))
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

            if (eventCurrent.button == 0 && eventCurrent.isMouse)
            {
                if (_aminaTopBlock.Contains(eventCurrent.mousePosition))
                {
                    float _aminaMouseXIndexF = Rect.PointToNormalized(_aminaTopBlock, eventCurrent.mousePosition).x * _aminaTopBlock.width / frameBlockWidth;
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

                    if(eventCurrent.type== EventType.MouseDrag)
                    {
                        isDraggingaminaTopBlock = true;
                    }
                }
                
                if (Components != null)
                {
                    if (frameEditorBlock.Contains(eventCurrent.mousePosition))
                    {
                        Vector2 frameEditPos = Rect.PointToNormalized(frameEditorBlock, eventCurrent.mousePosition);
                        int conponentIndex = (int)(frameEditPos.y * Components.Count);
                        //Debug.Log("conponentIndex:" + conponentIndex);
                        int _frameIndex = Convert.ToInt16(frameEditPos.x * _aminaTopBlock.width / frameBlockWidth) - 1;
                        if (_frameIndex < 0) { _frameIndex = 0; }

                        if (Components[conponentIndex].clip.IsTheIndexHaveKey(_frameIndex))
                        {
                            float _dY = topBlockHeight + conponentClipBoxHeight * (conponentIndex + 0.5f);
                            float _dX = RightRootBlock.x + frameBlockWidth * (_frameIndex + 1);
                            Rect _keyRect = new Rect(_dX - keySqureRadius, _dY - keySqureRadius, 2 * keySqureRadius, 2 * keySqureRadius);
                            if (_keyRect.Contains(eventCurrent.mousePosition))
                            {
                                if (!Components[conponentIndex].SelectionKeyFrameContains(_frameIndex))
                                {
                                    ClearSelectKey();
                                    draggingKeyFramesOffset = 0;
                                    Components[conponentIndex].SelectionKeyFrameAdd(_frameIndex);
                                    Repaint();
                                    eventCurrent.Use();
                                }

                                if (eventCurrent.type == EventType.MouseDrag)
                                {
                                    isDraggingKeyFrames = true;
                                    draggingKeyFramesBase = _frameIndex;
                                }
                            }
                            else
                            {
                                ClearSelectKey();

                                if (eventCurrent.type == EventType.MouseDrag)
                                {
                                    isDraggingSelectingRect = true;
                                    SelectRectStart = new Vector2Int(_frameIndex+1,Convert.ToInt16(frameEditPos.y * Components.Count));
                                }
                                

                            }
                        }
                        else
                        {
                            if (eventCurrent.type == EventType.MouseDrag)
                            {
                                isDraggingSelectingRect = true;
                                SelectRectStart = new Vector2Int(_frameIndex + 1, Convert.ToInt16(frameEditPos.y * Components.Count));
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
        
        

        //================
        //label必须放在GL后面
        for(int i = 1; i < maxFrameNum; i = i + 5)
        {
            GUI.Label(new Rect(_aminaTopBlock.x + frameBlockWidth * i, 0, 30, 25), (i-1).ToString());
        }
        
        GUI.EndScrollView();
    }

    public void Addkey(object k)
    {
        Vector2Int v2 = (Vector2Int)k;
        editorUndo.Addkey(v2,KeyType.LinearKey);
        GetRightBlockFrameMaxNumber();
    }

    public void AddkeyAccelerate(object k)
    {
        Vector2Int v2 = (Vector2Int)k;
        /*Transform _t = Components[v2.x].ComponentGO.transform;
        Vector2 _pos = _t.localPosition;
        float _ang = TransformUtils.GetInspectorRotation(_t).z;

        Components[v2.x].clip.AddKeyFrameAcceleration(_pos, _ang, v2.y);*/
        editorUndo.Addkey(v2, KeyType.AccelerateKey);
        GetRightBlockFrameMaxNumber();
    }

    public void AddkeyTrigono(object k)
    {
        Vector2Int v2 = (Vector2Int)k;
        /*Transform _t = Components[v2.x].ComponentGO.transform;
        Vector2 _pos = _t.localPosition;
        float _ang = TransformUtils.GetInspectorRotation(_t).z;

        Components[v2.x].clip.AddKeyFrameAcceleration(_pos, _ang, v2.y);*/
        editorUndo.Addkey(v2, KeyType.TrigonoKey);
        GetRightBlockFrameMaxNumber();
    }

    public void DeleteKey(object k)
    {
        Vector2Int v2 = (Vector2Int)k;
        editorUndo.DeleteKey(v2);
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
                for (int j = 0; j < Components[i].SelectionKeyFrameCount; j++)
                {
                    int k = Components[i].GetSelectKey(j);

                    if (_offset == -10) { _offset = k; } else { _offset = Mathf.Min(k, _offset); }

                    CopyKeyData _ckd = new CopyKeyData();
                    _ckd.componentIndex = i;
                    _ckd.offset = k;
                    _ckd.frame = new AminaFrame();
                    _ckd.frame.pos = new Vector2(Components[i].clip.frames[k].pos.x, Components[i].clip.frames[k].pos.y);
                    _ckd.frame.angle = Components[i].clip.frames[k].angle;

                    _cs.Add(_ckd);
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

    void DrawComponent()
    {
        conponentClipBoxHeight = 30;
        float labelWidth = 200;
        if (Components != null)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                float _col = (float)(0.9 - 0.06 * (i % 2));
                EditorGUI.DrawRect(new Rect(0, topBlockHeight + conponentClipBoxHeight * i, position.width, conponentClipBoxHeight), new Color(_col, _col, _col));
                //GUI.Box(new Rect(0, topBlockHeight, LeftRootBlockWidth, conponentClipBoxHeight), new GUIContent("11111111obj", scriptableObject.HeadSprite));

                GUI.Label(new Rect(20, topBlockHeight + conponentClipBoxHeight * i, labelWidth, conponentClipBoxHeight), Components[i].labelStr);
                
            }
        }
        
    }

    void DrawKeyFrame()
    {
        keySqureRadius = 5;

        if (Components != null)
        {

            for (int i = 0; i < Components.Count; i++)
            {
                float _dY = topBlockHeight + conponentClipBoxHeight * (i + 0.5f);

                for (int j = 0; j < Components[i].clip.keys.Count; j++)
                {
                    EditorAminaComponentClip.Key _key = Components[i].clip.keys[j];

                    if (Components[i].SelectionKeyFrameContains(_key.index))
                    {
                        Color _col= new Color();
                        //Debug.Log("select key"+ _key);
                        switch (_key.type)
                        {
                            case KeyType.LinearKey:
                                _col = Color.blue;
                                break;
                            case KeyType.AccelerateKey:
                                _col = new Color(0.4f, 0.8f, 0.4f);
                                break;
                            case KeyType.TrigonoKey:
                                _col = new Color(0.3f,0.7f,0.7f);
                                break;
                        }

                        float _dX = RightRootBlock.x + frameBlockWidth * (_key.index + 1+draggingKeyFramesOffset);
                        EditorGUI.DrawRect(new Rect(_dX - keySqureRadius, _dY - keySqureRadius, 2 * keySqureRadius, 2 * keySqureRadius), _col);
                    }
                    else
                    {
                        Color _col= new Color();
                        //Debug.Log("select key"+ _key);
                        switch (_key.type)
                        {
                            case KeyType.LinearKey:
                                _col = Color.black;
                                break;
                            case KeyType.AccelerateKey:
                                _col = new Color(0.4f, 0.2f, 0);
                                break;
                            case KeyType.TrigonoKey:
                                _col = new Color(0.3f, 0.0f, 0.0f);
                                break;
                        }
                        float _dX = RightRootBlock.x + frameBlockWidth * (_key.index + 1);
                        EditorGUI.DrawRect(new Rect(_dX - keySqureRadius, _dY - keySqureRadius, 2 * keySqureRadius, 2 * keySqureRadius), _col);
                    }
                    
                    //Debug.Log("keyframe" + Components[i].clip.keyNum[j]);
                }

                


                //RightRootBlock.x + frameBlockWidth * i
            }
        }
    }

    void DrawSelectRect()
    {
        if (isDraggingSelectingRect)
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
            int _count = Components[i].clip.keys.Count;

            for(int j = 0; j < _count; j++)
            {
                int k = Components[i].clip.keys[j].index;
                if (k >= _x1)
                {
                    if (k > _x2) break;
                    Components[i].SelectionKeyFrameAdd(k);
                }
            }
        }

    }

    void GetRightBlockFrameMaxNumber()
    {
        int _max = 0;

        if (Components != null)
        {
            for (int i = 0; i < Components.Count; i++)
            {
                _max = Mathf.Max(_max, Components[i].clip.framesCount);
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
                if (Components[i].clip.framesCount > _index)
                {
                    AminaFrame _af = Components[i].clip.frames[_index];

                    Components[i].ComponentGO.transform.localPosition = _af.pos;
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
            if (_aecd.SelectionKeyFrameCount > 0)
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

}

public class AminaEditorComponetData
{
    public GameObject ComponentGO;
    public int componentID;
    public EditorAminaComponentClip clip;
    public string labelStr;
    public List<int> SelectionKeyFrame;
    public int SelectionKeyFrameCount { get { return SelectionKeyFrame.Count; } }

    public AminaEditorComponetData(int _componentID, GameObject protype)
    {
        componentID = _componentID;

        switch (_componentID)
        {
            case 1://头                    
                ComponentGO = protype.transform.Find("Sp/Head").gameObject;
                labelStr = "Head";
                break;
            case 2://身体
                ComponentGO = protype.transform.Find("Sp/Body").gameObject;
                labelStr = "Body";
                break;
            case 3://前手
                ComponentGO = protype.transform.Find("Sp/FHand").gameObject;
                labelStr = "FHand";
                break;
            case 4://后手
                ComponentGO = protype.transform.Find("Sp/BHand").gameObject;
                labelStr = "BHand";
                break;
            case 5://前脚
                ComponentGO = protype.transform.Find("Sp/FLeg").gameObject;
                labelStr = "FLeg";
                break;
            case 6://后脚
                ComponentGO = protype.transform.Find("Sp/BLeg").gameObject;
                labelStr = "BLeg";
                break;

        }
        SelectionKeyFrame = new List<int>();
        //clip = new EditorAminaComponentClip();
        clip = ScriptableObject.CreateInstance<EditorAminaComponentClip>();
    }

    public void ClearSelectionKeyFrame()
    {
        SelectionKeyFrame = new List<int>();
    }

    public void SelectionKeyFrameAdd(int k)
    {
        SelectionKeyFrame.Add(k);
    }

    public bool SelectionKeyFrameContains(int k)
    {
        return SelectionKeyFrame.Contains(k);
    }

    public int GetSelectKey(int _index)
    {
        return SelectionKeyFrame[_index];
    }

}

public class CopyKeyData
{
    public int offset;
    public int componentIndex;
    public KeyType type;
    public AminaFrame frame;
}

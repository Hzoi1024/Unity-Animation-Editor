using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Timers;

public partial class AminaEditor : EditorWindow
{
    void OnGUI()
    {
        topBlockHeight = 30;
        Event eventCurrent = Event.current;

        OriginAminType choseOriginAminType = (OriginAminType)EditorGUI.EnumPopup(new Rect(0, 0, 100, topBlockHeight), originDisplay);
        if (choseOriginAminType != originDisplay)
        {
            ProtypeInit(choseOriginAminType);
        }
        originDisplay = choseOriginAminType;

        LeftRootBlock = new Rect(0, topBlockHeight, LeftRootBlockWidth, position.height);


        //帧动画特殊配置
        if (choseOriginAminType == OriginAminType.SpriteFrames)
        {
            AtlasGUILeft();
        }


        Rect playBox = new Rect(LeftRootBlockWidth - 60, 3, 24, 24);

        if (isPlaying)
        {
            GUI.Box(playBox, PlayingTex, NilStyle);
        }
        else
        {
            GUI.Box(playBox, playTex, NilStyle);
        }

        Rect clipButton = new Rect(120, 8, 16, 16);

        if (GUI.Button(clipButton, ""))// 左上下拉右边那个按钮
        {
            switch (originDisplay)
            {
                case OriginAminType.Human:
                    EditorAminaClipLoadWindow clipLoadWindow = GetWindow<EditorAminaClipLoadWindow>(false, "Load Clip", true);
                    clipLoadWindow.minSize = new Vector2(250, 250);
                    clipLoadWindow.position = new Rect(clipButton.xMax, position.y, 250, 250);
                    clipLoadWindow.Init(this, originDisplay);
                    break;
                case OriginAminType.SpriteFrames:
                    EditorAtlasWindow atlasLoadWindow = GetWindow<EditorAtlasWindow>(false, "Load Atlas", true);
                    atlasLoadWindow.minSize = new Vector2(250, 250);
                    atlasLoadWindow.position = new Rect(clipButton.xMax, position.y, 250, 250);
                    atlasLoadWindow.Init(this);
                    break;
                default: break;
            }


            if (originDisplay != OriginAminType.Defualt)
            {

            }
        }

        if (clip != null)
        {
            if (GUI.Button(new Rect(clipButton.xMax, 0, playBox.x - clipButton.xMax, topBlockHeight), clip.name, LabelStyle))
            {
                Selection.activeObject = clip;
            }
        }

        float _scrollHeight = 0;

        if (Components != null)
        {
            _scrollHeight = (Components.Count + componentParaCount) * conponentClipBoxHeight;
        }


        Rect _leftEditBlock = new Rect(0, topBlockHeight, LeftRootBlockWidth, position.height - topBlockHeight - 15);


        leftScrollPos = new Vector2(0, rightScrollPos.y);
        leftScrollPos = GUI.BeginScrollView(_leftEditBlock, leftScrollPos, new Rect(0, topBlockHeight, LeftRootBlockWidth - 20, _scrollHeight));

        DrawLeftComponent();
        if (eventCurrent.button == 0 && eventCurrent.type == EventType.MouseDown)
        {
            if (Components != null && Components.Count > 0)
            {
                LeftComponentEditBlock = new Rect(0, topBlockHeight, LeftRootBlock.width, conponentClipBoxHeight * (Components.Count + componentParaCount));

                if (LeftComponentEditBlock.Contains(eventCurrent.mousePosition))
                {
                    Vector2 _leftEditPos = Rect.PointToNormalized(LeftComponentEditBlock, eventCurrent.mousePosition);

                    int _leftIndex = (int)(_leftEditPos.y * (Components.Count + componentParaCount));

                    int _i = GetComponentIndex(_leftIndex);
                    Selection.activeGameObject = Components[_i].ComponentGO;
                }
                eventCurrent.Use();
            }

            if (atlas != null)
            {
                Rect leftAtlasBlock = new Rect(0, topBlockHeight + conponentClipBoxHeight * (Components.Count + componentParaCount), LeftRootBlock.width, conponentClipBoxHeight* (atlasParaCount+1));
                if (leftAtlasBlock.Contains(eventCurrent.mousePosition))
                {
                    Selection.activeGameObject = atlas.GetAtlasGameObject();
                    
                    if (atlas.showAtlasPara && atlas.isFace)
                    {
                        Rect _faceBlock = new Rect(leftAtlasBlock.x, leftAtlasBlock.yMax - conponentClipBoxHeight, leftAtlasBlock.width, conponentClipBoxHeight);
                        if (_faceBlock.Contains(eventCurrent.mousePosition))
                        {
                            Selection.activeGameObject = atlas.GetAtlasFace();
                        }
                    }
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
        aminaTopBlock = new Rect(LeftRootBlockWidth + MiddleControlBlockWidth, 0, _aminaWidth, topBlockHeight);
        RightRootBlock = new Rect(aminaTopBlock.x, 0, aminaTopBlock.width, position.height);
        Rect _scrollViewRect = new Rect(aminaTopBlock.x, topBlockHeight, position.width - LeftRootBlockWidth - MiddleControlBlockWidth, position.height - topBlockHeight);
        Rect _scrollTrueRect = new Rect(aminaTopBlock.x, topBlockHeight, aminaTopBlock.width, _scrollHeight);
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
        }
        else if (eventCurrent.button == 0 && eventCurrent.isMouse)
        {

            if (eventCurrent.type == EventType.MouseDown && playBox.Contains(eventCurrent.mousePosition))
            {
                ChangePlayBool();
                eventCurrent.Use();
            }

            if (aminaTopBlock.Contains(eventCurrent.mousePosition))
            {
                float _aminaMouseXIndexF = (Rect.PointToNormalized(aminaTopBlock, eventCurrent.mousePosition).x * aminaTopBlock.width + rightScrollPos.x) / frameBlockWidth;
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
            }
            else if (MiddleControlBlock.Contains(eventCurrent.mousePosition))
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
        ///

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
                    int _aminaMouseXIndexF = Convert.ToInt16(frameEditPos.x * aminaTopBlock.width / frameBlockWidth) - 1;
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
                    float _aminaMouseXIndex = Rect.PointToNormalized(aminaTopBlock, eventCurrent.mousePosition).x * aminaTopBlock.width / frameBlockWidth;
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
                    int _aminaMouseXIndexFR = Convert.ToInt16(_frameEditPos.x * aminaTopBlock.width / frameBlockWidth);
                    if (_aminaMouseXIndexFR < 0)
                    {
                        _aminaMouseXIndexFR = 0;
                    }

                    int _conponentIndex = Convert.ToInt16(_frameEditPos.y * (Components.Count + componentParaCount));
                    _conponentIndex = Mathf.Max(0, _conponentIndex);
                    _conponentIndex = Mathf.Min(Components.Count + componentParaCount, _conponentIndex);

                    if (SelectRectStart.x != _aminaMouseXIndexFR)
                    {
                        SelectRect = new Rect(RightRootBlock.x + frameBlockWidth * SelectRectStart.x, 
                            topBlockHeight + conponentClipBoxHeight * (SelectRectStart.y), 
                            (_aminaMouseXIndexFR - SelectRectStart.x) * frameBlockWidth, 
                            conponentClipBoxHeight * (_conponentIndex - SelectRectStart.y));
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
                case 5://拖拽pause
                    float _aminaMouseXIndex5 = Rect.PointToNormalized(aminaTopBlock, eventCurrent.mousePosition).x * aminaTopBlock.width / frameBlockWidth;
                    int newAminaPointerIndex5 = Convert.ToInt16(_aminaMouseXIndex5);
                    //Debug.Log("currentAminaPointerIndex:" + currentAminaPointerIndex);
                    if (newAminaPointerIndex5 < 1)
                    {
                        newAminaPointerIndex5 = 1;
                    }

                    tempPauseTime[1] = newAminaPointerIndex5 - 1;

                    if (eventCurrent.rawType == EventType.MouseDown)
                    {
                        //isDraggingSelectingRect = false;
                        AddPause(newAminaPointerIndex5);
                        DraggingOff();

                    }
                    Repaint();
                    break;
                case 6://拖拽outPut
                    float _aminaMouseXIndex6 = Rect.PointToNormalized(aminaTopBlock, eventCurrent.mousePosition).x * aminaTopBlock.width / frameBlockWidth;
                    int newAminaPointerIndex6 = Convert.ToInt16(_aminaMouseXIndex6) - 1;
                    if (newAminaPointerIndex6 < 0)
                    {
                        newAminaPointerIndex6 = 0;
                    }
                    outPutOffset = newAminaPointerIndex6 - outPutSelect.y;

                    if (eventCurrent.rawType == EventType.MouseUp)
                    {
                        MoveOutput(outPutSelect.x, outPutSelect.y, newAminaPointerIndex6);
                        outPutOffset = 0;
                        outPutSelect = new Vector2Int(-1, -1);
                        DraggingOff();
                    }
                    Repaint();

                    break;
                case 101://拖拽关键帧
                    Vector2 frameEditPos101 = Rect.PointToNormalized(frameEditorBlock, eventCurrent.mousePosition);
                    //wantsMouseMove = true;
                    int _aminaMouseXIndexF101 = Convert.ToInt16(frameEditPos101.x * aminaTopBlock.width / frameBlockWidth) - 1;
                    if (_aminaMouseXIndexF101 < 0)
                    {
                        _aminaMouseXIndexF101 = 0;
                    }

                    draggingKeyFramesOffset = _aminaMouseXIndexF101 - draggingKeyFramesBase;

                    Repaint();

                    if (eventCurrent.rawType == EventType.MouseUp)
                    {
                        //isDraggingKeyFrames = false;

                        if (draggingKeyFramesOffset != 0)
                        {
                            MoveSelectedKey_Atlas(draggingKeyFramesOffset);

                            draggingKeyFramesOffset = 0;
                        }

                        DraggingOff();
                    }
                    break;
                case 103://拖拽方形选择框
                    Debug.Log("drag 103");
                    Vector2 _frameEditPos103 = Rect.PointToNormalized(frameEditorBlock, eventCurrent.mousePosition);
                    //wantsMouseMove = true;
                    int _aminaMouseXIndexFR103 = Convert.ToInt16(_frameEditPos103.x * aminaTopBlock.width / frameBlockWidth);
                    if (_aminaMouseXIndexFR103 < 0)
                    {
                        _aminaMouseXIndexFR103 = 0;
                    }

                    int _conponentIndex103 = Convert.ToInt16(_frameEditPos103.y * (1+atlasParaCount));
                    _conponentIndex103 = Mathf.Max(0, _conponentIndex103);
                    _conponentIndex103 = Mathf.Min(1 + atlasParaCount, _conponentIndex103);

                    if (SelectRectStart.x != _aminaMouseXIndexFR103)
                    {
                        SelectRect = new Rect(RightRootBlock.x + frameBlockWidth * SelectRectStart.x, 
                            topBlockHeight + conponentClipBoxHeight * (SelectRectStart.y), 
                            (_aminaMouseXIndexFR103 - SelectRectStart.x) * frameBlockWidth, 
                            conponentClipBoxHeight * (_conponentIndex103 - SelectRectStart.y));
                    }
                    else
                    {
                        SelectRect = new Rect(RightRootBlock.x + frameBlockWidth * SelectRectStart.x - 4, 
                            topBlockHeight + conponentClipBoxHeight * (SelectRectStart.y), 8, 
                            conponentClipBoxHeight * (_conponentIndex103 - SelectRectStart.y));
                    }


                    if (eventCurrent.rawType == EventType.MouseUp)
                    {
                        //isDraggingSelectingRect = false;
                        RectSelect(SelectRectStart.x, _aminaMouseXIndexFR103, SelectRectStart.y, _conponentIndex103);
                        DraggingOff();

                    }
                    Repaint();
                    break;
                    
                default:
                    if (draggingControl != 4)
                    {
                        Debug.LogError("undifined draggingControl=" + draggingControl);
                        DraggingOff();
                    }
                    break;

            }
        }
        else
        {




            if (originDisplay== OriginAminType.Human)
            {
                frameEditorBlock = new Rect(aminaTopBlock.x, topBlockHeight, aminaTopBlock.width, conponentClipBoxHeight * (Components.Count + componentParaCount));

                if (eventCurrent.type == EventType.MouseDown && eventCurrent.button == 1)
                {
                    Spine_Mouse1(eventCurrent);
                }

                if (eventCurrent.button == 0 && eventCurrent.isMouse)
                {
                    Spine_Mouse0(eventCurrent);
                }
            }

            if(originDisplay== OriginAminType.SpriteFrames)
            {
                frameEditorBlock = new Rect(aminaTopBlock.x, topBlockHeight, aminaTopBlock.width, conponentClipBoxHeight * (atlasParaCount+1));

                
                if (!_scrollViewRect.Contains(eventCurrent.mousePosition))
                {
                    isAtlasDragSprites = false;
                    atlas.ClearTempKeys();
                }

                if (eventCurrent.button == 0 && eventCurrent.isMouse)
                {
                    Atlas_Mouse0(eventCurrent);
                }
                
                
                //List<Sprite> _dragSprites = AtlasDragSprites(eventCurrent, RightRootBlock);
                if (AtlasMouseDragSprites(eventCurrent))
                {
                    if (AtlasDragSprites != null && AtlasDragSprites.Count > 0)
                    {
                        for (int i = 0; i < AtlasDragSprites.Count; i++)
                        {
                            Debug.LogWarning(AtlasDragSprites[i].name);

                        }
                        
                    }
                }
                
            }

            if (eventCurrent.button == 0 && eventCurrent.isMouse)
            {
                if (atlas != null && eventCurrent.type == EventType.MouseDown)
                {
                    Rect atlasEditorBlock = new Rect(aminaTopBlock.x, topBlockHeight + conponentClipBoxHeight * (Components.Count + componentParaCount), aminaTopBlock.width, conponentClipBoxHeight);
                    if (atlasEditorBlock.Contains(eventCurrent.mousePosition))
                    {
                        //int _frameIndex = Convert.ToInt16(Rect.PointToNormalized(atlasEditorBlock, eventCurrent.mousePosition).x * _aminaTopBlock.width / frameBlockWidth) - 1;
                        
                        Vector2 frameEditPos = Rect.PointToNormalized(frameEditorBlock, eventCurrent.mousePosition);
                        int conponentIndex = (int)(frameEditPos.y * (Components.Count + componentParaCount));
                        //Debug.Log("conponentIndex:" + conponentIndex);
                        int _frameIndex = Convert.ToInt16(frameEditPos.x * aminaTopBlock.width / frameBlockWidth) - 1;
                        /*
                        SpriteFrame _s = atlas.GetSpriteFrame(_frameIndex);
                        if (_s != null)
                        {
                            spriteFrameContainer.sprite = _s;
                            Selection.activeObject = spriteFrameContainer;
                        }*/
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
        EditorGUI.DrawRect(aminaTopBlock, new Color(0.85f, 0.85f, 0.85f));
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
                GL.Vertex(new Vector2(framelineX, RightRootBlock.yMax - 15));
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
            float _labelLeft = aminaTopBlock.x + frameBlockWidth * i - rightScrollPos.x;
            if (_labelLeft > RightRootBlock.x)
            {
                GUI.Label(new Rect(_labelLeft, 0, 30, 25), (i - 1).ToString());
            }

        }
        //================
        //label必须放在GL后面



        if (isShowWindow)//内置窗口
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
                case 3:
                    // All GUI.Window or GUILayout.Window must come inside here
                    TrigonoInputWindowRect = GUILayout.Window(3, TrigonoInputWindowRect, SetAdvanceEndWindow, "Set AdvanceEnd");
                    break;
                default:
                    Debug.LogWarning("WindowControl wrong ! WindowControl=" + WindowControl);
                    isShowWindow = false;
                    break;
            }


            EndWindows();
        }

    }
}

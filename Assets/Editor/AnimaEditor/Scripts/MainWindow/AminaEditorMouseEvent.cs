using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Timers;

/// <summary>
/// 鼠标事件部分
/// </summary>
public partial class AminaEditor : EditorWindow
{
    /// <summary>
    /// 骨骼动画点击鼠标左键
    /// </summary>
    public void Spine_Mouse0(Event eventCurrent)
    #region
    {
        if (frameEditorBlock.Contains(eventCurrent.mousePosition))
        {//点击关键帧
            Vector2 frameEditPos = Rect.PointToNormalized(frameEditorBlock, eventCurrent.mousePosition);
            int conponentIndex = (int)(frameEditPos.y * (Components.Count + componentParaCount));
            //Debug.Log("conponentIndex:" + conponentIndex);
            int _frameIndex = Convert.ToInt16(frameEditPos.x * aminaTopBlock.width / frameBlockWidth) - 1;

            Vector3Int _para = GetKeyVec(conponentIndex, _frameIndex);

            if (_frameIndex < 0) { _frameIndex = 0; }

            bool _next = true;
            if (Components[_para.x].clip.OutData.Count > 0)
            {
                for (int m = 0; m < Components[_para.x].clip.OutData.Count; m++)
                {
                    if (Components[_para.x].clip.OutData[m].Index == _frameIndex)
                    {
                        float _dY = topBlockHeight + conponentClipBoxHeight * conponentIndex + 2;
                        float _dX = RightRootBlock.x + frameBlockWidth * (_frameIndex + 1) - 6;

                        Rect _outRect = new Rect(_dX, _dY, 12, 6);

                        if (_outRect.Contains(eventCurrent.mousePosition))
                        {
                            outPutSelect = new Vector2Int(_para.x, _para.y);
                            DraggingOn(6);
                        }
                        //FrameOutputData _newOutData = new FrameOutputData(Components[_para.x].clip.OutData[m]);
                        outputDataEditorContainer.outputData = Components[_para.x].clip.OutData[m];
                        Selection.activeObject = outputDataEditorContainer;

                        _next = false;
                        break;
                    }
                }
            }

            if (_next)
            {
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
                        if (eventCurrent.type == EventType.MouseDown && !Components[_para.x].SelectionKeyContained(_frameIndex, _para.z))
                        {
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
                            SelectRectStart = new Vector2Int(_frameIndex + 1, Convert.ToInt16(frameEditPos.y * (Components.Count + componentParaCount)));
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
    #endregion

    /// <summary>
    /// 骨骼动画点击鼠标右键
    /// </summary>
    public void Spine_Mouse1(Event eventCurrent)
    #region
    {
        if (frameEditorBlock.Contains(eventCurrent.mousePosition))
        {
            Vector2 frameEditPos = Rect.PointToNormalized(frameEditorBlock, eventCurrent.mousePosition);

            int conponentIndex = (int)(frameEditPos.y * (Components.Count + componentParaCount));
            //Debug.Log("conponentIndex:" + conponentIndex);
            int _frameIndex = Convert.ToInt16(frameEditPos.x * aminaTopBlock.width / frameBlockWidth) - 1;
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
            menu.AddItem(new GUIContent("Add Pause"), false, AddPauseOrder, _addkeyPara);
            menu.AddItem(new GUIContent("Set AdvanceEnd"), false, CallSetAdvanceEndWindow);
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
            if (Components[_addkeyPara.x].clip.PauseTime != null)
            {
                menu.AddItem(new GUIContent("Delete Pause"), false, DelPause, _addkeyPara.x);
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Delete Pause"), false);
            }
            bool _isCanAddOut = true;
            if (Components[_addkeyPara.x].clip.OutData.Count > 0)
            {
                for (int i = 0; i < Components[_addkeyPara.x].clip.OutData.Count; i++)
                {
                    if (Components[_addkeyPara.x].clip.OutData[i].Index == _addkeyPara.y)
                    {
                        _isCanAddOut = false;
                        break;
                    }
                }
            }
            if (_isCanAddOut)
            {
                menu.AddItem(new GUIContent("Add Output"), false, AddOut, _addkeyPara);
            }
            else
            {
                menu.AddItem(new GUIContent("Delete Output"), false, DelOut, _addkeyPara);
            }
            menu.ShowAsContext();
            eventCurrent.Use();
        }
    }
    #endregion

    /// <summary>
    /// 帧动画点击鼠标左键
    /// </summary>
    public void Atlas_Mouse0(Event eventCurrent)
    {
        if (frameEditorBlock.Contains(eventCurrent.mousePosition))
        {//点击关键帧
            Vector2 frameEditPos = Rect.PointToNormalized(frameEditorBlock, eventCurrent.mousePosition);
            int _indexY = (int)(frameEditPos.y * (atlasParaCount+1));
            //Debug.Log("conponentIndex:" + conponentIndex);
            int _frameIndex = Convert.ToInt16(frameEditPos.x * aminaTopBlock.width / frameBlockWidth) - 1;
            Debug.Log(_frameIndex + "," + _indexY);

            if (_frameIndex < 0) { _frameIndex = 0; }

            bool _next = true;
            if (atlas.outputEvent.Count > 0)
            {
                for (int m = 0; m < atlas.outputEvent.Count; m++)
                {
                    if (atlas.outputEvent[m].Index == _frameIndex)
                    {
                        float _dY = topBlockHeight + conponentClipBoxHeight * _indexY + 2;
                        float _dX = RightRootBlock.x + frameBlockWidth * (_frameIndex + 1) - 6;

                        Rect _outRect = new Rect(_dX, _dY, 12, 6);

                        if (_outRect.Contains(eventCurrent.mousePosition))
                        {
                            outPutSelect = new Vector2Int(0, _frameIndex);
                            DraggingOn(106);
                        }
                        //FrameOutputData _newOutData = new FrameOutputData(Components[_para.x].clip.OutData[m]);
                        outputDataEditorContainer.outputData_atlas = atlas.outputEvent[m];
                        Selection.activeObject = outputDataEditorContainer;

                        _next = false;
                        break;
                    }
                }
            }

            if (_next)
            {
                if (atlas.IsHaveIndexKey(_frameIndex, _indexY))
                {
                    float _dY = topBlockHeight + conponentClipBoxHeight * (_indexY + 0.5f);
                    float _dX = RightRootBlock.x + frameBlockWidth * (_frameIndex + 1);
                    Rect _keyRect = new Rect(_dX - keySqureRadius, _dY - keySqureRadius, 2 * keySqureRadius, 2 * keySqureRadius);
                    if (_keyRect.Contains(eventCurrent.mousePosition))
                    {
                        if (eventCurrent.type == EventType.MouseDown && !atlas.IsSelectionKeyContained(_frameIndex, _indexY))
                        {
                            ClearSelectKey_atlas();
                            draggingKeyFramesOffset = 0;
                            atlas.SelectionKeyFrameAdd(_frameIndex, _indexY);
                            Repaint();
                            eventCurrent.Use();
                        }


                        if (eventCurrent.type == EventType.MouseDrag)
                        {
                            //isDraggingKeyFrames = true;
                            DraggingOn(101);
                            draggingKeyFramesBase = _frameIndex;
                        }
                    }
                    else
                    {
                        ClearSelectKey_atlas();

                        if (eventCurrent.type == EventType.MouseDrag)
                        {
                            //isDraggingSelectingRect = true;
                            DraggingOn(103);
                            SelectRectStart = new Vector2Int(_frameIndex + 1, Convert.ToInt16(frameEditPos.y * (atlasParaCount+1)));
                        }
                    }
                    
                }
                else
                {
                    if (eventCurrent.type == EventType.MouseDrag)
                    {
                        //isDraggingSelectingRect = true;
                        DraggingOn(103);
                        SelectRectStart = new Vector2Int(_frameIndex + 1, Convert.ToInt16(frameEditPos.y * (atlasParaCount + 1)));
                        //SelectRectEnd = new Vector2Int(conponentIndex, _frameIndex);
                    }
                    ClearSelectKey_atlas();
                }
            }
        }
    }


}

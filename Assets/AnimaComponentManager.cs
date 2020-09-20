using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimaComponentManager
{

    Transform transform;

    AminaComponentClipData currentClip;

    int pointer;
    int maxPointer;
    int blankLeft;
    bool isLoop;

    float tranDeltaAng;

    /// <summary>
    /// 帧刷新
    /// </summary>
    public void UpdateFrame()
    {
        if (blankLeft <= 0)
        {
            if(pointer== maxPointer)
            {
                if (isLoop)
                {
                    pointer = 0;
                }
                else
                {
                    pointer--;
                }
            }

            transform.localPosition = currentClip.frames[pointer].pos;
            transform.localEulerAngles = new Vector3(0,0, currentClip.frames[pointer].angle);
            pointer++;
        }
        else
        {
            transform.localPosition = Vector2.Lerp(transform.localPosition, currentClip.frames[0].pos, 1 / blankLeft);
            //transform.localEulerAngles = Vector2.Lerp(transform.localEulerAngles, currentClip.frames[0].angle, 1 / blankLeft);
            transform.localEulerAngles = new Vector3(0, 0,transform.localEulerAngles.z+ tranDeltaAng);
            blankLeft--;
        }
    }

    public void SetNewClip(AminaComponentClipData newClip,int blank,bool isLoopPlay)
    {
        blankLeft = blank;
        maxPointer = newClip.frames.Length;
#if UNITY_EDITOR
        if (maxPointer == 0)
        {
            Debug.Log("max pointer=0");
        }

#endif
        if (blank != 0)
        {
            tranDeltaAng = (newClip.frames[0].angle - currentClip.frames[pointer].angle) / blank;
        }
        

        pointer = 0;
        currentClip = newClip;
        isLoop = isLoopPlay;
    }


    






}

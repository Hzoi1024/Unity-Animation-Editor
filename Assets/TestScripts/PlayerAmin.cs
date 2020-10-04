using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAmin : MonoBehaviour
{
    public GameObject Head;

    AminaComponentClipData headClip;

    float frameTime;
    float DeltaTime;

    int frameIndex;
    int maxIndex;
    // Start is called before the first frame update
    void Start()
    {
        headClip = FramLoadManager.Instance.GetAminaFrame(1001);
        Debug.Log(headClip.Event[0].Index);
        frameTime = 1f / 30;
        DeltaTime = 0f;
        frameIndex = 0;
        maxIndex = headClip.frames.Length;
    }

    // Update is called once per frame
    void Update()
    {
        
        DeltaTime += Time.deltaTime;
        if (DeltaTime > frameTime)
        {
            var frame = headClip.frames[frameIndex];
            Head.transform.localPosition = new Vector2(frame.x, frame.y);
            Head.transform.localEulerAngles = new Vector3(0,0,frame.angle);

            frameIndex++;
            if (frameIndex == maxIndex)
            {
                frameIndex = 0;
            }
            DeltaTime -= frameTime;
        }
        


    }
}

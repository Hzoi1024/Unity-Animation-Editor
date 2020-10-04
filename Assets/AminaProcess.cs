using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayType
{
    once=0,
    cycle=1,
}

public class AminaProcess
{
    private List<AminaFrame> afi;

    public int TotalFrames;

    public GameObject obj;
    


    private int Pointer;

    public AminaProcess() { Pointer = 0;}



    public void AminaPlayNext()
    {
        if (Pointer < afi.Count)
        {
            obj.transform.localPosition = new Vector2(afi[Pointer].x, afi[Pointer].y);
            obj.transform.localEulerAngles = new Vector3(0,0,afi[Pointer].angle);
            
        }
    }

}

public class AminaFrameInfoContainer
{
    public AminaFrame info;
    public int frameNum;
}

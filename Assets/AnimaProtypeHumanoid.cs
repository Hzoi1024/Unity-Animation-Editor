using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimaProtypeHumanoid : MonoBehaviour, IAnimaProtype
{
    Transform Body;//1
    Transform Head;//2
    Transform ForeHand;//3
    Transform BackHand;//4
    Transform ForeLeg;//5
    Transform BackLeg;//6

    public int ProtypeID { get { return 1; } }

    public void Awake()
    {
        //transform.Find()
    }
    

    public void SetAnimaAciton(AnimaActionData newAnimaAction)
    {

    }

}

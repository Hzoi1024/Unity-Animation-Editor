using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimaProtype
{
    int ProtypeID { get; }
    void SetAnimaAciton(AnimaActionData newAnimaAction);
}

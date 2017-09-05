using UnityEngine;
using System.Collections;

public class UIElementPivot : MonoBehaviour {

    public Transform DealerPivot;
    public UITweener DealerTween;
    public UIPlayAnimation DealerAnim;

    public void MoveDealer(bool b)
    {
        if (b)
            DealerTween.PlayReverse();
        else
            DealerTween.PlayForward();
    }

    public void CorrectDealerRotation()
    {
        //Debug.Log(string.Format("dealer rotation: {0}", DealerPivot.rotation.eulerAngles));
        float zRot = DealerPivot.rotation.eulerAngles.z;
        if (zRot != 270f)  // for some reason the tween won't go right back to 0
        {
            DealerPivot.rotation = Quaternion.identity;
        }
    }
}

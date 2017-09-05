using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TNet;

public class HandResult : TNBehaviour {
    public static HandResult instance;
    public UITweener mainTween;
    public UITweener backgroundTween;
    public UITweener resultTween;
    public UILabel resultLabel;

    public UIPieChart pieChart;

    private string mHandResult;
    public string HandResultString
    {
        get { return mHandResult; }
        set { tno.Send("OnSetHandResult", Target.AllSaved, value); }
    }

    private string mPlayerResults;
    public string PlayerResultString
    {
        get { return mPlayerResults; }
        set { tno.Send("OnSetPlayerResults", Target.AllSaved, value); }
    }
    public void RevealHand(string result)
    {
        HandResultString = result;
        tno.Send("ShowOrHide", Target.AllSaved, true);
    }

    public void RevealHand(string[] labels, float[] values)
    {
        tno.Send("OnSetPieValues", Target.All, labels, values);
        tno.Send("ShowOrHide", Target.AllSaved, true);
    }

    public void ResetResult()
    {
        HandResultString = "";
        PlayerResultString = "";
        HandResult.instance.tno.Send("ShowOrHide", Target.All, false);
        //ShowOrHide(false);
    }
    [RFC]
    public void OnSetHandResult(string r)
    {
        mHandResult = r;
    }

    [RFC]
    public void OnSetPlayerResults(string r)
    {
        mPlayerResults = r;
    }

    [RFC]
    public void OnSetPieValues(string[] labels, float[] values)
    {
        pieChart.PieValues = GetPieResults(labels, values);
    }
    private System.Collections.Generic.List<UIPieChart.UIPieValue> GetPieResults(string[] labels, float[] values)
    {
        var newList = new System.Collections.Generic.List<UIPieChart.UIPieValue>();
        for (int lcv = 0; lcv < labels.Length; lcv++ )
        {
            newList.Add(new UIPieChart.UIPieValue(labels[lcv], values[lcv]));
        }
        return newList;
    }

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        ResetResult();
    }
    [RFC]
    public void ShowOrHide(bool show)
    {
        if (show)
        {
            backgroundTween.PlayForward();
            mainTween.PlayForward();
            pieChart.UpdateChart();
        }
        else
        {
            backgroundTween.PlayReverse();
            mainTween.PlayReverse();
            resultTween.PlayReverse();
            pieChart.ClearPieValues();
        }
    }

    public void ShowResult()
    {
        /*
        if (!string.IsNullOrEmpty(HandResultString))
        {
            resultLabel.alpha = 0f;
            resultLabel.text = HandResultString;
            resultTween.PlayForward();
        }
         */
    }
}

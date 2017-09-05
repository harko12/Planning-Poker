using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Pie Chart")]
public class UIPieChart : UIWidgetContainer
{
    [System.Serializable]
    public struct UIPieValue
    {
        public string Label;
        public float Value;

        public UIPieValue(string l, float v)
        {
            Label = l;
            Value = v;
        }
    }

    public GameObject SlicePrefab;

    [SerializeField]
    [HideInInspector]
    public Dictionary<string, UIPieChartSlice> PieSlices = new Dictionary<string, UIPieChartSlice>();

    public Gradient ColorRange;
    public List<UIPieValue> PieValues = new List<UIPieValue>();

    void Start()
    {
        RefreshChart = false;
    }

    public void ClearPieValues()
    {
        // clear out slices too.. maybe with animation?
        foreach (var kvp in PieSlices)
        {
            kvp.Value.ZeroOut();
            //NGUITools.DestroyImmediate(kvp.Value.gameObject);
        }
        //PieValues.Clear();
    }

    /// <summary>
    /// Optionally set the max population, for percentage calculations
    /// </summary>
    public float TotalPopulation = 0f;

    public bool RefreshChart = false;

    void Update()
    {
        if (RefreshChart)
        {
            UpdateChart();
            RefreshChart = false;
        }
    }

    public void UpdateChart()
    {
        ClearPieValues();
        //figure out the max
        float max = 0f;
        if (TotalPopulation == 0f)
        {
            foreach (var pv in PieValues)
            {
                max += pv.Value;
            }
        }
        else
        {
            max = TotalPopulation;
        }

        // loop again to get percentages
        float totalPercentage = 0f;
        float totalVotes = 0f;
        bool alternateRow = false;
        for (int lcv = 0; lcv < PieValues.Count; lcv++)
        {
            var pv = PieValues[lcv];
            float percentage = pv.Value / max;
            AddSlice(pv, percentage, totalPercentage, GetColor((float)lcv / (float)PieValues.Count, alternateRow));

            //Debug.Log(string.Format("{0} -> {1:0.00}% totalPercent {2:0.00}%", pv.Label, percentage, totalPercentage));
            totalPercentage += percentage;
            totalVotes += pv.Value;
            alternateRow = !alternateRow; // flip it
        }
        var extra = 1f - totalPercentage;
        if (extra > 0)
        {
            AddSlice(new UIPieValue(UnaccountedLabel, totalVotes), extra, totalPercentage, GetColor( 1, alternateRow));
        }
        //Debug.Log(string.Format("{0} -> {1:0.00}%", "Unaccounted", extra));
    }

    public string UnaccountedLabel = "?";

    [HideInInspector]
    // right now, this doesn't work if set to false
    public bool InvertRotation = true;

    public float ScaleSizeRandomFactor = .5f;
    public float ScaleSpeedRandomFactor = .5f;

    private Color GetColor(float progressP, bool alternateRow)
    {
        if (alternateRow)
        {
            //progressP = 1 - progressP;
        }
        return ColorRange.Evaluate(progressP);
    }

    private void AddSlice(UIPieValue pv, float p, float totalP, Color sliceColor)
    {
        UIPieChartSlice sliceScript = null;
        if (!PieSlices.ContainsKey(pv.Label))
        {
            var slice = GameObject.Instantiate(SlicePrefab, transform.position, Quaternion.identity) as GameObject;
            PlanningPokerTools.MakeGameObjectChildOf(slice, gameObject);
            slice.name = string.Format("Slice for {0}", pv.Label);
            sliceScript = slice.GetComponent<UIPieChartSlice>();
            PieSlices[pv.Label] = sliceScript;
        }
        else
        {
            sliceScript = PieSlices[pv.Label];
        }

        //figure out some randomness
        float randomScale = Random.Range(-ScaleSizeRandomFactor, ScaleSizeRandomFactor) + 1f;
        sliceScript.scaleTween.to = new Vector3(randomScale, randomScale, randomScale);

        float randomGrowTime = Random.Range(-ScaleSpeedRandomFactor, ScaleSpeedRandomFactor);
        sliceScript.scaleTween.duration += randomGrowTime;

        sliceScript.PieValue = pv;
        sliceScript.InvertRotation = InvertRotation;
        sliceScript.StartingPercent = totalP;
        sliceScript.Percent = p;
        sliceScript._color = sliceColor;
        sliceScript.transform.localScale = Vector3.zero;
        sliceScript.scaleTween.PlayForward();
        sliceScript.RefreshSlice = true;
    }
}

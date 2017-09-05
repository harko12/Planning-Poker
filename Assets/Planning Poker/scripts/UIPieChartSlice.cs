using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(UISprite))]
public class UIPieChartSlice : MonoBehaviour {
    private UISprite _sprite;

    public UILabel _label;
    public Transform LabelPivot;
    public TweenScale scaleTween;


    public float StartingPercent;
    public float Percent;

    public UIPieChart.UIPieValue PieValue;
    [HideInInspector]
    public Color _color;
	// Use this for initialization
	void Start () {
        UpdateColor();
        UpdateLabel();
	}

    public void ZeroOut()
    {
        StartingPercent = 0f;
        Percent = 0f;
        scaleTween.PlayReverse();
        RefreshSlice = true;
    }

    private void UpdateColor()
    {
        _sprite = GetComponent<UISprite>();
        _sprite.color = _color;
    }

    private void UpdateLabel()
    {
        if (_label != null)
        {
            _label.text = "";
            if (Percent > 0)
            {
                _label.text = PieValue.Label;
            }
        }
    }

    [HideInInspector]
    public bool InvertRotation = true;
    public float RotationDegreesReference
    {
        get { return (InvertRotation ? -360f : 360f); }
    }

    private void UpdateSlice()
    {
        float start = (StartingPercent * RotationDegreesReference); // because we are 'backwards' in the scene
        var startRot = new Vector3(0, 0, start);
        transform.localRotation = Quaternion.Euler(startRot);
        _sprite.fillAmount = Percent;

        //position the label
        var angle2 = startRot + new Vector3(0, 0, Percent * RotationDegreesReference);
        var labelAngle = (angle2 - startRot) * .5f;
        LabelPivot.transform.localRotation = Quaternion.Euler(labelAngle);
        _label.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
    }
    public bool RefreshSlice = false;

    void Update()
    {
        if (RefreshSlice)
        {
            UpdateLabel();
            UpdateColor();
            UpdateSlice();
            RefreshSlice = false;
        }
    }

}

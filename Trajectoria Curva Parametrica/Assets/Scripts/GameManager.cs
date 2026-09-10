using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ControlPointsController _controlPointsController;
    [SerializeField] private List<Curve> _curves;

    private int _curCurveIndex = 0;

    private void Start()
    {
        ShowCurve();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            NextCurve();
        }
    }

    private void ShowCurve()
    {
        foreach (var curve in _curves)
        {
            curve.ClearCurvePoints();
        }

        _curves[_curCurveIndex].GenerateCurve();
    }

    public void UpdateCurve()
    {
        _curves[_curCurveIndex].GenerateCurve();
    }

    private void NextCurve()
    {
        _curCurveIndex++;
        if (_curCurveIndex >= _curves.Count)
        {
            _curCurveIndex = 0;
        }
        ShowCurve();
    }


}

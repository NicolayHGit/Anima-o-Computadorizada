using Unity.VisualScripting;
using UnityEngine;

public class LinearCurve : Curve
{
    public override void GenerateCurve()
    {
        ClearCurvePoints();
        for (int i = 0; i < _controlPoints.ControlPoints.Count - 1; i++)
        {
            Vector2 p0 = _controlPoints.ControlPoints[i].transform.position;
            Vector2 p1 = _controlPoints.ControlPoints[i + 1].transform.position;
            for (int j = 0; j <= _curveResolution; j++)
            {
                float t = (float)j / _curveResolution;
                Vector2 pointOnCurve = Vector2.Lerp(p0, p1, t);
                AddCurvePoint(pointOnCurve);
            }
        }
    }
}

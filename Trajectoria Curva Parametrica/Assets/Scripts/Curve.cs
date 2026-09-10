using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Curve : MonoBehaviour
{
    [SerializeField] protected ControlPointsController _controlPoints;
    [SerializeField] protected Transform _curvePointPrefab;
    [SerializeField] protected int _curveResolution = 10;

    protected List<Transform> _curvePointsList = new();


    protected void AddCurvePoint(Vector2 point)
    {
        Transform curvePoint = Instantiate(_curvePointPrefab, point, Quaternion.identity);

        _curvePointsList.Add(curvePoint);
    }

    public void ClearCurvePoints()
    {
        foreach (var curve in _curvePointsList)
        {
            Destroy(curve.gameObject);
        }

        _curvePointsList.Clear();
    }

    public virtual void GenerateCurve()
    {

    }
}

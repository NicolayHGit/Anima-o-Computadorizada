using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ControlPointsController : MonoBehaviour
{
    [SerializeField] private List<ControlPoint> _controlPointsList;

    public List<ControlPoint> ControlPoints => _controlPointsList;
}

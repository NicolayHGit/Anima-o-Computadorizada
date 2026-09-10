using UnityEngine;
using Matrix4x4 = System.Numerics.Matrix4x4;

public class BezierCurve : Curve
{
    private static Matrix4x4 CreateBezierMatrix()
    {
        return new Matrix4x4(
            -1f, 3f, -3f, 1f,
             3f, -6f, 3f, 0f,
            -3f, 3f, 0f, 0f,
             1f, 0f, 0f, 0f
        );
    }

    public override void GenerateCurve()
    {
        ClearCurvePoints();

        if (_controlPoints.ControlPoints.Count < 4)
            return;

        Matrix4x4 bezierMatrix = CreateBezierMatrix();

        Vector2 p0 = _controlPoints.ControlPoints[0].transform.position;
        Vector2 p1 = _controlPoints.ControlPoints[1].transform.position;
        Vector2 p2 = _controlPoints.ControlPoints[2].transform.position;
        Vector2 p3 = _controlPoints.ControlPoints[3].transform.position;

        Vector4 Gx = new(p0.x,p1.x,p2.x,p3.x);

        Vector4 Gy = new(p0.y,p1.y,p2.y,p3.y);

        float step = 1f / _curveResolution;

        for (int j = 0; j <= _curveResolution; j++)
        {
            float t = j * step;

            Vector4 T = new(t * t * t,t * t,t, 1f);

            Vector4 basis = MatrixMult(bezierMatrix, T);

            float x = Vector4.Dot(basis, Gx);
            float y = Vector4.Dot(basis, Gy);

            AddCurvePoint(new Vector2(x, y));
        }
    }

    private Vector4 MatrixMult(Matrix4x4 m, Vector4 t)
    {
        return new Vector4(
            Vector4.Dot(new Vector4(m.M11, m.M12, m.M13, m.M14),t),
            Vector4.Dot(new Vector4(m.M21, m.M22, m.M23, m.M24),t),
            Vector4.Dot(new Vector4(m.M31, m.M32, m.M33, m.M34),t),
            Vector4.Dot(new Vector4(m.M41, m.M42, m.M43, m.M44),t)
        );
    }
}

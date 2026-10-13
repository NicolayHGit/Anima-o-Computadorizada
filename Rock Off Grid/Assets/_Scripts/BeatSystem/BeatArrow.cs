using UnityEngine;
using UnityEngine.InputSystem;

public class BeatArrow : MonoBehaviour
{
    [SerializeField] private RectTransform _rectTransform;

    private BeatArrowPool _pool;
    private bool _isActive = false;

    private Vector3 _startPos;
    private Vector3 _endPos;

    private float _travelTime;
    private float _startTime;

    // Update is called once per frame
    void Update()
    {
        if (!_isActive) return;

        float t = (Time.time - _startTime) / _travelTime;
        _rectTransform.anchoredPosition = Vector3.Lerp(_startPos, _endPos, t);

        if (t >= 1f)
            OnReachedCenter();
    }

    public void Initialize(BeatArrowPool pool, bool left)
    {
        _pool = pool;
        if (left) _rectTransform.localScale = new Vector3(-1f, 1f, 1f);
    }

    public void ArrowStart(Vector3 start, Vector3 end, float travelTime)
    {
        _startPos = start;
        _endPos = end;
        _travelTime = travelTime;
        _startTime = Time.time;
        _rectTransform.anchoredPosition = start;
        _isActive = true;
        gameObject.SetActive(true);
    }

    private void OnReachedCenter()
    {
        _isActive = false;
        gameObject.SetActive(false);
        _pool.ReturnToPool(this);
    }
}

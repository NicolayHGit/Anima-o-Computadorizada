using UnityEngine;
using System.Collections.Generic;

public class BeatArrowPool : MonoBehaviour
{
    [SerializeField] private BeatArrow arrowPrefab;
    
    [SerializeField] private int poolSize = 30;
    [SerializeField] private bool _leftOrientation;
    [SerializeField] private Vector2 offset = new(320f, 0f);

    private RectTransform _rectTransform;
    private Queue<BeatArrow> _pool = new Queue<BeatArrow>();

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        for (int i = 0; i < poolSize; i++)
        {
            BeatArrow arrow = Instantiate(arrowPrefab, transform);
            arrow.Initialize(this, _leftOrientation);
            arrow.gameObject.SetActive(false);
            _pool.Enqueue(arrow);
        }
    }

    public BeatArrow GetArrow()
    {
        if (_pool.Count > 0)
        {
            return _pool.Dequeue();
        }

        return null;
    }

    public void SpawnArrow(RectTransform middleRT, float travelTime)
    {
        BeatArrow arrow = GetArrow();
        Vector3 start = _rectTransform.anchoredPosition;
        //Vector3 end = middleRT.localPosition;
        Vector3 end = _rectTransform.anchoredPosition + offset;
        arrow.ArrowStart(start, end, travelTime);
    }

    public void ReturnToPool(BeatArrow arrow)
    {
        arrow.gameObject.SetActive(false);
        _pool.Enqueue(arrow);
    }
}

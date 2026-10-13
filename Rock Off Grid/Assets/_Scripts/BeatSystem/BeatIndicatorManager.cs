using UnityEngine;

public class BeatIndicatorManager : MonoBehaviour
{
    [SerializeField] private float _interval = 1f;
    [SerializeField] private float _arrowTravelBeatMultiplier = 1f;
    [SerializeField] private RectTransform _middlePoint;
    [SerializeField] private BeatArrowPool _leftPool;
    [SerializeField] private BeatArrowPool _rightPool;

    private void Start()
    {
        Debug.Log("Registering Beat Indicator");
        BeatManager.instance.RegisterInInterval(_interval, SpawnArrows);
    }

    public void SpawnArrows()
    {
        float beatDuration = BeatManager.instance.BeatDuration * _arrowTravelBeatMultiplier;
        _leftPool.SpawnArrow(_middlePoint, beatDuration);
        _rightPool.SpawnArrow(_middlePoint, beatDuration);
    }
}

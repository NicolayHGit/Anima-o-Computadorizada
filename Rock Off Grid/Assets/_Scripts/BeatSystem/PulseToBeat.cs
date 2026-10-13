using System.Collections;
using UnityEngine;

public class PulseToBeat : MonoBehaviour
{
    [SerializeField] private float _interval = 1;
    [SerializeField] private float _pulseSize = 1.15f;
    [SerializeField] private float _returnSpeed = 5f;

    private Transform _transformToPulse;
    private Vector3 _startSize;

    private void Awake()
    {
        _transformToPulse = GetComponent<Transform>();
    }

    private void Start()
    {
        _startSize = _transformToPulse.localScale;
        BeatManager.instance.RegisterInInterval(_interval, Pulse);
    }

    private void OnDestroy()
    {
        BeatManager.instance.UnRegisterInInterval(_interval, Pulse);
    }


    private void Update()
    {
        _transformToPulse.localScale = Vector3.Lerp(_transformToPulse.localScale, _startSize, Time.deltaTime * _returnSpeed);
    }

    public void Pulse()
    {
        Vector3 targetScale = _startSize * _pulseSize;
        _transformToPulse.localScale = new Vector3(
            Mathf.Max(_transformToPulse.localScale.x, targetScale.x),
            Mathf.Max(_transformToPulse.localScale.y, targetScale.y),
            _startSize.z // garante que o Z não seja perdido
        );
    }
}

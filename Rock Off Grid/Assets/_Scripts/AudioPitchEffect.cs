using UnityEngine;

public class AudioPitchEffect : MonoBehaviour
{
    [SerializeField] private AudioSource _audio;
    [SerializeField] private Vector2 _pitchRange = new Vector2(0.9f, 1.1f);

    public void PlayRandRangeAudio()
    {
        _audio.pitch = Random.Range(_pitchRange.x, _pitchRange.y);
        _audio.Play();
    }
}

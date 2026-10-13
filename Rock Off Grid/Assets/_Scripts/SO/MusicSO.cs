using UnityEngine;

[CreateAssetMenu(menuName = "Rhythm/Music")]
public class MusicSO : ScriptableObject
{
    public string songName;
    public int bpm;
    public AudioClip audioClip;
}

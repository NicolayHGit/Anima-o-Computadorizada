using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rhythm/ScoreConfig")]
public class ScoreConfig : ScriptableObject
{
    public int scorePerNote = 100;
    public int scorePerGoodNote = 125;
    public int scorePerPerfectNote = 150;
    public List<ScoreThreshold> multiplierThresholds;
}

[System.Serializable]
public class ScoreThreshold
{
    public int multiplierThreshold;
    public string rank;
}


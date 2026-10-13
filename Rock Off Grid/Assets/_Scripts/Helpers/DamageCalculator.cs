public static class DamageCalculator
{
    public static float CalculateDamage(float baseValue, int playerMultiplier, TimingAccuracyType accuracy)
    {
        float accuracyMultiplier = accuracy switch
        {
            TimingAccuracyType.Miss => 0.75f,
            TimingAccuracyType.Normal => 1f,
            TimingAccuracyType.Good => 1.25f,
            TimingAccuracyType.Perfect => 1.5f,
            _ => 1f
        };

        return baseValue * playerMultiplier * accuracyMultiplier;
    }
}
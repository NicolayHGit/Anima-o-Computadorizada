using UnityEngine;

public class Enemy : Character
{
    [Header("IA Timing Settings")]
    [SerializeField, Range(0, 1)] private float basePerfectChance = 0.1f;
    [SerializeField, Range(0, 1)] private float baseGoodChance = 0.3f;
    [SerializeField, Range(0, 1)] private float baseErrorChance = 0.05f;

    [Header("IA Behavior Settings")]
    [SerializeField] private int meleeRange = 1;
    [SerializeField] private int rangedRange = 3;
    [SerializeField] private float defenseChance = 0.2f;

    [SerializeField] private int minPauseBeats = 1;
    [SerializeField] private int maxPauseBeats = 3;

    [Header("Difficulty Balance")]
    [SerializeField] private float multiplierInfluence = 0.1f;
    // quanto o multiplier do jogador altera a performance do inimigo

    private Character _playerTarget;

    private int _pauseBeatsRemaining = 0;

    public override void Init(PlayerType type)
    {
        base.Init(type);
        _playerTarget = GameManager.instance.GetCharacter(PlayerType.PLAYERA);
    }

    protected override void OnBeat()
    {
        base.OnBeat();
        if (BattleManager.instance.State != BattleState.Playing) return;

        if (_playerTarget == null || _playerTarget.CurTile == null || CurTile == null)
            return;

        // ⏸ Se ainda está em pausa, só decrementa
        if (_pauseBeatsRemaining > 0)
        {
            _pauseBeatsRemaining--;
            return;
        }

        // 1️⃣ Prever posição do jogador
        Tile predictedTile = PredictPlayerNextTile();

        // 2️⃣ Decidir movimento
        if (Random.Range(0f, 1f) > 0.5)
            _movementComponent.SetMovementTowardsEnemy(predictedTile);
        else
            _movementComponent.SetRandomMovement();

            // 3️⃣ Calcular acurácia fake
            TimingAccuracyType fakeAccuracy = GetFakeAccuracy();

        // 4️⃣ Atacar/defender
        DecideAndUseSkill(predictedTile, fakeAccuracy);

        // 5️⃣ Definir nova pausa
        SetPauseBasedOnMultiplier();
    }

    private Tile PredictPlayerNextTile()
    {
        // TODO: integrar com Movement do player se tiver buffer de inputs
        // Por agora, apenas usa a posição atual
        return _playerTarget.CurTile;
    }

    private TimingAccuracyType GetFakeAccuracy()
    {
        int playerMultiplier = _playerTarget.Score.CurMultiplier;

        // influencia baseada no multiplier do player
        float difficultyBonus = Mathf.Clamp01((playerMultiplier - 1) * multiplierInfluence);

        // calcula probabilidades
        float perfectChance = basePerfectChance + difficultyBonus * 0.3f;
        float goodChance = baseGoodChance + difficultyBonus * 0.5f;
        float errorChance = Mathf.Max(baseErrorChance, 1f - (perfectChance + goodChance + 0.6f));

        float rnd = Random.value;

        if (rnd < errorChance) return TimingAccuracyType.Miss;
        if (rnd < perfectChance) return TimingAccuracyType.Perfect;
        if (rnd < goodChance) return TimingAccuracyType.Good;

        return TimingAccuracyType.Normal;
    }

    private void SetPauseBasedOnMultiplier()
    {
        int playerMultiplier = _playerTarget.Score.CurMultiplier;

        // menor pausa conforme multiplier aumenta
        float difficultyMod = Mathf.Clamp01((playerMultiplier - 1) * multiplierInfluence);

        // sorteia entre min e max beats
        int randomPause = Random.Range(minPauseBeats, maxPauseBeats + 1);

        // aplica redução
        _pauseBeatsRemaining = Mathf.Max(1, randomPause - Mathf.FloorToInt(difficultyMod * randomPause));
    }

    private void DecideAndUseSkill(Tile predictedTile, TimingAccuracyType accuracy)
    {
        int dist = (int)Vector2.Distance(CurTile.Position, predictedTile.Position);

        // 🛡️ Defesa (se estiver prestes a ser atacado e sorte passar)
        if (Random.value < defenseChance && _defendSkillComponent != null && _defendSkillComponent.CanUse())
        {
            UseDefendSkill(accuracy);
            _characterScore.AddDefenseBonus(accuracy);
            return;
        }

        // 💥 Melee se perto
        if (dist <= meleeRange && _meleeSkillComponent != null && _meleeSkillComponent.CanUse())
        {
            UseMeleeSkill(accuracy);
            _characterScore.PerformActionBeat(accuracy);
            return;
        }

        // 🌟 Especial se tiver mana suficiente
        if (_specialSkillComponent != null && _specialSkillComponent.CanUse() && Mana.CurMana >= 50) // exemplo
        {
            UseSpecialSkill(accuracy);
            _characterScore.PerformActionBeat(accuracy);
            return;
        }

        // 🎯 Ranged se longe
        if (dist > meleeRange && dist <= rangedRange && _rangedSkillComponent != null && _rangedSkillComponent.CanUse())
        {
            UseRangeSkill(accuracy);
            _characterScore.PerformActionBeat(accuracy);
            return;
        }

        // Caso nenhuma condição, faz defesa leve (fallback)
        if (_defendSkillComponent != null && _defendSkillComponent.CanUse())
        {
            UseDefendSkill(accuracy);
            _characterScore.AddDefenseBonus(accuracy);
        }
    }
}

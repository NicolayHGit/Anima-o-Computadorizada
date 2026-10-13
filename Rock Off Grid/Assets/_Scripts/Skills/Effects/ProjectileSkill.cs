using UnityEngine;
using UnityEngine.TextCore.Text;

public class ProjectileSkill : SkillObject
{
    [SerializeField] protected bool _smooth;
    [SerializeField] protected bool _halfBeat = false;

    // Para smooth
    private Tile _startTile;
    private Tile _endTile;
    private float _travelTime;
    private float _elapsed;


    public override void Init(SkillComponent skill, PlayerType type, TimingAccuracyType accuracy, float value = 0)
    {
        base.Init(skill,type, accuracy, value);
        PlayAudioSkill();
        _startTile = _skillComponent.Character.CurTile;
        transform.position = _startTile.transform.position;
        Launch(accuracy);
    }

    protected override void Recycle()
    {
        float step = _halfBeat ? 2f : 1f;
        if (!_smooth) BeatManager.instance.UnRegisterInInterval(step, StepMove);
        base.Recycle();
    }

    private void Launch(TimingAccuracyType accuracy)
    {
        int endPosX = (_type == PlayerType.PLAYERA) ? 5 : 0;
        _endTile = GridManager.instance.GetTileAtPosition(new Vector2Int(endPosX, _skillComponent.Character.CurTile.Position.y));

        if (_smooth)
        {
            float distance = (_type == PlayerType.PLAYERA) ? _endTile.Position.x - _startTile.Position.x : _startTile.Position.x - _endTile.Position.x;
            _travelTime = (distance + 1) * (_halfBeat ? BeatManager.instance.BeatDuration / 2f
                                            : BeatManager.instance.BeatDuration);
            _elapsed = 0;
        }
        else
        {
            _curTile = _startTile;
            float step = _halfBeat ? 2f : 1f;
            BeatManager.instance.RegisterInInterval(step, StepMove);
        }
    }

    private void Update()
    {
        if (!_smooth) return;

        _elapsed += Time.deltaTime;
        float t = _elapsed / _travelTime;
        transform.position = Vector3.Lerp(_startTile.transform.position, _endTile.transform.position, t);

        if (t >= 1f)
            Recycle();
    }

    private void StepMove()
    {
        var NextTile = GridManager.instance.GetTileAtPosition(_curTile.Position + _direction);
        if (NextTile != null)
        {
            transform.position = NextTile.transform.position;
            _curTile = NextTile;
        }
        else
            Recycle();
    }    
}

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GranadeSkill : SkillObject
{
    [SerializeField] protected SpriteRenderer _render;
    [SerializeField] protected GameObject _explosionPrefab;
    [SerializeField] protected int _poolSize = 1;

    protected Queue<GameObject> _explosionPool = new Queue<GameObject>();

    private Tile _startTile;
    private Tile _endTile;

    private void Awake()
    {
        InstantiatePool();
    }

    public override void Init(SkillComponent skill, PlayerType type, TimingAccuracyType accuracy, float value = 0, Vector2Int? offset = null)
    {
        base.Init(skill, type, accuracy, value);
        _startTile = skill.Character.CurTile;
        transform.position = _startTile.transform.position;
        _endTile = GridManager.instance.GetTileAtPosition(_startTile.Position + (Vector2Int)offset);
        Show();
        Launch(accuracy);
    }

    private void Launch(TimingAccuracyType accuracy)
    {
        int beatsToTravel = (accuracy == TimingAccuracyType.Perfect || accuracy == TimingAccuracyType.Good) ? 1 : 2;
        float targetTime = BeatManager.instance.GetNextBeatTime(beatsToTravel);
        float travelTime = targetTime - BeatManager.instance.SongTime;

        StartCoroutine(MoveToTarget(travelTime));
    }

    private void Show()
    {
        _render.color = new Color(1, 1, 1, 1);
    }

    private void Hide()
    {
        _render.color = new Color(1,1,1,0);
    }

    private IEnumerator MoveToTarget(float travelTime)
    {
        Vector3 start = _startTile.transform.position;
        Vector3 end = _endTile.transform.position;

        float elapsed = 0f;

        while (elapsed < travelTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / travelTime;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;
        PlayAudioSkill();
        Hide();
        DoExplosion(_endTile);

        // explosão secundária na próxima batida
        float waitNextBeat = BeatManager.instance.BeatDuration;
        yield return new WaitForSeconds(waitNextBeat);

        ExplodeAdjacent(_endTile);
        yield return new WaitForSeconds(waitNextBeat);
        Recycle();
    }

    private void DoExplosion(Tile tile)
    {
        if (tile == null) return;
        if (tile.Type == _type) return;

        var explosion = GetFromPool();
        explosion.transform.position = tile.transform.position;

        StartCoroutine(RecycleExplosion(explosion));

        var character = tile.CurCharacter;
        if (character == null) return;
        if (GameManager.instance.IsOnline() && !NetworkManager.Singleton.IsServer) return;
        character.Hit(_multiplierDamage);
    }

    private void ExplodeAdjacent(Tile centerTile)
    {
        if (centerTile == null) return;

        Vector2Int[] offsets = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        PlayAudioSkill();
        foreach (var offset in offsets)
        {
            Tile tile = GridManager.instance.GetTileAtPosition(centerTile.Position + offset);
            if(tile != null) DoExplosion(tile);
        }
    }

    protected virtual void InstantiatePool()
    {
        for (int i = 0; i < _poolSize; i++)
        {
            var obj = Instantiate(_explosionPrefab, transform);
            obj.SetActive(false);
            _explosionPool.Enqueue(obj);
        }
    }

    protected GameObject GetFromPool()
    {
        if (_explosionPool.Count > 0)
        {
            GameObject obj = _explosionPool.Dequeue();
            obj.transform.SetParent(null);
            obj.gameObject.SetActive(true);
            return obj;
        }

        var newObj = Instantiate(_explosionPrefab);
        return newObj;
    }

    protected void ReturnToPool(GameObject obj)
    {
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(transform);
        _explosionPool.Enqueue(obj);
    }

    private IEnumerator RecycleExplosion(GameObject obj)
    {
        yield return new WaitForSeconds(_duration);
        ReturnToPool(obj);
    }
}

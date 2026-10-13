using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager instance;

    [SerializeField] private BattleManager _battleManager;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private int _width;
    [SerializeField] private int _height;
    [SerializeField] private float _tileOffset = 2.5f;

    [SerializeField] private Transform _cam;

    private Dictionary<Vector2Int, Tile> _tiles = new();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (!GameManager.instance.IsOnline()) GenerateGrid();
    }

    public void GenerateGrid()
    {
        Vector2 _tileWorldPosition = Vector2.zero;
        for (int x = 0; x < _width; x++)
        {
            PlayerType type = (x <= 2) ? PlayerType.PLAYERA : PlayerType.PLAYERB;
            for (int y = 0; y < _height; y++)
            {
                _tileWorldPosition = new Vector2(x * _tileOffset, y * _tileOffset);
                var tile = Instantiate(_tilePrefab, _tileWorldPosition, Quaternion.identity);
                tile.name = $"Tile {x}-{y}";
                tile.Init(type, new Vector2Int(x, y));


                _tiles[new Vector2Int(x, y)] = tile;
            }
        }

        _cam.transform.position = new Vector3((float)_width * _tileOffset / 2 - (_tileOffset/2), (float)_height * _tileOffset / 2 - (_tileOffset / 2), -10f);

        if (!GameManager.instance.IsOnline())
            _battleManager.GenerateBattlefield();
    }

    public void MoveCharacterToTile(Character character, Vector2Int tilePos)
    {
        Tile tile = GetTileAtPosition(tilePos);
        if ((tile == null) ||
           (tile.Type != character.Type) ||
           (tile.CurCharacter != null))  return;

        

        if (character.CurTile != null)
            character.CurTile.MoveCharacterOut();

        tile.MoveCharacterIn(character);
    }

    public Tile GetTileAtPosition(Vector2Int pos)
    {
        if (_tiles.TryGetValue(pos, out var tile))
        {
            return tile;
        }
        return null;
    }

    public Tile GetTileAtPosition(Vector2Int pos, PlayerType type)
    {
        if (_tiles.TryGetValue(pos, out var tile))
        {
            if(tile.Type != type) return null;
            return tile;
        }
        return null;
    }

    public void ClearAllTiles()
    {
        foreach (var tile in _tiles.Values)
        {
            tile.MoveCharacterOut();
        }
    }
}

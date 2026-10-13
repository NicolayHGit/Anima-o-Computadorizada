using UnityEngine;
using UnityEngine.UIElements;

public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite _spriteA, _spriteB;

    [SerializeField] private PlayerType _type;
    [SerializeField] private Vector2Int _position;
    [SerializeField] private Character _curCharacter;

    public PlayerType Type => _type;
    public Vector2Int Position => _position;
    public Character CurCharacter => _curCharacter;

    public void Init(PlayerType type, Vector2Int pos)
    {
        _type = type;
        _position = pos;
        _spriteRenderer.sprite = (_type == PlayerType.PLAYERA) ? _spriteA : _spriteB;
    }

    public void MoveCharacterIn(Character character)
    {
        _curCharacter = character;
        _curCharacter.MoveToTile(this);
    }

    public void MoveCharacterOut()
    {
        _curCharacter = null;
    }

    public void HitTargetInTile(float value)
    {
        if(_curCharacter == null) return;
        _curCharacter.Hit(value);
    }
}

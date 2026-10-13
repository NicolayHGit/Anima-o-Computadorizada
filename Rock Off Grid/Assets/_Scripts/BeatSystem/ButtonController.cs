using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ButtonController : MonoBehaviour
{
    [SerializeField] private InputAction _pressKey;
    //[SerializeField] private List<BeatScroller> _beatScrollers = new();
    [SerializeField] private SpriteRenderer _spriteRender;
    [SerializeField] private Sprite _defaultSprite;
    [SerializeField] private Sprite _pressedSprite;

    private BeatScore _curScore;

    private void Start()
    {
        _pressKey.started += KeyPressed;
        _pressKey.canceled += KeyRelease;
        _pressKey.Enable();
    }

    public void PulseSpawnArrow()
    {
        /*if(_beatScrollers == null) return;
        foreach (var scroller in _beatScrollers)
        {
            scroller.SpawnArrow();
        }*/
    }

    public void ChangeCurScore(BeatScore score) => _curScore = score;

    public void KeyPressed(InputAction.CallbackContext context)
    {
        _spriteRender.sprite = _pressedSprite;
    }

    public void KeyRelease(InputAction.CallbackContext context)
    {
        _spriteRender.sprite = _defaultSprite;
    }
}

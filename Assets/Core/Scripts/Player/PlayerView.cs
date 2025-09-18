using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : MonoBehaviour
{
    private SpriteRenderer _spritePlayer;


    private void Start()
    {
        _spritePlayer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Vector2 _mousePosition = GameUtils.GetMousePosition();

        float directionToMouse = _mousePosition.x - transform.position.x;

        if (directionToMouse > 0.1f)
        {
            _spritePlayer.flipX = false;
        }
        else if (directionToMouse < -0.1f )
        {
            _spritePlayer.flipX = true;
        }
    }
}

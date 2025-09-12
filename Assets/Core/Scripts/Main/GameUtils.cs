using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameUtils
{

    //
    public static Vector2 GetMousePosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    
}

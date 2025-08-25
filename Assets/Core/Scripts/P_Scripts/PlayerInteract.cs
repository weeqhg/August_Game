using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerInteract : MonoBehaviour
{

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }
    public abstract void Interact();


}

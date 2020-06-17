using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_movement : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.Translate(0,0, InputManager.instance.GetAxis("Vertical") * 0.1f, Space.Self);
        transform.Rotate(0, InputManager.instance.GetAxis("Horizontal"), 0, Space.Self);
    }
}

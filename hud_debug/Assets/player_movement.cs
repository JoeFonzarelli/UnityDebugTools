using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class player_movement : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.Translate(0,0, Input.GetAxis("Vertical")*0.3f, Space.Self);
        transform.Rotate(0, Input.GetAxis("Horizontal"), 0, Space.Self);
    }
}

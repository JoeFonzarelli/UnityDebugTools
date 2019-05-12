using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarImg_Setup : MonoBehaviour
{
    [SerializeField] Transform minPos, maxPos;


    internal Vector2 proportion; //size of the square formed by minpos and maxpos


    void Start()
    {
        proportion.x =  Mathf.Abs(minPos.position.x - maxPos.position.x);
        proportion.y = Mathf.Abs(minPos.position.z - maxPos.position.z);

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

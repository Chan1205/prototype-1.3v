using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform2 : MonoBehaviour
{
    public Vector2 pos1;
    public Vector2 pos2;
    public float speed = 2f;
    public float timer = 1;

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.Lerp(pos1, pos2, Mathf.PingPong(Time.time * speed, timer));


    }
}

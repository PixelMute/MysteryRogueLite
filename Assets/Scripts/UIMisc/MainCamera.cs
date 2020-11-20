using System;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        var x = (int)Math.Round((double)transform.position.x, 2);
        var y = (int)Math.Round((double)transform.position.y, 2);
        var z = (int)Math.Round((double)transform.position.x, 2);
        transform.position = new Vector3(x, y, z);
    }
}

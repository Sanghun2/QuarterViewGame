using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    
    void Update()
    {
        transform.Rotate(Vector3.right * 10 * Time.deltaTime);
    }
}

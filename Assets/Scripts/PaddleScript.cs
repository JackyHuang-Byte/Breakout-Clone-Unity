using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleScript : MonoBehaviour
{
    [Tooltip("[KEEP CONSISTENT WITH SCRIPT]")] public float Speed = 30f;
    [Tooltip("[KEEP CONSISTENT WITH SCRIPT]")] public float MaxX = 7.5f;

    private float _movementHoriz;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _movementHoriz = Input.GetAxis("Horizontal"); //Debug.Log(_movementHoriz);

        if((_movementHoriz > 0 && transform.position.x < MaxX)
         ||(_movementHoriz < 0 && transform.position.x > -MaxX))
        {
            transform.position += Vector3.right * _movementHoriz * Speed * Time.deltaTime;
        }

    }
}

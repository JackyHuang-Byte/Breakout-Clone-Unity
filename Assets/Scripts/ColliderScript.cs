using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ColliderScript : MonoBehaviour
{
    [SerializeField] private BouncyBallScript BouncyBallScript;
    void Awake()
    {
        BouncyBallScript = FindObjectOfType<BouncyBallScript>();
    }

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        BouncyBallScript.BouncyBallOnCollision(collision);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        BouncyBallScript.BouncyBallOnTrigger(collision);
    }

    private string TagIndex(int index) //set up a local getter
    {
       return BouncyBallScript.TagIndex(index);
    }
}

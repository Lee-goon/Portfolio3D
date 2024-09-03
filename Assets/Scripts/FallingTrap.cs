using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingTrap : MonoBehaviour
{
    Rigidbody rigid;
    
    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    /*
    private void OnTriggerStay(Collider collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            float distance = Mathf.Abs(collision.transform.position.y - this.transform.position.y);
            if (distance < 1.2f) 
            {
                // Debug.Log("³»·Á°¨");
            }
        }
    }
    */

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            rigid.velocity = Vector3.zero;
            rigid.useGravity = true;
            Destroy(gameObject, 3f);
        }
    }
}

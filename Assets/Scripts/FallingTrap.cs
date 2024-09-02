using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingTrap : MonoBehaviour
{
    Rigidbody rigid;
    
    float distance;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

    
    private void OnTriggerStay(Collider collision)
    {
        if(collision.gameObject.tag == "Player")
        {
            distance = Mathf.Abs(collision.transform.position.y - this.transform.position.y);
            if (distance < 1.2f) 
            {
                Debug.Log("³»·Á°¨");
                rigid.velocity = Vector3.zero;
                rigid.useGravity = true;
            }
        }
        else
        {
            return;
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            Destroy(gameObject, 2f);
        }
    }
}

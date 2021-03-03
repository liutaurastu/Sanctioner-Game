using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public GameObject fracture;
    public float health;
    //Rigidbody rb;

    public void TakeDamage(float dmgAmount)
    {
        health -= dmgAmount;
        if(health <= 0f)
        {
            Destruct();
        }
    }

    void Destruct()
    {
        if(fracture != null)  
        {
            Vector3 pos = transform.position;
            try
            {
                fracture = (GameObject) Instantiate(fracture, pos, transform.rotation);
                // foreach child game object in fracture - child rigidbody add force -------------no errors doesnt work
                /*
                for(int i=0; i<fracture.transform.childCount; i++)
                {
                    rb = fracture.transform.GetChild(i).GetComponent<Rigidbody>();
                    rb.AddForce(Vector3.forward * forceAmount);
                }
                */
            }
            finally
            {
                Destroy(fracture, 10f);
            }
        }
        Destroy(gameObject);
    }
}

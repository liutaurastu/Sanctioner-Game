using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    public GameObject weapon;
    Animator animator;

    public float swayAmount = 0.3f; //(0.3)
    public float moveAmount = 0.2f; //(0.2)

    Vector3 pos;

    float swayAmountSmoothness;
    float moveAmountSmoothness;

    void Start()
    {
        //get current weapon position
        if(weapon != null) pos = weapon.transform.localPosition;
        else pos = transform.localPosition;

        //smoothness indicates the speed at which the weapon sways (10) or moves (15), I base it off the original amount
        swayAmountSmoothness = swayAmount * 10;
        moveAmountSmoothness = moveAmount * 15;

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        //calculate exagerated mouse movement (opposite)
        float mouseX = -Input.GetAxis("Mouse X") * swayAmount;
        float mouseY = -Input.GetAxis("Mouse Y") * swayAmount;

        //calculate exagerated movement (opposite)
        float movementX = -Input.GetAxis("Horizontal") * moveAmount;
        float movementY = -Input.GetAxis("Vertical") * moveAmount;

        //weapon sway
        Vector3 finalPos = new Vector3(mouseX, mouseY, 0);
        //weapon move
        Vector3 finalMovePos = new Vector3(movementX, movementY, 0);

        //To know if the player is attacking
        if(animator.GetBool("AnimationState") == false)
        {
            if(weapon != null) 
            {
                weapon.transform.localPosition = Vector3.Lerp(weapon.transform.localPosition, finalPos + pos, Time.deltaTime * swayAmountSmoothness);

                //weapon moves in the opposite direction from the player
                weapon.transform.localPosition = Vector3.Lerp(weapon.transform.localPosition, finalMovePos + pos, Time.deltaTime * moveAmountSmoothness);
            }
            else 
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, finalPos + pos, Time.deltaTime * swayAmountSmoothness);

                //weapon moves in the opposite direction from the player
                transform.localPosition = Vector3.Lerp(transform.localPosition, finalMovePos + pos, Time.deltaTime * moveAmountSmoothness);
            }
        }
    }
}

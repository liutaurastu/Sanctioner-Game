using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public CharacterController controller;

    public float speed = 4f;//(default: 4f)
    float sprintSpeed = 7f;//(default: 7f)
    float jumpHeight = 1f;//(default: 1f)
    float gravity = -20f;//(default: -20f)
    float groundDistance = 0.5f;//(default: 0.5f)
    float ceilingDistance = 0.2f;//(default: 0.2f)

    //gameobject to check if player is on ground or on the ceiling
    public Transform groundCheck;
    public Transform ceilingCheck;

    //layer mask to determine if the interactable model is ground or a ceiling
    public LayerMask groundMask;
    public LayerMask ceilingMask;
    Vector3 move;

    public bool isGrounded; //boolean to check if player's feet are touching the ground
    
    //check if player is hitting ceiling
    bool isHittingCeiling;

    //falling velocity
    Vector3 velocity;
    Vector3 lastVelocity;

    //variables to help determine if the player is sprinting
    public bool isSprinting = false;

    //initial player values to reset them after changing
    float originalControllerSpeed;
    float originalControllerHeight;

    private Animator animator;


    private int maxHealth = 100;
    public int currentHealth;
    public HealthBar healthBar;

    private float maxStamina = 100f;
    private float staminaRegenRate = 15f;
    public float currentStamina;
    public StaminaBar staminaBar;

    private float maxMana = 100f;
    private float manaRegenRate = 2f;
    public float currentMana;
    public ManaBar manaBar;

    void Start()
    {
        //save the original players values for future reference
        originalControllerHeight = controller.height;
        originalControllerSpeed = speed;
        //get weapon animation controller
        animator = GetComponentInChildren<Animator>();
        
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);

        currentStamina = maxStamina;
        staminaBar.SetMaxStamina(maxStamina);

        currentMana = maxMana;
        manaBar.SetMaxMana(maxMana);
    }

    void Update()
    {
        InputChecks();
        Jump();
        Sprint();
        Move();
        RegenStamina(staminaRegenRate);
        RegenStamina(manaRegenRate);
    }

    private void InputChecks()
    {
        //check if player is on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        //check if player is hitting the ceiling
        isHittingCeiling = Physics.CheckSphere(ceilingCheck.position, ceilingDistance, ceilingMask);

        //get inputs
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        //define move direction depending on inputs x and z
        move = transform.right * x + transform.forward * z;
    }
    
    private void Jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            //save your velocity
            lastVelocity = move;
            //jump
            velocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);
        }

        if (!isGrounded)
        {
            //disables stepOffset and slopelimit when not on ground because so we cannot climb up a stair while falling
            controller.slopeLimit = 0f;
            controller.stepOffset = 0f;
            //just to make it feel more natural, disble animation when in the air
            animator.SetBool("IsSprinting", false);
        }
        else
        {
            //when on ground enable step offset and slopelimit
            controller.slopeLimit = 45;
            controller.stepOffset = 0.4f;
        }

        //if player hits ceiling, switch to falling velocity
        if (isHittingCeiling) velocity.y = -2.0f;

        //reset velocity
        if (isGrounded && velocity.y < 0) velocity.y = -2.0f;
    }

    private void Sprint()
    {
        if (Input.GetKey(KeyCode.LeftShift) && isGrounded)
        {
            //sprint
            isSprinting = true;
            speed = sprintSpeed;
            //start playing sprint animation
            if(move.magnitude > 0) animator.SetBool("IsSprinting", true);
            else animator.SetBool("IsSprinting", false);
        }
        else if (isGrounded)
        {
            //stop sprinting
            isSprinting = false;
            speed = originalControllerSpeed;
            //stop playing sprint animation
            animator.SetBool("IsSprinting", false);
        }
    }

    private void Move()
    {
        //always change move speed to its maximum value (to counter diagnal speed problem)
        if (move.magnitude >= 1.0f) move.Normalize();
        if (lastVelocity.magnitude >= 1.0f) lastVelocity.Normalize();

        //move the player, if the player is not touching the ground, you can only control its movements at 50% power
        if (!isGrounded)
        {
            controller.Move(lastVelocity * 0.5f * speed * Time.deltaTime);
            controller.Move(move * 0.5f * speed * Time.deltaTime);
        }
        else controller.Move(move * speed * Time.deltaTime);

        //velocity calculation
        velocity.y += gravity * Time.deltaTime;

        //applying velocity (an extra Time.deltaTime because its based on a physics formula)
        controller.Move(velocity * Time.deltaTime);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
    }
    public void Heal(int heal)
    {
        currentHealth += heal;
        healthBar.SetHealth(currentHealth);
    }

    public void UseStamina(float staminaUsage)
    {
        currentStamina -= staminaUsage;
        staminaBar.SetStamina(currentStamina);
    }
    private void RegenStamina(float staminaRegenRate)
    {
        if(currentStamina < 100) 
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            staminaBar.SetStamina(currentStamina);
        }
    }

    public void UseMana(float manaUsage)
    {
        currentMana -= manaUsage;
        manaBar.SetMana(currentMana);
    }
    private void RegenMana(float manaRegenRate)
    {
        if(currentMana < 100) 
        {
            currentMana += manaRegenRate * Time.deltaTime;
            manaBar.SetMana(currentMana);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwing : MonoBehaviour
{
    public float damage = 20f;
    public float range = 3f;
    public float recoverySpeed = 1f;
    public float swingSpeed = 1f;
    public float swingRadius = 0.3f;
    public int oneSwingTargetCount = 3;

    public GameObject hitParticle;
    public GameObject swingDecal;
    public GameObject swingDecalLeft;
    public AudioClip hitSound;
    public AudioClip swingSound;

    public Camera cameraView;

    AudioSource audioSource;
    Animator animator;
    TrailRenderer trailRender;
    float attackDelayTimer;
    float defaultAttackDelay;
    float[] pendingDamage;


    int decalsLeft = 1;
    int groundMask;
    int wallMask;
    int decalLayerMask;
    bool canAttack;
    bool attackMiss;
    int targetCount = 0;

    PlayerController playerController;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        trailRender = GetComponentInChildren<TrailRenderer>();

        playerController = GetComponentInParent<PlayerController>();

        groundMask = 1 << LayerMask.NameToLayer("Ground");
        wallMask = 1 << LayerMask.NameToLayer("Wall");
        decalLayerMask = groundMask | wallMask;

        pendingDamage = new float[oneSwingTargetCount];
        for(int i=0; i<oneSwingTargetCount; i++) pendingDamage[i] = damage;

        //This monster right here is completely due to my own lack of mathematical knowledge, I just know there is a formula to simply get the value I want but I don't know how to 
        //write it. Here is a failed example:
        //defaultAttackDelay = 0.2f * Mathf.Pow(0.7f, attackDelay);

        if(swingSpeed < 0.15f) defaultAttackDelay = 2f;
        else if(swingSpeed >= 0.15f && swingSpeed < 0.25f) defaultAttackDelay = 1f;
        else if(swingSpeed >= 0.25f && swingSpeed < 0.35f) defaultAttackDelay = 0.6f;
        else if(swingSpeed >= 0.35f && swingSpeed < 0.45f) defaultAttackDelay = 0.4f;
        else if(swingSpeed >= 0.45f && swingSpeed < 0.55f) defaultAttackDelay = 0.36f;
        else if(swingSpeed >= 0.55f && swingSpeed < 0.85f) defaultAttackDelay = 0.24f;
        else if(swingSpeed >= 0.85f && swingSpeed < 1.5f) defaultAttackDelay = 0.2f;

        else if(swingSpeed >= 1.5f && swingSpeed < 2.5f) defaultAttackDelay = 0.1f;
        else if(swingSpeed >= 2.5f && swingSpeed < 3.5f) defaultAttackDelay = 0.06f;
        else if(swingSpeed >= 3.5f && swingSpeed < 4.5f) defaultAttackDelay = 0.04f;
        else if(swingSpeed >= 4.5f && swingSpeed < 5.5f) defaultAttackDelay = 0.036f;
        else if(swingSpeed >= 5.5f && swingSpeed < 8.5f) defaultAttackDelay = 0.024f;
        else if(swingSpeed >= 8.5f) defaultAttackDelay = 0.02f;
        
        attackDelayTimer = defaultAttackDelay;
    }

    void Update()
    {
        //Old animation controller script, got messy so I had to redo it
        /*
        if(Input.GetButtonDown("Fire1"))
        {
            if(animator.GetBool("AttackInitiate") == false)
            {
                animator.SetBool("AnimationState", true);
                animator.SetBool("AttackInitiate", true);
                AttackTimerEnable = true;
            }
            else if(info.IsName("WeaponComboWindow"))
            {
                animator.SetBool("AttackAgain", true);
            }
        }
        if(AttackTimerEnable) AttackTimer -= Time.deltaTime;
        if(animator.GetBool("AttackAgain") && AttackTimer <=0)
        {
            animator.SetBool("AttackAgain", false);
            AttackTimer = 2*defaultTime;
        }
        if(AttackTimer <= 0)
        {
            animator.SetBool("AnimationState", false);
            animator.SetBool("AttackInitiate", false);
            AttackTimerEnable = false;
            AttackTimer = defaultTime;;
        }*/

        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        SetSpeed(info);

        //Reset all values if we are back at the original position
        if(info.IsName("Idle") && animator.GetBool("AttackAgain") == false) animator.SetBool("AttackCycle", false);

        //if by any chance you already started the next attack, disable attack again so you dont activate the next next attack.
        if(info.IsName("WeaponRebound1") || info.IsName("WeaponSwing1")) animator.SetBool("AttackAgain", false);
 
        //If were back at the original position start the attack, or we are in the combo window do another attack allow held fire button if speed too high
        if(Input.GetButtonDown("Fire1") || Input.GetButton("Fire1") && recoverySpeed >= 2.5f)
        {
            if(info.IsName("Idle"))
            {
                animator.SetBool("AttackCycle", true);
            }
            else if(info.IsName("WeaponComboWindow") || info.IsName("WeaponRebound2"))
            {
                animator.SetBool("AttackAgain", true);
                animator.SetBool("AttackCycle", true);
            }
        }

        //when we are in an attacking animation
        if(info.IsName("WeaponSwing1") || info.IsName("WeaponSwing2"))
        {
            //start the timer for when to actually deal damage (preferably at he peak of the swing)
            attackDelayTimer -= Time.deltaTime;
            
            
            if(attackDelayTimer <= 0 && canAttack)
            {
                //get an array of objects which a sphere raycast hit 
                RaycastHit[] hits;
                hits = Physics.SphereCastAll(cameraView.transform.position, 0.5f, cameraView.transform.forward, range);

                targetCount = 0;
                foreach(var item in hits)
                {
                    //looking though all the objects (which is troublesome because there are 50-100 whith the each swing) select the object which we want and proceed only if the time to 
                    //deal damage is right.
                    if (item.collider.tag == "DestructableProp" || item.collider.tag == "Enemy")
                    {
                        //deal damage to an item, this item will no longer be able to take damage this cycle and only the other j number of objects will also receive damage once if there
                        //are any
                        Target target = item.transform.GetComponent<Target>();
                        if(targetCount < oneSwingTargetCount) 
                        {
                            target.TakeDamage(pendingDamage[targetCount]);
                            pendingDamage[targetCount] = 0;
                            targetCount++;
                        }

                        //to make sure it only plays the sound once per swing
                        if(canAttack) audioSource.PlayOneShot(hitSound);
                        if(canAttack && playerController.currentStamina >= 40) playerController.UseStamina(40f);
                        canAttack = false;
                        attackMiss = false;
                    }   
                }

                //shoot a raycast to get an exact position of the swing(maybe could be optimized by somehow getting the centre of our attack sphere above) to print a decal there
                GameObject decal;
                RaycastHit decalPos;
                if(Physics.Raycast(cameraView.transform.position, cameraView.transform.forward, out decalPos, range, decalLayerMask) && decalsLeft > 0 && canAttack) 
                {
                    if(playerController.currentStamina >= 40) playerController.UseStamina(40f);
                    //depending on the swing direction, change the decal direction. Since both decals are the same except mirrored, I probably could have just used one and mirrored it to
                    //get a second one, I could not figure out how to do that so I simply made two.
                    if(info.IsName("WeaponSwing1")) 
                    {
                        decal = (GameObject) Instantiate(swingDecal, decalPos.point + decalPos.normal * 0.001f, Quaternion.identity);
                    }
                    else 
                    {
                        decal = (GameObject) Instantiate(swingDecalLeft, decalPos.point + decalPos.normal * 0.001f, Quaternion.identity);
                    }
                    
                    var particle = Instantiate(hitParticle, decalPos.point + decalPos.normal * 0.001f,Quaternion.identity);
                    particle.transform.LookAt(decalPos.point + decalPos.normal);
                    //make decal look exactly at you
                    decal.transform.LookAt(decalPos.point + decalPos.normal);
                    //destroy decal after 10 secs
                    Destroy(decal, 10f);
                    //Destroy(particle, 10f);
                    //to make sure it plays the sound only once per swing
                    if(decalsLeft>0) audioSource.PlayOneShot(hitSound);
                    //to make sure it prints the decal only once per swing
                    canAttack = false;
                    attackMiss = false;
                    decalsLeft--;
                }
            }

            if(attackDelayTimer <= 0 && swingSpeed >= 0.2f) 
            {
                //play a swing sound if the attack didnt touch anything
                if(attackMiss)
                {
                    if(playerController.currentStamina >= 40) playerController.UseStamina(40f);
                    audioSource.PlayOneShot(swingSound);
                    attackMiss = false;
                }
                //start rendering trail
                trailRender.enabled = true;
            }
        }
        else 
        {
            //if no attack is being launched, reset all pending damage numbers and reset the attack delay.
            for(int i=0; i<oneSwingTargetCount; i++) pendingDamage[i] = damage;
            attackDelayTimer = defaultAttackDelay;
            trailRender.enabled = false;
            canAttack = true;
            attackMiss = true;
            decalsLeft = 1;
        }
    }
    //Just to be able to control the speed at which the animations run
    private void SetSpeed(AnimatorStateInfo info)
    {
        if(info.IsName("WeaponSwing1") || info.IsName("WeaponSwing2"))
        {
            animator.speed = swingSpeed;
        }
        else animator.speed = recoverySpeed;
    }

    //Dealing damage upon colliding with the weapon didnt work, because it was very unintuitive, registering hits was inconsistent and random, many times when you would think it 
    //should hit it doesn't. I leave this here because it may be useful someday. Update: It did work eventually, however I had already developed spherecast system.

    /*
    public void OnTriggerEnter(Collider col)
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (col.gameObject.tag == "DestructableProp" && (info.IsName("WeaponSwing1") || info.IsName("WeaponSwing2")))
        {
            Target target = col.gameObject.transform.GetComponent<Target>();
            target.TakeDamage(damage);
            Debug.Log(target.health);
        }
    }
    */
}

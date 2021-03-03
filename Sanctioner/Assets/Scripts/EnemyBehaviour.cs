using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehaviour : MonoBehaviour
{
    public float spotRadius = 5f;
    public float attackRadius = 1f;
    public GameObject player;
    public float fieldOfView = 170f;
    public float rotationSpeed = 5f;

    Transform target;
    NavMeshAgent agent;
    Animator animator;
    //Vector3 personalLastSighting;
    bool playerSpotted;
    bool goodSwingTrajectory;
    Vector3 playerSighting;
    Vector3 lookDirection;

    PlayerController targetController;

    float alertTimer = 10f;

    float attackTimer = 0;


    // Start is called before the first frame update
    void Start()
    {
        target = player.transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        targetController = player.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        float distance = Vector3.Distance(target.position, transform.position);

        if(info.IsName("Armature|Run") || info.IsName("Armature|PlayerSpotted")) agent.SetDestination(target.position);
        else agent.SetDestination(transform.position);
        if(distance < 1.6f) agent.SetDestination(transform.position);

        if(playerSpotted)
        {
            animator.SetBool("PlayerSpotted", true);
            lookDirection = (player.transform.position - transform.position).normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * rotationSpeed);
        }
        else animator.SetBool("PlayerSpotted", false);

        if(distance > attackRadius)
        {
            animator.SetBool("PlayerIsFar", true);
        }
        else animator.SetBool("PlayerIsFar", false);

        
        if(distance <= attackRadius && goodSwingTrajectory)
        {
            animator.SetBool("CanAttack", true);
            
            //Debug.DrawRay(transform.position + transform.up, transform.rotation * Vector3.forward, Color.green, 2f);
            //if in range, deal damage
            RaycastHit hit;
            if(Physics.Raycast(transform.position + transform.up, transform.rotation * Vector3.forward, out hit, 2f))
            {
                if(hit.collider.gameObject == player && attackTimer <= 0 && info.IsName("Armature|Attack"))
                {
                    targetController.TakeDamage(4);
                    attackTimer = 2f;
                }
            }
        }
        else animator.SetBool("CanAttack", false);
        
        alertTimer -= Time.deltaTime;
        attackTimer -= Time.deltaTime;
    }
    void OnTriggerStay(Collider col)
    {
        if(col.gameObject == player)
        {
            playerSpotted = false;

            Vector3 direction = col.transform.position - transform.position;
            float angle = Vector3.Angle(direction, transform.rotation * Vector3.forward);

            if(angle < fieldOfView/2 || alertTimer <= 0)
            {
                alertTimer = 0;
                RaycastHit hit;
                if(Physics.Raycast(transform.position, direction.normalized, out hit))
                {
                    //Debug.DrawRay(transform.position, direction.normalized * 100000f, Color.red, 20f);
                    if(hit.collider.gameObject == player || hit.collider.tag == "DestructableProp" || hit.collider.tag == "Enemy")
                    {
                        playerSpotted = true;
                        playerSighting = player.transform.position;
                        if(angle < fieldOfView/3) goodSwingTrajectory = true;
                        else goodSwingTrajectory = false;
                    }
                }
            }
        }
    }
    void OnTriggerExit(Collider col)
    {
        if(col.gameObject == player)
        {
            playerSpotted = false;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if(col.collider.tag == "DestructableProp")
        {
            Target target = col.transform.GetComponent<Target>();
            target.TakeDamage(15f);
        }
    }
}

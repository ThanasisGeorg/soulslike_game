using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BearActions : MonoBehaviour
{
    [Header("Bear")]
    [SerializeField] private Transform bear;
    [SerializeField] private Transform bearObj;
    [SerializeField] private Transform orientation;

    [Header("Movement")]
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float groundDrag;

    [Header("Ground Check")]
    [SerializeField] private float bearHeight;
    [SerializeField] private LayerMask ground;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerObj;

    [Header("Distance")]
    [SerializeField] private float chaseDistance;
    [SerializeField] private float stopDistance;

    private Rigidbody rb;
    private NavMeshAgent m_Agent;
    public Transform[] goals = new Transform[3];
    private int m_NextGoal = 1;
    private Animator _animator;
    private bool chaseTriggered = false;
    private Transform ct; 
    private PlayerStatus playerStatus;
    private BearStatus bearStatus;
    private bool isAttacking = false;

    private void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        _animator = bearObj.GetComponentInChildren<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerStatus = FindObjectOfType<PlayerStatus>();
        bearStatus = GetComponent<BearStatus>();
    }

    void Update()
    {
        if (bearStatus.currentHealth == 0)
        {
            bear.gameObject.SetActive(false);
        }
        else
        {
            if (chaseTriggered == false)
            {
                Loop();
            }
            else
            {
                ChaseHanldling();
            }
        }
    }

    private void ChaseHanldling()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        LookAtPlayer();

        if (distance > stopDistance)
        {
            if (isAttacking)
            {
                ResetAttack();
            }
            StartRunning();
            SetDestination();
        }
        else
        {
            m_Agent.ResetPath();
            StopMoving();
            Attack();           
            Invoke(nameof(ResetAttack), 1.5f);
        }
    }

    private void SetDestination()
    {
        m_Agent.SetDestination(player.position);
    }

    private void LookAtPlayer()
    {
        Vector3 direction = player.position - transform.position;
        direction.y = 0f; // Optional: keep only horizontal rotation
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f); // Smooth rotation
        }
    }

    private void Loop()
    {
        float distance = Vector3.Distance(m_Agent.transform.position, goals[m_NextGoal].transform.position);
        if (distance < 3f)
        {
            _animator.SetFloat("speed", m_Agent.speed);
            m_NextGoal = m_NextGoal != 2 ? m_NextGoal + 1 : 0;
        }
        m_Agent.destination = goals[m_NextGoal].transform.position;
    }

    private void Push()
    {
        // players orientation
        Vector3 inputDir = orientation.right + orientation.forward;
        if (inputDir != Vector3.zero) 
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);

        // get back direction from player's orientation and add force
        Vector3 backDirection = -playerObj.forward;
        rb.velocity = Vector3.zero;
        rb.AddForce(backDirection * 5f, ForceMode.Impulse);
    }

    private void StartWalking()
    {
        m_Agent.speed = 1.5f;
        _animator.SetFloat("speed", m_Agent.speed);
    }

    private void StartRunning()
    {
        m_Agent.speed = 4;
        _animator.SetFloat("speed", m_Agent.speed);
        SetDestination();
    }

    private void StopMoving()
    {
        m_Agent.speed = 0;
        _animator.SetFloat("speed", m_Agent.speed);
    }

    private void Attack()
    {
        isAttacking = true;
        _animator.SetBool("timeToAttack", true);
        //Push();
    }

    private void Hit()
    {
        playerStatus.TakeDamage(10f);
    }

    private void ResetAttack()
    {
        isAttacking = false;
        _animator.SetBool("timeToAttack", false);
    }

    private void ReadyToFight()
    {
        _animator.SetBool("readyToFight", true);
    }

    private void ResetFighting()
    {
        _animator.SetBool("readyToFight", false);
        ResetAttack();
        StartRunning();
    }

    public void StartChasing()
    {
        Debug.Log("chase begins");
        chaseTriggered = true;
        StartRunning();
    }

    public void StopChasing()
    {
        Debug.Log("chase stops");
        chaseTriggered = false;
        StartWalking();
    }


}

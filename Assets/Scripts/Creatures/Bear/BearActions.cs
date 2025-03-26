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

    private bool grounded;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb;
    private NavMeshAgent m_Agent;
    public Transform[] goals = new Transform[3];
    private int m_NextGoal = 1;


    // Start is called before the first frame update
    private void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        //rb = GetComponent<Rigidbody>();
        //rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        float distance = Vector3.Distance(m_Agent.transform.position, goals[m_NextGoal].position);
        if (distance < 0.5f)
        {
            m_NextGoal = m_NextGoal != 2 ? m_NextGoal + 1 : 0;
        }
        m_Agent.destination = goals[m_NextGoal].position;
        //Loop();
    }

    private bool IsGrounded()
    {
        return grounded = Physics.Raycast(bearObj.position, Vector3.down, bearHeight / 2, ground);
    }

    private void Loop()
    {
        float distance = Vector3.Distance(m_Agent.transform.position, goals[m_NextGoal].position);
        if (distance < 0.5f)
        {
            m_NextGoal = m_NextGoal != 2 ? m_NextGoal + 1 : 0;
        }
        m_Agent.destination = goals[m_NextGoal].position;
    }

}

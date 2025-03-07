using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerObj;
    [SerializeField] private GameObject playerCam;
    [SerializeField] private Transform armature;

    [SerializeField] private float rotationSpeed;
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float groundDrag;
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMultiplier;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;

    [Header("Keybinds")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private Transform orientation;

    private bool grounded;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb;

    private Animator _animator;
    

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        _animator = playerObj.GetComponent<Animator>();
    }

    private void Update()
    {
        PlayerInput();
        
        //SpeedControl();

        Debug.Log("Grounded: " + IsGrounded());
        // ground check
        if (IsGrounded())
            rb.drag = groundDrag;          
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        Rotate();
        Run(); 
        AnimateRunMovement();
    }

    private bool IsGrounded()
    {
        return grounded = Physics.Raycast(playerObj.position, Vector3.down, (playerHeight / 2), whatIsGround);
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if((Gamepad.current.buttonEast.wasPressedThisFrame || Input.GetKeyUp(jumpKey)) && grounded)
        {
            Jump();
        }
    }

    private void Run()
    {
        // calculate movement direction
        moveDirection = horizontalInput * orientation.right + verticalInput * orientation.forward;        

        // on ground
        if(grounded) 
        {
            rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);
        }
        // in air
        else if(!grounded)
            rb.AddForce(moveDirection * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void Rotate()
    {
        // rotate orientation
        Vector3 viewDir = player.position - new Vector3(playerCam.transform.position.x, player.position.y, playerCam.transform.position.z);
        orientation.forward = viewDir; 

        // rotate player object
        //horizontalInput = Input.GetAxis("Horizontal");
        //verticalInput = Input.GetAxis("Vertical");
        Vector3 inputDir = horizontalInput * orientation.right + verticalInput * orientation.forward;

        // execute rotation
        if (inputDir != Vector3.zero) 
        {
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void AnimateRunMovement()
    {
        float distanceCovered = new Vector2(horizontalInput, verticalInput).magnitude;
        float speed = Mathf.Lerp(_animator.GetFloat("speed"), distanceCovered, Time.deltaTime * 10f);
        _animator.SetFloat("speed", speed);
        
    }

    private void AnimateJumpMovement()
    {
        float distanceCovered = new Vector2(horizontalInput, verticalInput).magnitude;
        float speed = Mathf.Lerp(_animator.GetFloat("speed"), distanceCovered, Time.deltaTime * 10f);
        _animator.SetFloat("speed", speed);
        
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }
}
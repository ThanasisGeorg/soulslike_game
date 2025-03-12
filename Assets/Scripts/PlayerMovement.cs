using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerObj;
    [SerializeField] private GameObject playerCam;

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
    [SerializeField] private LayerMask ground;
    [SerializeField] private Transform orientation;

    private bool grounded;
    private bool readyToBackStep = true;
    private bool readyToRoll = true;
    private bool readyToAttack = true;
    private bool readyToBlock = true;
    private bool isAttacking = false;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb;

    private Animator _animator;
    

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        _animator = playerObj.GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        PlayerInput();
        
        //SpeedControl();

        Debug.Log("Grounded: " + IsGrounded());
        // ground check
        if (IsGrounded())
        {
            rb.drag = groundDrag;          
        }
        else 
        {
            rb.drag = 0;
        }
    }

    private void FixedUpdate()
    {
        if(!isAttacking)
        {
            Rotate();
            Run(); 
        }
    }

    private bool IsGrounded()
    {
        return grounded = Physics.Raycast(playerObj.position, Vector3.down, (playerHeight / 2), ground);
    }

    private void PlayerInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Gamepad.current.leftStick.ReadValue().magnitude > 0.1f && Gamepad.current.buttonEast.wasPressedThisFrame && grounded)
        {
            Debug.Log("Time to roll");
            Roll();
        }
        else if((Gamepad.current.buttonEast.wasPressedThisFrame || Input.GetKeyUp(jumpKey)) && grounded)
        {
            Debug.Log("Time to backstep");
            //Backstep();
        }
        if(Gamepad.current.rightShoulder.wasPressedThisFrame && grounded)
        {
            Debug.Log("Time to attack");
            Attack();
        }
        if(Gamepad.current.leftShoulder.isPressed && grounded)
        {
            Debug.Log("Time to block");
            Block();
        }
        if(Gamepad.current.leftShoulder.wasReleasedThisFrame)
        {
            readyToBlock = false;
            ResetBlock();
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

        AnimateRun();
    }

    private void AnimateRun()
    {
        float distanceCovered = new Vector2(horizontalInput, verticalInput).magnitude;
        float speed = Mathf.Lerp(_animator.GetFloat("speed"), distanceCovered, Time.deltaTime * 10f);
        _animator.SetFloat("speed", speed);
    }

    private void Rotate()
    {
        // rotate orientation
        Vector3 viewDir = player.position - new Vector3(playerCam.transform.position.x, player.position.y, playerCam.transform.position.z);
        orientation.forward = viewDir; 

        // rotate player object
        Vector3 inputDir = horizontalInput * orientation.right + verticalInput * orientation.forward;

        // execute rotation
        if (inputDir != Vector3.zero) 
        {
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
        }
    }

    private void Roll()
    {
        if(!readyToRoll) return;

        moveDirection = horizontalInput * orientation.right + verticalInput * orientation.forward;

        AnimateRoll();
        rb.AddForce(moveDirection * (moveSpeed + 2f) * 10f, ForceMode.Force);

        readyToRoll = false;
        Invoke(nameof(ResetRoll), 1f);
    }

    private void AnimateRoll()
    {
        _animator.SetBool("readyToRoll", true);  
    }

    private void ResetRoll()
    {
        readyToRoll = true;
        _animator.SetBool("readyToRoll", false);  
    }

    private void Backstep()
    {
        if(!readyToBackStep) return;
        
        AnimateBackstep();
        Invoke(nameof(ExecuteBackstep), 0.1f);

        readyToBackStep = false;
        Invoke(nameof(ResetBackStep), 0.3f);
    }

    private void AnimateBackstep()
    {
        _animator.SetBool("readyToBackstep", true);  
    }   

    private void ExecuteBackstep()
    {
        Vector3 backDirection = -player.forward;
        rb.velocity = Vector3.zero;
        rb.AddForce(backDirection * 40f, ForceMode.Impulse);
    }

    private void ResetBackStep()
    {
        readyToBackStep = true;
        _animator.SetBool("readyToBackstep", false);
    } 

    private void Attack()
    {
        if(!readyToAttack) return;

        AnimateAttack();

        Invoke(nameof(ResetAttack), 0.5f);
    }

    private void AnimateAttack()
    {
        isAttacking = true;
        _animator.SetBool("readyToAttack", true);
    }

    private void ResetAttack()
    {
        isAttacking = false;
        readyToAttack = true;
        _animator.SetBool("readyToAttack", false);
    }

    private void Block()
    {
        if(!readyToBlock) return;

        AnimateBlock();
    }

    private void AnimateBlock()
    {
        _animator.SetBool("readyToBlock", true);
    }

    private void ResetBlock()
    {
        readyToBlock = true;
        _animator.SetBool("readyToBlock", false);
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
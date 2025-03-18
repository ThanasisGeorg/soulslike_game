using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerActions : MonoBehaviour
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

    private float tapDuration = 0.2f;
    private float buttonEastPressTime = 0f;
    private bool buttonEastHeld = false;
    private bool grounded;
    private bool readyToBackStep = true;
    private bool readyToRoll = true;
    private bool readyToAttack1 = true;
    private bool readyToAttack2 = false;
    private bool readyToBlock = true;
    private bool readyToBlockWhileWalking = true;
    private bool readyToFall = false;
    private bool readyToSprint = true;
    private bool isAttacking = false;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb;

    private PlayerStatus playerStatus; 
    private Animator _animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        playerStatus = FindObjectOfType<PlayerStatus>();
        _animator = playerObj.GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        PlayerInput();

        Debug.Log("Grounded: " + IsGrounded());
        // ground check
        if (IsGrounded())
        {
            rb.drag = groundDrag;   
            ResetFalling();       
        }
        else 
        {
            rb.drag = 0;
            AnimateFalling();
        }
    }

    private void FixedUpdate()
    {
        if(!isAttacking || readyToFall)
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
        Debug.Log("Button East value: " + Gamepad.current.buttonEast.ReadValue());
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if(Gamepad.current.leftStick.ReadValue().magnitude > 0.1f && grounded)
        {
            ButtonEastHandlingWhileMoving();
            if(Gamepad.current.leftShoulder.isPressed)
            {
                Debug.Log("Time to block while walking");
                BlockWhileWalking();
            }   
        }
        else if(Gamepad.current.buttonEast.wasPressedThisFrame || Input.GetKeyUp(jumpKey))
        {
            Debug.Log("Time to backstep");
            Backstep();
            
        }
        if(Gamepad.current.rightShoulder.wasPressedThisFrame)
        {
            Debug.Log("Time to attack 1");
            Attack1();
        }
        if(Gamepad.current.leftShoulder.isPressed && grounded)
        {
            Debug.Log("Time to block");
            Block();
        }
        if(Gamepad.current.leftStick.ReadValue().magnitude == 0 || Gamepad.current.buttonEast.wasReleasedThisFrame)
        {
            ResetSprint();
        }
        if(Gamepad.current.leftShoulder.wasReleasedThisFrame)
        {
            ResetBlock();
        }
        if(Gamepad.current.leftStick.ReadValue().magnitude == 0 || Gamepad.current.leftShoulder.wasReleasedThisFrame)
        {
            ResetBlockWhileWalking();
        }
        if(playerStatus.currentStamina < 100f && Gamepad.current.buttonEast.ReadValue() == 0) 
            if(readyToRoll == false || readyToBackStep == false || readyToAttack1 == false)
                Invoke(nameof(playerStatus.RefillStamina), 1f);
            else playerStatus.RefillStamina();
    }

    private void ButtonEastHandlingWhileMoving()
    {
        if(Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            buttonEastPressTime = Time.time;
            buttonEastHeld = true;
        }

        if(Gamepad.current.buttonEast.wasReleasedThisFrame)
        {
            if((Time.time - buttonEastPressTime) < tapDuration)
            {
                Debug.Log("Time to roll");
                Roll();
                playerStatus.ConsumeStamina(20f);
            }

            buttonEastHeld = false;
        }

        if(buttonEastHeld && (Time.time - buttonEastPressTime) >= tapDuration)
        {
            if(playerStatus.currentStamina > 0)
            {
                Debug.Log("Time to sprint");
                Sprint();
                playerStatus.ConsumeStamina(2f);
            }
            else ResetSprint();
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

    private void Sprint()
    {
        if(!readyToSprint) return;

        readyToSprint = false;

        moveDirection = horizontalInput * orientation.right + verticalInput * orientation.forward;
        rb.AddForce(moveDirection * moveSpeed * 20f, ForceMode.Force);

        AnimateSprint();
    }

    private void AnimateSprint()
    {
        _animator.SetBool("readyToSprint", false);
    }

    private void ResetSprint()
    {
        readyToSprint = true;
        _animator.SetBool("readyToSprint", true);
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

        readyToRoll = false;

        moveDirection = horizontalInput * orientation.right + verticalInput * orientation.forward;

        AnimateRoll();
        rb.AddForce(moveDirection * (moveSpeed + 2f) * 10f, ForceMode.Force);

        Invoke(nameof(ResetRoll), 1f);
    }

    private void AnimateRoll()
    {
        _animator.SetBool("readyToRoll", false);  
    }

    private void ResetRoll()
    {
        readyToRoll = true;
        _animator.SetBool("readyToRoll", true);  
    }

    private void Backstep()
    {
        if(!readyToBackStep) return;
        
        readyToBackStep = false;
        
        playerStatus.ConsumeStamina(20f);

        AnimateBackstep();
        Invoke(nameof(ExecuteBackstep), 0.15f);

        Invoke(nameof(ResetBackStep), 0.3f);
    }

    private void AnimateBackstep()
    {
        _animator.SetBool("readyToBackstep", false);  
    }   

    private void ExecuteBackstep()
    {
        // players orientation
        Vector3 inputDir = horizontalInput * orientation.right + verticalInput * orientation.forward;
        if (inputDir != Vector3.zero) 
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);

        // get back direction from player's orientation and add force
        Vector3 backDirection = -playerObj.forward;
        rb.velocity = Vector3.zero;
        rb.AddForce(backDirection * 40f, ForceMode.Impulse);
    }

    private void ResetBackStep()
    {
        readyToBackStep = true;
        _animator.SetBool("readyToBackstep", true);
    } 

    private void Attack1()
    {
        if(!readyToAttack1) return;

        readyToAttack1 = false;
        
        playerStatus.ConsumeStamina(40f);

        AnimateAttack1();
        Invoke(nameof(ExecuteAttack1), 0.5f);

        Invoke(nameof(ResetAttack1), 2.2f);
    }

    private void AnimateAttack1()
    {        
        isAttacking = true;
        _animator.SetBool("readyToAttack1", false);
    }

    private void ExecuteAttack1()
    {
        // players orientation
        Vector3 inputDir = horizontalInput * orientation.right + verticalInput * orientation.forward;
        if (inputDir != Vector3.zero) 
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);

        // get front direction from player's orientation and add force
        Vector3 frontDirection = playerObj.forward;

        rb.AddForce(frontDirection * 10f, ForceMode.Impulse);
    }

    private void ResetAttack1()
    {
        isAttacking = false;
        readyToAttack1 = true;
        _animator.SetBool("readyToAttack1", true);
    }

    private void Attack2()
    {
        //if(readyToAttack2) return;

        AnimateAttack2();
        Invoke(nameof(ExecuteAttack2), 0.5f);

        Invoke(nameof(ResetAttack2), 2.2f);
    }

    private void AnimateAttack2()
    {        
        isAttacking = true;
        _animator.SetBool("readyToAttack2", true);
    }

    private void ExecuteAttack2()
    {
        // players orientation
        Vector3 inputDir = horizontalInput * orientation.right + verticalInput * orientation.forward;
        if (inputDir != Vector3.zero) 
            playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);

        // get front direction from player's orientation and add force
        Vector3 frontDirection = playerObj.forward;

        rb.AddForce(frontDirection * 10f, ForceMode.Impulse);
    }

    private void ResetAttack2()
    {
        isAttacking = false;
        readyToAttack2 = false;
        _animator.SetBool("readyToAttack2", false);
    }

    private void Block()
    {
        if(!readyToBlock) return;

        AnimateBlock();
    }

    private void AnimateBlock()
    {
        _animator.SetBool("readyToBlock", false);
    }

    private void ResetBlock()
    {
        readyToBlock = true;
        _animator.SetBool("readyToBlock", true);
    }

    private void BlockWhileWalking()
    {
        if(!readyToBlockWhileWalking) return;

        _animator.SetBool("readyToBlockWhileWalking", true);
    }

    private void ResetBlockWhileWalking()
    {
        readyToBlockWhileWalking = true;
        _animator.SetBool("readyToBlockWhileWalking", false);
    }

    private void AnimateFalling()
    {
        readyToFall = true;
        _animator.SetBool("readyToFall", true);
    }

    private void ResetFalling()
    {
        readyToFall = false;
        _animator.SetBool("readyToFall", false);
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
using UnityEngine;
using UnityEngine.InputSystem; // Tambahan agar InputAction tidak error

public class FPSMovement : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float runSpeed = -6.0f;
    [SerializeField] private float crouchSpeed = -2.5f;

    [Header("Jump and Fall")]
    [SerializeField] private float jumpForce = 1.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float initialFallVelocity = 2.5f;

    [Header("Crouching")]
    [SerializeField] private float StandingHeight = 2f;
    [SerializeField] private float CrouchingHeight = 1f;
    [SerializeField] private float CrouchTransitionSpeed = 10f;
    [SerializeField] private float cameraOffset = 0.5f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference sprintAction;

    private CharacterController _characterController;
    private Vector2 _moveInput;
    private bool _isGrounded;
    private bool _isCrouching;
    private bool _isSprinting;
    private float _verticalVelocity;
    private float _targetHeight;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _targetHeight = StandingHeight;
    }

    // Menggunakan 'O' besar agar dikenali Unity
    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        sprintAction.action.Enable();
        crouchAction.action.Enable();

        moveAction.action.performed += StoreMovementInput;
        moveAction.action.canceled += StoreMovementInput;
        jumpAction.action.performed += Jump;
        sprintAction.action.performed += Sprint;
        sprintAction.action.canceled += Sprint;
        crouchAction.action.performed += Crouch;

    }

    // Menggunakan 'O' besar agar dikenali Unity
    private void OnDisable()
    {
        moveAction.action.performed -= StoreMovementInput;
        moveAction.action.canceled -= StoreMovementInput;
        jumpAction.action.performed -= Jump;
        sprintAction.action.performed -= Sprint;
        sprintAction.action.canceled -= Sprint;
        crouchAction.action.performed -= Crouch;

        jumpAction.action.Disable();
        moveAction.action.Disable();
        sprintAction.action.Disable();
        crouchAction.action.Disable();
    }


    private void Update()
    {
        _isGrounded = _characterController.isGrounded;
        HandleGravity();
        HandleMovement();
        HandleCrouchTransition();
    }

    // Menggunakan 'S' besar agar sama dengan yang didaftarkan di atas
    private void StoreMovementInput(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }


    private void Jump(InputAction.CallbackContext context)
    {
        if (_isGrounded)
        {
            _verticalVelocity = jumpForce;
        }
    }


    private void Crouch(InputAction.CallbackContext context)
    {
        if (_isCrouching)
        {
            if (!CanStandUp())
            {
                return;
            }
            _targetHeight = StandingHeight;
        }
        else
        {
            _targetHeight = CrouchingHeight;
        }


        _isCrouching = !_isCrouching;
    }

    private bool CanStandUp()
    {
        return !Physics.CapsuleCast(
            transform.position + _characterController.center,
            transform.position + (Vector3.up * _characterController.height / 2),
            _characterController.radius,
            Vector3.up);
    }


    private void Sprint(InputAction.CallbackContext context)
    {
        _isSprinting = context.performed;
    }

    private void HandleGravity()
    {
        if (_isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -2f;
        }

        _verticalVelocity += gravity * Time.deltaTime;

    }

    private void HandleMovement()
    {
        var move = cameraTransform.TransformDirection(new Vector3(_moveInput.x, 0, _moveInput.y)).normalized;
        var currentSpeed = _isCrouching ? crouchSpeed : _isSprinting ? runSpeed : walkSpeed;
        var finalMove = move * currentSpeed;
        finalMove.y = _verticalVelocity;

        var collision = _characterController.Move(finalMove * Time.deltaTime);
        if ((collision & CollisionFlags.Above) != 0)
        {
            _verticalVelocity = 0;
        }
    }

    private void HandleCrouchTransition()
    {
       var currentHeight = _characterController.height;
       if (Mathf.Abs(currentHeight - _targetHeight) < 0.01f)
       {
        _characterController.height = _targetHeight;
       }

       var newHeight = Mathf.Lerp(currentHeight, _targetHeight, CrouchTransitionSpeed * Time.deltaTime);
       _characterController.height = newHeight;
        _characterController.center = Vector3.up * (newHeight * 0.5f);

       var cameraTargetPosition = cameraTransform.localPosition;
       cameraTargetPosition.y = _targetHeight - cameraOffset;
       cameraTransform.localPosition = Vector3.Lerp(
        cameraTransform.localPosition, 
        cameraTargetPosition, 
        CrouchTransitionSpeed * Time.deltaTime);
    }
}

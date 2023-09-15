using System;
using Cinemachine;
using Spells;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Player
{
    [RequireComponent(typeof(PlayerInputHolder))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(SpellHolder))]
    public class PlayerController : NetworkBehaviour
    { 
        [Header("Speed")]
        [SerializeField] private float walkSpeed = 1.6f;
        [SerializeField] private float moveSpeed = 2.7f;
        [SerializeField] private float sprintSpeed = 5.0f;
        [SerializeField] private float aimSpeed = 2.4f;
        [SerializeField] private float rotationSmoothTime = 0.12f;
        [SerializeField] private float speedChangeRate = 10.0f;
    
        [Header("Audio")]
        [SerializeField] private AudioClip landingAudioClip;
        [SerializeField] private AudioClip[] footstepAudioClips;
        [SerializeField] private float footstepAudioVolume = 0.5f;

        [Header("Jump and gravity")]
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -15.0f;
        [SerializeField] private float jumpTimeout = 0.50f;
        [SerializeField] private float fallTimeout = 0.15f;
    
        [Header("Grounded")]
        [SerializeField] private bool grounded = true;
        [SerializeField] private float groundedOffset = -0.14f;
        [SerializeField] private float groundedRadius = 0.28f;
        [SerializeField] private LayerMask groundLayers;
    
        [Header("Camera")]
        [SerializeField] private GameObject cinemachineCameraTarget;
        [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
        [SerializeField] private float topClamp = 70.0f;
        [SerializeField] private float bottomClamp = -30.0f;
        [SerializeField] private float cameraAngleOverride;
        [SerializeField] private bool lockCameraPosition;
    
        [Header("Aim")]
        [SerializeField] private LayerMask aimColliderMask;
        [SerializeField] private Transform debugTransform;
        [SerializeField] private float normalSensitivity = 1f;
        [SerializeField] private float aimSensitivity = 0.5f;
    
        private float _cinemachineTargetYaw; 
        private float _cinemachineTargetPitch;

        private float _speed;
        private float _speedMultiplier = 1f;
        private float _animationBlend;
        private float _targetRotation;
        private float _rotationVelocity;
        private float _verticalVelocity;
    
        private const float TerminalVelocity = 53.0f;
        private const float SpeedOffset = 0.1f;
        private const float Threshold = 0.01f;

        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;
    
        private float _sensitivity = 1f;

        private int _animIDSpeed;
        private int _animIDSpeedX;
        private int _animIDSpeedY;
        private int _animIDAim;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDFreeFallSpeed;

        private CharacterController _controller;
        private PlayerInputHolder _input;
        private Camera _mainCamera;
        private SpellHolder _spellHolder;

        public Animator Animator { get; private set; }
        public Transform DebugTransform => debugTransform;

        private void Awake()
        {
            _mainCamera = Camera.main;
            
            Animator = GetComponent<Animator>();
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputHolder>();
            _spellHolder = GetComponent<SpellHolder>();

            AssignAnimationIDs();
        }

        public override void OnNetworkSpawn()
        {
            if(!IsOwner) return;
            
            _cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _jumpTimeoutDelta = jumpTimeout;
            _fallTimeoutDelta = fallTimeout;
        }

        private void Update()
        {
            if(!IsOwner) return;
            
            JumpAndGravity();
            GroundedCheck();
            Move();
            Aim();
            UseSpell();
            ChangeSpell();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDSpeedX = Animator.StringToHash("SpeedX");
            _animIDSpeedY = Animator.StringToHash("SpeedY");
            _animIDAim = Animator.StringToHash("Aim");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDFreeFallSpeed = Animator.StringToHash("FreeFallSpeed");
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset,
                transform.position.z);
        
            grounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers,
                QueryTriggerInteraction.Ignore);
        
            Animator.SetBool(_animIDGrounded, grounded);
        }

        private void CameraRotation()
        {
            if (_input.look.sqrMagnitude >= Threshold && !lockCameraPosition)
            {
                _cinemachineTargetYaw += _input.look.x * _sensitivity;
                _cinemachineTargetPitch += _input.look.y * _sensitivity;
            
                _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);
            }
        

            cinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + cameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            float targetSpeed = (_input.aim ? aimSpeed : _input.sprint ? sprintSpeed :
                _input.walk ? walkSpeed : moveSpeed) * _speedMultiplier;
        
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        
            if (currentHorizontalSpeed < targetSpeed - SpeedOffset ||
                currentHorizontalSpeed > targetSpeed + SpeedOffset)
            {
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed,
                    Time.deltaTime * speedChangeRate);

                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
        
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;

                if (!_input.aim)
                {
                    float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                        rotationSmoothTime);

                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }
            }

            if (_input.aim)
            {
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _mainCamera.transform.eulerAngles.y, ref _rotationVelocity,
                    rotationSmoothTime);
            
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        
            Animator.SetFloat(_animIDSpeed, _animationBlend);
        }

        private void JumpAndGravity()
        {
            if (grounded)
            {
                _fallTimeoutDelta = fallTimeout;
            
                Animator.SetBool(_animIDJump, false);
                Animator.SetBool(_animIDFreeFall, false);

                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                
                    Animator.SetBool(_animIDJump, true);
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = jumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    Animator.SetBool(_animIDFreeFall, true);
                }

                _input.jump = false;
                Animator.SetFloat(_animIDFreeFallSpeed, _verticalVelocity);
            }

            if (_verticalVelocity < TerminalVelocity)
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }
        }

        private void Aim()
        {
            if (_input.aim)
            {
                if (!aimVirtualCamera.gameObject.activeInHierarchy)
                {
                    aimVirtualCamera.gameObject.SetActive(true);
                    SetSensitivity(aimSensitivity);
                    Animator.SetBool(_animIDAim, true);
                    _spellHolder.ActivateSpellVisuals(true);
                }
            
                Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);
                Ray ray = _mainCamera.ScreenPointToRay(screenCenterPoint);
                if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, aimColliderMask))
                {
                    debugTransform.position = raycastHit.point;
                }
            
                Animator.SetLayerWeight(1, Mathf.Lerp(Animator.GetLayerWeight(1), 1f, Time.deltaTime * speedChangeRate));
            
                Animator.SetFloat(_animIDSpeedX, Mathf.Lerp(Animator.GetFloat(_animIDSpeedX),
                    _input.move.x, Time.deltaTime * speedChangeRate));
                Animator.SetFloat(_animIDSpeedY, Mathf.Lerp(Animator.GetFloat(_animIDSpeedY),
                    _input.move.y, Time.deltaTime * speedChangeRate));

            }
            else
            {
                if (aimVirtualCamera.gameObject.activeInHierarchy)
                {
                    aimVirtualCamera.gameObject.SetActive(false);
                    SetSensitivity(normalSensitivity);
                    Animator.SetBool(_animIDAim, false);
                    _spellHolder.ActivateSpellVisuals(false);
                }
            
                Animator.SetLayerWeight(1, Mathf.Lerp(Animator.GetLayerWeight(1), 0f, Time.deltaTime * speedChangeRate));
            }
        }

        private void UseSpell()
        {
            if (_input.useSpell && _input.aim)
            {
                _spellHolder.UseSpell();
            }
        }

        private void ChangeSpell()
        {
            if (_spellHolder.CurrentSpellNumber != _input.currentSpell)
            {
                _spellHolder.ChangeSpell(_input.currentSpell, _input.aim);
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) 
                lfAngle += 360f;
        
            if (lfAngle > 360f) 
                lfAngle -= 360f;
        
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void SetSensitivity(float newSensitivity)
        {
            _sensitivity = newSensitivity;
        }
    
        public void SetSpeedMultiplier(float newSpeedMultiplier)
        {
            _speedMultiplier = newSpeedMultiplier;
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z),
                groundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (footstepAudioClips.Length > 0)
                {
                    var index = Random.Range(0, footstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(footstepAudioClips[index], transform.TransformPoint(_controller.center), footstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(landingAudioClip, transform.TransformPoint(_controller.center), footstepAudioVolume);
            }
        }
    }
}
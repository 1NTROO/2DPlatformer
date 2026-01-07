using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using System.ComponentModel;
using UnityEngine.Analytics;
using Cinemachine;
using Platformer.View;
using UnityEngine.UI;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;
        public AudioClip gravityInverseAudio;
        public AudioClip landAudio;
        public AudioClip normalLandAudio;

        public GameObject playerFlipImage;

        [Header("Player Parameters")]
        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;
        /// <summary>
        /// Friction applied to horizontal movement.
        /// </summary>
        public float friction = 0.6f;

        /// <summary>
        /// Coyote time duration in seconds.
        /// </summary>
        public float coyoteTime = 0.2f;
        private float coyoteTimeCounter;

        /// <summary>
        /// Jump buffer duration in seconds.
        /// </summary>
        public float jumpBufferTime = 0.2f;
        private float jumpBufferCounter;

        /// <summary>
        /// Player direction enum
        /// </summary>
        public enum PlayerDirection
        {
            Right,
            Left,
            Up,
            Down
        }

        /// <summary>
        /// Horizontal direction the player is facing.
        /// </summary>
        public PlayerDirection horizontalDirection = PlayerDirection.Right;
        /// <summary>
        /// Vertical direction the player is facing.
        /// </summary>
        public PlayerDirection verticalDirection = PlayerDirection.Up;

        [Header("Wall Slide Parameters")]
        /// <summary>
        /// Wall slide speed limit
        /// </summary>
        public float wallSlideSpeed = 3f;
        [SerializeField] private bool isWallSliding;

        /// <summary>
        /// Layer mask for wall detection
        /// </summary>
        [SerializeField] private LayerMask wallLayer;

        /// <summary>
        /// Transforms for wall checks
        /// </summary>
        [SerializeField] private List<Transform> wallCheck;

        [Header("Wall Jump Parameters")]
        private bool isWallJumping;
        private bool wallJump;
        [SerializeField] private float wallJumpDirection;

        /// <summary>
        /// Time allowed to wall jump after leaving the wall
        /// </summary>
        [SerializeField] private float wallJumpingTime = 0.2f;

        /// <summary>
        /// Duration of wall jump invincibility
        /// </summary>
        [SerializeField] private float wallJumpingDuration = 0.4f;
        private float wallJumpingDurationCounter;

        /// <summary>
        /// Power of wall jump in x and y directions
        /// </summary>
        [SerializeField] private Vector2 wallJumpingPower = new Vector2(8f, 16f);

        [Header("Gravity Inversion")]
        /// <summary>
        /// Direction of gravity: 1 for normal, -1 for inverted
        /// </summary>
        [SerializeField] private bool hasLandedAfterGravityInverse { get; set; } = true;

        [Header("Player States")]
        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        [SerializeField] private CinematicCameraSwitcher cinematicCameraSwitcher;
        public Health health;
        public bool controlEnabled = true;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();
        private CinemachineImpulseSource impulseSource;
        private TrailRenderer trail;
        [SerializeField] private Slider chargeSlider;
        private int chargeCount = 0;
        private int maxCharge = 9;

        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            animator = GetComponentInChildren<Animator>();
            impulseSource = GetComponent<CinemachineImpulseSource>();
            trail = GetComponentInChildren<TrailRenderer>();

            chargeCount = 7;
            if (chargeSlider != null)
            {
                chargeSlider.maxValue = maxCharge;
                chargeSlider.value = chargeCount;
            }

            // Physics2D.IgnoreLayerCollision(3, 8, true);
            // cineMachine = Camera.main.GetComponent<Cinemachine.CinemachineBrain>();

            // cineMachine.m_WorldUpOverride = transform;
        }

        protected override void Update()
        {
            if (IsGrounded)
                coyoteTimeCounter = coyoteTime;
            else
                coyoteTimeCounter -= Time.deltaTime;

            if (controlEnabled)
            {
                if (Input.GetButtonDown("Jump"))
                    jumpBufferCounter = jumpBufferTime;
                else
                    jumpBufferCounter -= Time.deltaTime;

                var tempGravityDirection = GravityDirection;
                
                if (!hasLandedAfterGravityInverse && IsGrounded)
                {
                    hasLandedAfterGravityInverse = true;
                    trail.emitting = false;
                    jumpTakeOffSpeed *= -1f;
                    // cinematicCameraSwitcher.SwitchState();
                    GameController.Instance.cameraShake.TriggerShake(impulseSource);
                    AudioManager.Instance.PlayAudio(landAudio, transform.position, 0.25f, 1f);
                }
                if (!hasLandedAfterGravityInverse) 
                {
                    trail.emitting = true;
                    tempGravityDirection *= -1;
                }
                move.x = Input.GetAxis("Horizontal") * tempGravityDirection;
                if (jumpState == JumpState.Grounded && jumpBufferCounter > 0f
                    || (isWallSliding && wallJumpingDurationCounter > 0f && jumpBufferCounter > 0f))
                {
                    jumpState = JumpState.PrepareToJump;
                }
                else if (Input.GetButtonUp("Jump"))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;

                    coyoteTimeCounter = 0f;
                }

                // WallSlide();
                // WallJumpPrep();

                if (Input.GetButtonDown("InverseGravity") && hasLandedAfterGravityInverse)
                {
                    InverseGravity();
                }
            }
            else
            {
                move.x = 0;
            }
            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState()
        {
            jump = false;
            wallJump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    if (!isWallSliding) jump = true;
                    else if (isWallSliding)
                    {
                        wallJump = true;
                        isWallSliding = false;
                    }
                    stopJump = false;
                    jumpBufferCounter = 0f;
                    JumpImmunityTimer = JumpImmunityDuration;
                    
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        AudioManager.Instance.PlayAudio(normalLandAudio, transform.position);
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
        }

        protected override void ComputeVelocity()
        {
            if (jump && coyoteTimeCounter > 0f)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (wallJump && wallJumpingDurationCounter > 0f)
            {
                float closestWallDistance = float.MaxValue;
                wallJumpDirection = 0f; // Default to no direction

                foreach (var check in wallCheck)
                {
                    Collider2D wall = Physics2D.OverlapCircle(check.position, 0.25f, wallLayer);
                    if (wall != null)
                    {
                        float distanceToWall = Mathf.Abs(check.position.x - transform.position.x);
                        if (distanceToWall < closestWallDistance)
                        {
                            closestWallDistance = distanceToWall;
                            wallJumpDirection = check.position.x > transform.position.x ? -1f : 1f;
                        }
                    }
                }

                if (wallJumpDirection != 0f)
                {
                    velocity.x = wallJumpDirection * wallJumpingPower.x;
                    velocity.y = wallJumpingPower.y;
                }
                else
                {
                    Debug.Log("Wall Jump Skipped: No valid wallJumpDirection.");
                }

                isWallJumping = true;
                wallJump = false;

                targetVelocity.x = velocity.x;
            }
            if (isWallJumping)
            {
                // Blend player input with wall jump momentum after the duration
                float inputInfluence = move.x * maxSpeed;
                if (Mathf.Abs(inputInfluence) > 0.5f) velocity.x = Mathf.Lerp(velocity.x, inputInfluence, Time.deltaTime); // Reduced blending speed
                Debug.Log($"Wall Jump Input Blending: Velocity.x = {velocity.x}, Input Influence = {inputInfluence}");            
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            FlipHorizontal();

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityY", GravityDirection * velocity.y);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            // Ensure targetVelocity does not overwrite wall jump momentum immediately
            if (!isWallJumping)
            {
                targetVelocity = move * maxSpeed;
                // targetVelocity *= 1 - 0.03f * (7 - chargeCount);
            }
        }

        private void FlipHorizontal()
        {
            bool canFlip = false;
            switch (verticalDirection)
            {
                case PlayerDirection.Up:
                    if (!hasLandedAfterGravityInverse)
                    {
                        if (HorizontalFlipCheck(horizontalDirection, true))
                            canFlip = true;
                    }
                    else if (HorizontalFlipCheck(horizontalDirection, false))
                            canFlip = true;
                    break;
                case PlayerDirection.Down:
                    if (!hasLandedAfterGravityInverse)
                    {
                        if (HorizontalFlipCheck(horizontalDirection, false))
                            canFlip = true;
                    }
                    else if (HorizontalFlipCheck(horizontalDirection, true))
                            canFlip = true;
                    break;
                default:
                    break;

            }
            
            if (canFlip)
            {
                if (horizontalDirection == PlayerDirection.Right)
                    horizontalDirection = PlayerDirection.Left;
                else horizontalDirection = PlayerDirection.Right;
                Vector3 scaler = transform.localScale;
                scaler.x *= -1;
                transform.localScale = scaler;
            }
        }

        private bool HorizontalFlipCheck(PlayerDirection horizontalDir, bool upsideDown = false)
        {
            bool b = false;

            if (horizontalDir == PlayerDirection.Right)
            {
                if (upsideDown ? move.x > 0 : move.x < 0)
                    b = true;
            }
            else if (horizontalDir == PlayerDirection.Left)
            {
                if (upsideDown ? move.x < 0 : move.x > 0)
                    b = true;
            }

            return b;
        }

        private void FlipVertical()
        {
            if (verticalDirection == PlayerDirection.Down)
                verticalDirection = PlayerDirection.Up;
            else verticalDirection = PlayerDirection.Down;
            Vector3 scaler = transform.localScale;
            scaler.y *= -1;
            transform.localScale = scaler;
            
            Vector3 playerFlipImageScale = playerFlipImage.transform.localScale;
            playerFlipImageScale.y *= -1;
            playerFlipImage.transform.localScale = playerFlipImageScale;
        }

        public void InverseGravity()
        {
            // if (chargeCount <= 0) return;
            ModifyPlayerCharges(-1);
            if (chargeSlider != null)
            {
                chargeSlider.value = chargeCount;
            }

            Physics2D.gravity *= -1;
            GravityDirection *= -1;

            FlipVertical();

            SetGrounded(false);
            hasLandedAfterGravityInverse = false;

            // cinematicCameraSwitcher.SwitchState();

            AudioManager.Instance.PlayAudio(gravityInverseAudio, transform.position, 0.1f, 0.6f);
        }

        public void ModifyPlayerCharges(int amount)
        {
            chargeCount += amount;
            if (chargeCount > maxCharge) chargeCount = maxCharge;
            if (chargeCount <= 0)
            {
                chargeCount = 0;
                StartCoroutine(OutOfCharges());
            } 
            else if (chargeCount > 0) 
            {
                StopCoroutine(OutOfCharges());
            }

            if (chargeSlider != null)
            {
                chargeSlider.value = chargeCount;
            }
        }

        public IEnumerator OutOfCharges()
        {
            yield return new WaitForSeconds(7f);
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            // if (collision.gameObject.CompareTag("Pickup"))
            // {
            //     ModifyPlayerCharges(2);
            //     Destroy(collision.gameObject);
            // }
            // if (collision.gameObject.CompareTag("ChargeVoid"))
            // {
            //     ModifyPlayerCharges(-3);
            //     Destroy(collision.gameObject);
            // }
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}
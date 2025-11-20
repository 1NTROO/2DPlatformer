using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using System.ComponentModel;

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
        /// Coyote time duration in seconds.
        /// </summary>
        public float coyoteTime = 0.2f;
        private float coyoteTimeCounter;

        /// <summary>
        /// Jump buffer duration in seconds.
        /// </summary>
        public float jumpBufferTime = 0.2f;
        private float jumpBufferCounter;


        [Header("Wall Slide Parameters")]
        /// <summary>
        /// Wall slide speed limit
        /// </summary>
        public float wallSlideSpeed = 3f;
        private bool isWallSliding;

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
        [SerializeField] private float wallJumpDirection;

        /// <summary>
        /// Time allowed to wall jump after leaving the wall
        /// </summary>
        [SerializeField] private float wallJumpingTime = 0.2f;
        private float wallJumpingCounter;

        /// <summary>
        /// Duration of wall jump invincibility
        /// </summary>
        [SerializeField] private float wallJumpingDuration = 0.4f;

        /// <summary>
        /// Power of wall jump in x and y directions
        /// </summary>
        [SerializeField] private Vector2 wallJumpingPower = new Vector2(8f, 16f);

        [Header("Player States")]
        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
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
                move.x = Input.GetAxis("Horizontal");
                if (jumpState == JumpState.Grounded && jumpBufferCounter > 0f)
                    jumpState = JumpState.PrepareToJump;
                else if (Input.GetButtonUp("Jump"))
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;

                    coyoteTimeCounter = 0f;
                }

                WallSlide();
                WallJump();

                EnableMomentum();
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
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    jumpBufferCounter = 0f;
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
            if (isWallJumping)
            {
                Debug.Log($"ComputeVelocity: isWallJumping = {isWallJumping}, Velocity = {velocity}");
                targetVelocity.x = velocity.x;
                return;
            }

            if (jump && coyoteTimeCounter > 0f)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }


            if (move.x > 0.01f)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;               

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityY", velocity.y);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        private bool IsWalled()
        {
            foreach (var check in wallCheck)
            {
                if (Physics2D.OverlapCircle(check.position, 0.1f, wallLayer))
                {
                    return true;
                }
            }
            return false;
        }

        private void WallSlide()
        {
            if (IsWalled() && !IsGrounded && !isWallJumping && move.x != 0)
            {
                isWallSliding = true;
                velocity.y = Mathf.Clamp(velocity.y, -wallSlideSpeed, float.MaxValue);
            }
            else
            {
                isWallSliding = false;
            }
        }

        private void WallJump()
        {
            if (isWallSliding)
            {
                isWallJumping = false;
                if (wallJumpDirection == 0f)
                    wallJumpDirection = -Mathf.Sign(move.x);
                wallJumpingCounter = wallJumpingTime;

                CancelInvoke(nameof(StopWallJump));
            }
            else
            {
                wallJumpingCounter -= Time.deltaTime;
            }

            if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
            {
                isWallJumping = true;
                isWallSliding = false;
                velocity.x = wallJumpDirection * jumpTakeOffSpeed * model.jumpModifier;
                velocity.y = jumpTakeOffSpeed * model.jumpModifier * 0.5f;

                PreserveMomentum = true;


                wallJumpingCounter = 0f;

                if (wallJumpDirection == (spriteRenderer.flipX ? 1f : -1f))
                {
                    spriteRenderer.flipX = !spriteRenderer.flipX;
                }

                Debug.Log($"Wall Jump! Velocity: {velocity}, Direction: {wallJumpDirection}");

                wallJumpDirection = 0f;

                Invoke(nameof(StopWallJump), wallJumpingDuration);
            }

            if (wallJumpingCounter <= 0f || IsGrounded)
            {
                wallJumpDirection = 0f;
            }
        }

        private void StopWallJump()
        {
            isWallJumping = false;

            targetVelocity.x = velocity.x;
        }   

        private void EnableMomentum()
        {
            if (IsWalled() || IsGrounded)
                PreserveMomentum = false;
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
using Game.Common.Packets;
using LiteNetLib;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Assets.Scripts
{
    public class AnimationStateMachine : MonoBehaviour
    {
        [SerializeField] private ServerEntity _serverEntity;
        public SpriteRenderer SpriteRenderer;

        public IdleState IdleState { get; private set; }
        public MovingState MovingState { get; private set; }
        public AttackingState AttackingState { get; private set; }
        public DeadState DeadState { get; private set; }

        [SerializeField] private CustomAnimation _idleAnimation;
        [SerializeField] private CustomAnimation _movingAnimation;
        [SerializeField] private CustomAnimation _attackingAnimation;
        [SerializeField] private CustomAnimation _deathAnimation;

        public AnimationState CurrentState { get; private set; }

        private Vector3 _previousPosition;
        private Vector2 _previousAttackDir;
        public bool IsMoving;


        private void Awake()
        {
            NetworkManager.Instance.PacketDispatcher.Subscribe<EntityDiedPacket>(OnEntityDied);
            NetworkManager.Instance.PacketDispatcher.Subscribe<EntityAttackedPacket>(OnEntityAttacked);

            IdleState = new IdleState(this, _idleAnimation);
            MovingState = new MovingState(this, _movingAnimation);
            AttackingState = new AttackingState(this, _attackingAnimation);
            DeadState = new DeadState(this, _deathAnimation);

            _serverEntity = GetComponentInParent<ServerEntity>();

            CurrentState = IdleState;
        }
        private void OnEntityAttacked(NetPeer peer, EntityAttackedPacket packet)
        {
            if (packet.EntityID != _serverEntity.EntityID)
                return;
            Debug.Log("Switching to Attacking State");

            _previousAttackDir = new Vector2 (packet.AttackDirection.X, packet.AttackDirection.Y);

            SetState(AttackingState);
        }

        private void OnEntityDied(NetPeer peer, EntityDiedPacket packet)
        {
            if (packet.EntityID != _serverEntity.EntityID)
                return;
            Debug.Log("Switching to Dead State");
            SetState(DeadState);
        }

        public void SetState(AnimationState state)
        {
            CurrentState.Exit();
            CurrentState = state;
            CurrentState.Enter();
        }

        public void FixedUpdate()
        {
            IsMoving = (transform.position - _previousPosition).magnitude > 0.01f;

            if (CurrentState == AttackingState)
            {
                UpdateSpriteFlip(_previousAttackDir);
            }
            else
            {
                Vector3 movementDirection = transform.position - _previousPosition;
                UpdateSpriteFlip(movementDirection);
            }


            CurrentState.Execute();
           _previousPosition = transform.position;
        }

        public void Update()
        {

            CurrentState?.Update();
        }

        private void UpdateSpriteFlip(Vector2 dir)
        {
            SpriteRenderer.flipX = dir.x < 0;
        }

   
    }

    public class IdleState : AnimationState
    {
        public IdleState(AnimationStateMachine stateMachine, CustomAnimation customAnimation) : base(stateMachine, customAnimation)
        {
        }

        public override void Enter()
        {
            ResetAnimation();
        }

        public override void Execute()
        {
            if (_stateMachine.IsMoving) 
            {
                Debug.Log("Switching to Moving State");
                _stateMachine.SetState(_stateMachine.MovingState);
            }
        }

        public override void Exit()
        {
            
        }
    }

    public class MovingState : AnimationState
    {
        public MovingState(AnimationStateMachine stateMachine, CustomAnimation customAnimation) : base(stateMachine, customAnimation)
        {
        }

        public override void Enter()
        {
            ResetAnimation();
        }

        public override void Execute()
        {
            if (!_stateMachine.IsMoving)
            {
                Debug.Log("Switching to Idle State");
                _stateMachine.SetState(_stateMachine.IdleState);
            }
        }

        public override void Exit()
        {

        }
    }

    public class AttackingState : AnimationState
    {
        private float _startTime;
        public AttackingState(AnimationStateMachine stateMachine, CustomAnimation customAnimation) : base(stateMachine, customAnimation)
        {
            
        }

        public override void Enter()
        {
            ResetAnimation();
            _startTime = Time.time;
        }

        public override void Execute()
        {
            if(Time.time - _startTime >= CustomAnimation.AnimationDuration)
            {
                if (_stateMachine.IsMoving)
                {
                   
                    Debug.Log("Switching to Moving State");
                    _stateMachine.SetState(_stateMachine.MovingState);
                }
                else
                {
                    Debug.Log("Switching to Idle State");
                    _stateMachine.SetState(_stateMachine.IdleState);
                }
            }
        }

        public override void Exit()
        {
         
        }
    }

    public class DeadState : AnimationState
    {
        public DeadState(AnimationStateMachine stateMachine, CustomAnimation customAnimation) : base(stateMachine, customAnimation)
        {
        }

        public override void Enter()
        {
            ResetAnimation();
        }

        public override void Execute()
        {

        }

        public override void Exit()
        {

        }
    }

    public abstract class AnimationState
    {
        protected readonly AnimationStateMachine _stateMachine;
        public readonly CustomAnimation CustomAnimation;
        public float _frameTimer;
        public int _frameIndex;
        protected bool _animationComplete;
        public AnimationState(AnimationStateMachine stateMachine, CustomAnimation customAnimation)
        {
            CustomAnimation = customAnimation;
            _stateMachine = stateMachine;
            CustomAnimation.Initialize();
        }
        public virtual void Update()
        {
            // Skip update if animation is complete and not set to loop
            if (_animationComplete && !CustomAnimation.ShouldLoop)
                return;

            // Cycle through animation frames
            _frameTimer += Time.deltaTime;
            float frameTime = 1f / CustomAnimation.FramesPerSecond;

            if (_frameTimer >= frameTime)
            {
                // Determine frame progression based on looping
                if (CustomAnimation.ShouldLoop)
                {
                    // Normal looping behavior
                    _frameIndex = (_frameIndex + 1) % CustomAnimation.AnimationFrames.Length;
                }
                else
                {
                    // Non-looping behavior
                    if (_frameIndex < CustomAnimation.AnimationFrames.Length - 1)
                    {
                        _frameIndex++;
                    }
                    else
                    {
                        _animationComplete = true;
                    }
                }

                _frameTimer -= frameTime;

                // Update sprite renderer with current frame
                _stateMachine.SpriteRenderer.sprite = CustomAnimation.AnimationFrames[_frameIndex];
            }
        }

        public virtual void ResetAnimation()
        {
            _frameIndex = 0;
            _frameTimer = 0f;
            _animationComplete = false;
        }

        public abstract void Enter();
        public abstract void Execute();
        public abstract void Exit();
    }

}

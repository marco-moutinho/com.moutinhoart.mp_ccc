using MP_CCC.Enums;
using UnityEngine;

// Created on 27-Feb-2026 | Last change on [ 01 - May - 2026 ]
namespace MP_CCC
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMovementComponent : MonoBehaviour
    {
        private CharacterController _characterController;
        private float _gravityForce = -9.81f;
        private Vector3 _MovementDirection;
        private Vector3 _gravityVector;
        private Vector3 _velocityVector;
        private Vector3 _FinalMovementVector;
        private bool _isGrounded;
        private Vector3 _GroundNormal;
        private float _currentDecelerationRate;
        private float _currentAccelerationRate;

        protected bool _isCrouched;

        //protected bool BUseAcceleration = false;

        private float _targetCurrentMovementSpeed;
        private float _currentMovementSpeed;

        private float _currentJumpHeight;
        private EMovementState _lastTickMovementState;
        private EMovementState _currentMovementState;

        public struct FCastParams
        {
            public Vector3 StartLocation;
            public Vector3 Direction;
            public float Range;
            public LayerMask layer;
            public QueryTriggerInteraction query;
            public RaycastHit hit;
        }
        protected FCastParams _ceilingCastParams;

        [SerializeField]
        private LayerMask _ceilingLayerMask;

        //rto,,,
        private float _jumpQuequeDuration;
        private float _jumpQuequeTimer;

        // debugers
        private Vector3 _JumpStartLocation;
        private Vector3 _JumpApexLocation;
        private Vector3 _LandedLocation;
        private float _OnAirTimeElapsed;

        [SerializeField] private bool _drawGizmos = true;

        [SerializeField]
        private bool _sendDebugMsg = false;
        private void OnValidate()
        {
#if UNITY_EDITOR
            _characterController = GetComponent<CharacterController>();
#endif
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _characterController = GetComponent<CharacterController>();
            _jumpQuequeTimer = 0.0f;

            //_currentMovementState = EMovementState.Grounded;
        }

        public virtual void Method_Execute()
        {
            _lastTickMovementState = _currentMovementState;

            //if (_currentMovementState == EMovementState.Jumping)
            //{
            //    Method_CheckCeiling();
            //}

            // calculate movement vector
            Method_CalculateMovementVector();

            // apply movement
            Method_Move();

            if (_jumpQuequeTimer >= 0.0f)
            {
                _jumpQuequeTimer -= Time.deltaTime;

                //Debug.Log("Queque = " + _jumpQuequeTimer);
            }
            if (_currentMovementState == EMovementState.Jumping)
            {
                Method_CheckCeiling();
            }
        }

        private bool _isWaitingToResolveJump;
        // added on 02-mar-2026
        protected virtual void Method_CalculateMovementVector()
        {
            switch (_currentMovementState)
            {
                case EMovementState.Grounded:
                    /// a minha teoria é
                    /// se o codigo no jump calcula a velocityVector.y e sabemos que é positiva, se o personagem n se move em Y então posso calcular que a linha que comentei abaixo para n correr esteja a fazer override da função...
                    /// chamada pelo input to jogador, logo a solução pode passar por criar uma var que pode ter 2 valores, significando um que o jogador deu input e esta a espera que seja resolvido, e o outro valor que o pedido...
                    /// foi resolvido
                    //_gravityVector = new Vector3(0, 0, 0);
                    if (_isWaitingToResolveJump == false)
                    {
                        _gravityVector = new Vector3(0, 0, 0);
                    }
                    else
                    {
                        _gravityVector = new Vector3(0, _gravityVector.y, 0);
                        //_isWaitingToResolveJump = false; // <- aqui funciona mas e que tal ficar resolvido onStartJump?
                    }
                    break;

                case EMovementState.Falling:
                    _gravityVector.y += _gravityForce * Time.deltaTime;
                    break;

                case EMovementState.Jumping:
                    _gravityVector.y += _gravityForce * Time.deltaTime;
                    break;
            }

            // calculate final movement vector
            _velocityVector = (_gravityVector + (_MovementDirection * _currentMovementSpeed));

            if (_velocityVector.y <= 0 && _isGrounded)
            {
                _FinalMovementVector = Vector3.ProjectOnPlane(_velocityVector, _GroundNormal);
            }
            else
            {
                _FinalMovementVector = _velocityVector;
            }
            //Debug.Log("Velocity.y = " + _velocityVector.y);
        }

        protected virtual void Method_Move()
        {
            Vector3 LcMotion = _FinalMovementVector * Time.deltaTime; ;
            _characterController.Move(LcMotion);
        }
        // added on 03-Mar-2026
        public virtual void Method_Jump()
        {
            if (_sendDebugMsg) { Debug.Log("Method_Jump()"); }

            if (_isGrounded)
            {
                _isWaitingToResolveJump = true;
                //_currentMovementState = EMovementState.Jumping;
                _JumpStartLocation = transform.position;
                _gravityVector.y = Mathf.Sqrt(_currentJumpHeight * -2 * _gravityForce);
            }

            //Debug.Log("Start Jump Queque!!!!!");    
            _jumpQuequeTimer = _jumpQuequeDuration;
        }

        // added on 06-Mar-2026
        protected virtual void Method_CheckCeiling()
        {
            _ceilingCastParams.StartLocation = transform.position + _characterController.center + (transform.up * ((_characterController.height * 0.5f) - _characterController.radius));
            _ceilingCastParams.Direction = transform.up;
            _ceilingCastParams.Range = _characterController.skinWidth;
            _ceilingCastParams.layer = _ceilingLayerMask;

            if (Physics.SphereCast(origin: _ceilingCastParams.StartLocation, radius: _characterController.radius, direction: _ceilingCastParams.Direction, hitInfo: out _ceilingCastParams.hit, maxDistance: _ceilingCastParams.Range, layerMask: _ceilingCastParams.layer, queryTriggerInteraction: _ceilingCastParams.query))
            {
                // to solve the hit ceiling while jump and imediatly down, cause jump "buffer" it jumps emidiatly on land, so this is a quick fix, but i want a more roboust solution
                // cause imagine that I hit a high ceiling, I may want to jump anyway, so yup, thos just cancel the "_isWaitingToResolveJump"
                //_isWaitingToResolveJump = false;

                _jumpQuequeTimer = 0;
                _gravityVector.y = 0.0f;
            }
        }

        // added on 03-Mar-2026
        public void Method_ReceiveMovementDirection(in Vector3 inVector3d)
        {
            _MovementDirection = inVector3d;
        }
        public void Method_SetIsGrounded(in bool InValue)
        {
            _isGrounded = InValue;
        }

        // added on 03-Mar-2026
        public void Method_SetCurrentMovementSpeed(in float inValue)
        {
            //_targetCurrentMovementSpeed = inValue;
            _currentMovementSpeed = inValue;
        }

        // added on 03-Mar-2026
        public void Method_SetJumpHeight(in float inValue)
        {
            _currentJumpHeight = inValue;
        }

        // added on 06-Mar-2026
        public void Method_SetJumpQuequeDuration(in float inValue)
        {
            _jumpQuequeDuration = inValue;
        }

        // added on 10-Mar-2026
        public void Method_ReceiveGroundNormal(Vector3 InValue)
        {
            _GroundNormal = InValue;
        }

        // added on 15-Mar-2026
        public void Method_ReceiveGravityForce(in float InValue)
        {
            _gravityForce = -InValue;
        }

        // added on 15-Mar-2026
        public void Method_SetCurrentAccelerationRate(in float inValue)
        { _currentAccelerationRate = inValue; }

        // added on 15-Mar-2026
        public void Method_SetCurrentDecelerationRate(in float inValue)
        { _currentDecelerationRate = inValue; }

        public virtual void Method_HandleMovementState()
        {
            // "State Machine" - define state based on "physics" state
            if (_isGrounded && _gravityVector.y <= 0f) { _currentMovementState = EMovementState.Grounded; }
            if (_isGrounded && _gravityVector.y > 0f) { _currentMovementState = EMovementState.Jumping; }
            if (_isGrounded == false && _gravityVector.y <= 0f) { _currentMovementState = EMovementState.Falling; }
            if (_isGrounded == false && _gravityVector.y > 0f) { _currentMovementState = EMovementState.Jumping; }

            // debug vars...
            if (_isGrounded == false)
            {
                _OnAirTimeElapsed += Time.deltaTime;
            }
            else
            {
                _OnAirTimeElapsed = 0;
            }


            // Handle onEnter moveStates:
            //...
            if (_lastTickMovementState != _currentMovementState)
            {
                switch (_currentMovementState)
                {
                    // Landing condition
                    case EMovementState.Grounded:
                        if (_lastTickMovementState == EMovementState.Falling)
                        {
                            Method_OnLanded();
                        }
                        break;

                    case EMovementState.Jumping:
                        if (_lastTickMovementState == EMovementState.Grounded)
                        {
                            Method_OnStartJumping();
                        }
                        break;

                    case EMovementState.Falling:
                        if (_lastTickMovementState == EMovementState.Jumping)
                        {
                            Method_OnStartFalling();
                        }
                        break;
                }
            }
            //if(_lastTickMovementState == _currentMovementState && _currentMovementState == EMovementState.Jumping) { Debug.Log(" AHAH!"); }
        }


        // created on 04-Mar-2026
        protected virtual void Method_OnStartFalling()
        {
            //Debug.Log("Start Falling");
            _JumpApexLocation = transform.position;
            //Debug.Log("Jump height : " + (_JumpApexLocation - _JumpStartLocation).magnitude);
        }

        // created on 04-Mar-2026
        protected virtual void Method_OnStartJumping()
        {
            //Debug.Log("Start Jump!");
            _isWaitingToResolveJump = false;

        }

        // created on 04-Mar-2026
        protected virtual void Method_OnLanded()
        {
            // Debugger
            //Debug.Log("Landed!!");
            _LandedLocation = transform.position;

            // input queque timer has terminated
            if (_jumpQuequeTimer > 0.0f)
            {
                // reset its value
                _jumpQuequeTimer = -1.0f;
                // call jump
                if (_sendDebugMsg) { Debug.Log(this + " : Jump Buffer !!"); }
                Method_Jump();

            }
        }

        [Header("[ GIZMOS ]")]
        public Color colorForward;
        public Color colorMoveDir;
        public Color colorCeilingTraceColor;
        public Color colorCeilingHitColor;

        private void OnDrawGizmos()
        {
            if (_drawGizmos)
            {
                Gizmos.color = colorMoveDir;
                Vector3 LcLine_A_Start = transform.position;

                // Movement Direction
                Vector3 LcLine_A_End = LcLine_A_Start + (_MovementDirection * 2);
                Gizmos.DrawLine(LcLine_A_Start, LcLine_A_End);

                // Final Movement Direction
                Gizmos.DrawLine(LcLine_A_Start, LcLine_A_Start + _FinalMovementVector.normalized * 2);

                Gizmos.color = colorForward;
                Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2);

                Gizmos.color = Color.ghostWhite;
                Gizmos.DrawWireSphere(_JumpStartLocation, 0.3f);
                Vector3 lcGhostVector = new Vector3(_JumpStartLocation.x, _JumpApexLocation.y, _JumpStartLocation.z);
                Gizmos.DrawWireSphere(lcGhostVector, 0.3f);
                Gizmos.DrawLine(_JumpStartLocation, lcGhostVector);
                Gizmos.color = Color.orange;
                Gizmos.DrawWireSphere(_JumpApexLocation, 0.3f);
                Gizmos.DrawLine(_JumpStartLocation, _JumpApexLocation);

                Gizmos.color = colorCeilingTraceColor;

                Gizmos.DrawWireSphere(_ceilingCastParams.StartLocation, _characterController.radius);

                Gizmos.DrawWireSphere(_ceilingCastParams.StartLocation + (transform.up * _characterController.skinWidth), _characterController.radius);

                Gizmos.color = colorCeilingHitColor;
                Gizmos.DrawWireSphere(_ceilingCastParams.hit.point, 0.3f);
                Gizmos.DrawLine(_ceilingCastParams.StartLocation, _ceilingCastParams.hit.point);
            }
        }
    }
}
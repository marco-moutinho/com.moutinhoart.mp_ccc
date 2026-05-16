using UnityEngine;
using MP_CCC.Data;
// Created on 27-Feb-2026
// [ 16 - May - 2026 ] #Changed

/// TO DO LIST
/// [ ] - Change ground check from Raycast to SphereOverlap ?
/// [ ] - Get ground normal on this or movement script instead of CharacterGroundSensor ?
namespace MP_CCC
{
    [RequireComponent(typeof(CharacterMovementComponent))]
    [RequireComponent(typeof(CharacterGroundSensor))]
    public class CharacterControllerComponent : MonoBehaviour
    {
        [SerializeField] private CharacterControllerComponentData _characterControllerData;

        //[Header("[ Tuning ]")]
        //[SerializeField] protected float _jumpQuequeTime = 0.15f;

        //[Header("Camera")]

        [SerializeField] float _verticalOffset;
        
        private float _lookSensitivity;
        //[SerializeField] protected float _LookPitchMin;
        //[SerializeField] protected float _LookPitchMax;
        //[SerializeField][Min(0.0f)] protected float _rotationSmoothFactor;

        private float _walkSpeed;
        private float _sprintSpeed;
        private float _jumpHeight;
        //[SerializeField, Min(0f)] protected float _jumpDuration; // to implement in the fufure
        private float walkAcceleration;
        private float walkDeceleration;
        private float sprintAcceleration;
        private float sprintDeceleration;
        

        [Header("Gravity")]
        [SerializeField] float _gravityForce = 9.81f;

        // Components
        //...
        protected CharacterMovementComponent _CharacterMovementComp;
        protected CharacterGroundSensor _CharacterGroundSensor;
        protected CharacterController _characterController;
        // Transforms
        [SerializeField] protected Transform _YawPivot;
        [SerializeField] protected Transform _PitchPivot;
        [SerializeField] protected Transform _CameraTransform;

        // rto vars
        private Vector3 _standingPovLocalPosition;
        private Vector3 _crouchedPovLocalPosition;
        
        private Vector3 _standCenterLocalPosition;
        private Vector3 _crouchCenterLocalPosition;

        private Vector3 _targetPovLocalPostion;
        private Vector3 _smoothPovTime;

        protected bool _isCrouched = false;
        protected bool _isCrouching = false;

        private RaycastHit _uncrouchHitInfo;

        //...
        // inputs...
        protected Vector2 _ControllerInputMoveVector;
        protected Vector2 _ControllerInputLookVector;
        protected float _Pitch;
        protected float _YawDelta;
        protected Quaternion _targetYawQuaternion;
        protected Quaternion _targetPitchQuaternion;

        protected enum Enum_MovementState { ms_Grounded, ms_Falling, ms_Jumping, ms_Flying }
        protected Enum_MovementState _CurrentMovementState;

        [SerializeField] private bool _drawGizmos = true;

        private void OnValidate()
        {
            _YawPivot.position = transform.position + (transform.up * _verticalOffset);
        }

        private void Awake()
        {
            Method_InitializeMovementValues();
        }
        protected virtual void Start()
        {
            _CharacterMovementComp = GetComponent<CharacterMovementComponent>();
            _characterController = GetComponent<CharacterController>();
            _CharacterGroundSensor = GetComponent<CharacterGroundSensor>();
            
            _CharacterGroundSensor.Method_SetSphereCastRadius(_characterController.radius);

            _YawPivot.position = transform.position +( transform.up * _verticalOffset) ;

            if(_CharacterMovementComp == null) { Debug.LogError(this + " : [ MARCO ] : _CharacterMovementComp is null !!!"); return; }
            _CharacterMovementComp.Method_SetCurrentMovementSpeed(_walkSpeed);
            _CharacterMovementComp.Method_SetCurrentAccelerationRate(walkAcceleration);
            _CharacterMovementComp.Method_SetCurrentDecelerationRate(walkDeceleration);
            _CharacterMovementComp.Method_SetJumpHeight(_jumpHeight);

            _CharacterMovementComp.Method_SetJumpQuequeDuration(_characterControllerData.jumpInputBuffer);

            // STORE / CACHE
            _targetPovLocalPostion = _YawPivot.localPosition;
            Method_StoreCrouchValues();

            Debug.Log(this + " / " + gameObject + " : START()");
        }

        protected virtual void Update()
        {
            Method_PovSmoothTransition();

            // Look
            Method_HandleLook();

            // CharacterMovement
            if(_isCrouching == false) { Method_UnCrouch(); }
            Method_HandleGround();
            Method_HandleMove();

            //CharacterRotation
            //Method_HandleCharacterYaw();

            _CharacterMovementComp.Method_HandleMovementState();
        }

        private Vector3 _trueStart; // added on 07-Mar-2026
        // added on 02-Mar-2026
        protected virtual void Method_HandleGround()
        {
            // calculate ground sensor cast/trace start point
            float lcVerticalOffset = (_characterController.height * 0.5f) - _characterController.radius - _characterController.skinWidth;
            Vector3 lcStartPoint = transform.position + _characterController.center - new Vector3(0f, lcVerticalOffset, 0f);
            _trueStart = lcStartPoint;

            // Set it
            _CharacterGroundSensor.Method_SetTraceStartPointOffset(inValue: lcStartPoint);
            
            // Execute ground sensor
            _CharacterGroundSensor.Method_Execute();
            bool lcIsGrounded;
            _CharacterGroundSensor.Method_ReturnIsGrounded(outValue: out  lcIsGrounded);

            // update CharacterMovementComponent
            _CharacterMovementComp.Method_SetIsGrounded(lcIsGrounded);
            _CharacterMovementComp.Method_ReceiveGroundNormal(_CharacterGroundSensor.Method_GetGroundNormal());

        }

        // this was meant ( at the time of creation ) only for first person type of gameplay and setup...
        protected virtual void Method_HandleLook()
        {
            // horizontal look
            _YawDelta += _ControllerInputLookVector.x * _lookSensitivity;

            // added on 02.May.2026
            // I added this block so without this the var _YawDelta just acumulates forever and eventualy will hit 'float point precison errors', so the more the player looks over time the closest it is from hit computer limits
            if (_YawDelta > 360f || _YawDelta < -360f)
            {
                _YawDelta = Mathf.Repeat(_YawDelta, 360f);
            }

            //transform.Rotate(Vector3.up, _YawDelta);

            // Vertical Look
            _Pitch -= _ControllerInputLookVector.y * _lookSensitivity;
            //_Pitch = Mathf.Clamp(_Pitch, _LookPitchMin, _LookPitchMax); // OLD - prefab based
            _Pitch = Mathf.Clamp(_Pitch, _characterControllerData.stLookSettings.minPitch, _characterControllerData.stLookSettings.maxPitch); // NEW - Data based

            // calculate quaternions
            _targetYawQuaternion = Quaternion.Euler(0f ,_YawDelta, 0f);
            _targetPitchQuaternion = Quaternion.Euler(_Pitch, 0f, 0f);

            // smooth - OLD - prefab based
            //transform.rotation = Quaternion.Slerp(transform.rotation, _targetYawQuaternion, _rotationSmoothFactor * Time.deltaTime);
            //_PitchPivot.localRotation = Quaternion.Slerp(_PitchPivot.localRotation, _targetPitchQuaternion, _rotationSmoothFactor * Time.deltaTime);
            
            // NEW - Data based
            float lcRTF = _characterControllerData.stLookSettings.rotationSmoothFactor;
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetYawQuaternion, lcRTF * Time.deltaTime);
            _PitchPivot.localRotation = Quaternion.Slerp(_PitchPivot.localRotation, _targetPitchQuaternion, lcRTF * Time.deltaTime);
        }
        protected virtual void Method_HandleMove()
        {
            // Step 1 - Calculate movement direction
            Vector3 lcMovementDirection = (transform.forward * _ControllerInputMoveVector.y) + (transform.right * _ControllerInputMoveVector.x);

            // Step 2 -Send movement direction
            _CharacterMovementComp.Method_ReceiveMovementDirection(lcMovementDirection);
            _CharacterMovementComp.Method_ReceiveGravityForce(InValue: _gravityForce);

            // Step 3 - Execute movement logic on the component
            _CharacterMovementComp.Method_Execute();
        }

        protected virtual void Method_HandleCharacterYaw()
        {
            transform.Rotate(Vector3.up, _YawDelta);
        }

        public virtual void Method_ReceiveMoveInput(in Vector2 inValue)
        {
            //inValue.Normalize();
            _ControllerInputMoveVector = inValue;
        }
        public virtual void Method_ReceiveLookInput(in Vector2 inDirection, in float inSensitivity)
        {
            //inValue.Normalize();
            _ControllerInputLookVector = inDirection;
            _lookSensitivity = inSensitivity;
        }

        public virtual void Method_ReceiveJumpInput()
        {
            _CharacterMovementComp.Method_Jump();
        }
        
        // added on 15-Mar-2026
        public virtual void Method_ReceiveStartToSprintInput()
        {
            _CharacterMovementComp.Method_SetCurrentMovementSpeed(_sprintSpeed);
            _CharacterMovementComp.Method_SetCurrentAccelerationRate(sprintAcceleration);
        }
        
        // added on 15-Mar-2026
        public virtual void Method_ReceiveStopSprintInput()
        {
            _CharacterMovementComp.Method_SetCurrentMovementSpeed(_walkSpeed);
            _CharacterMovementComp.Method_SetCurrentDecelerationRate(sprintDeceleration);
        }
         // added on 02-Apr-2026
         public virtual void Method_InitializeMovementValues()
        {
            // safety check
            if(_characterControllerData == null)
            {
                Debug.LogError("[ MARCO ] : " + this + "_characterControllerData is null !!!");
                return;
            }

            // initilize movement ( run time ) values based on scriptable object asset values
            _walkSpeed = _characterControllerData.walkSpeed;
            _sprintSpeed = _characterControllerData.sprintSpeed;
            _jumpHeight = _characterControllerData.jumpHeight;
        }

        [SerializeField]protected float _topOffset;
        private void Method_StoreCrouchValues()
        {
            float currentVerticalBottom = _characterController.center.y - (_characterController.height * 0.5f);

            float newHeight = _characterControllerData.stCaracterControllerCapsuleSettings.crouchHeight;

            Vector3 newCenter = _characterController.center;
            newCenter.y = currentVerticalBottom + (newHeight * 0.5f); // *0.5f cause Unity CharacterController creates a capsule from the center, so half of height
            // store it
            _crouchCenterLocalPosition = newCenter;

            // pov...
            //store/cache normal/standing local position
            _standingPovLocalPosition = _YawPivot.localPosition;
            _standCenterLocalPosition = _characterController.center;

            // compute crouch POV local position
            Vector3 standPovLocalPos = _YawPivot.localPosition;
            float verticalDelta = _characterController.height - newHeight; // top displacement
            Vector3 crouchPovLocalPos = standPovLocalPos + (Vector3.down * verticalDelta);
            //store/cache it
            _crouchedPovLocalPosition = crouchPovLocalPos;
        }

        // [ 08 - May - 2026 ] #Added
        // [ 09 - May - 2026 ] #Changed
        public virtual void Method_Crouch()
        {
            _isCrouched = true;
            _isCrouching = true;
            // Apply changes to CharacterCapsule
            _characterController.center = _crouchCenterLocalPosition;
            _characterController.height = _characterControllerData.stCaracterControllerCapsuleSettings.crouchHeight;

            _targetPovLocalPostion = _crouchedPovLocalPosition;
        }

        // [ 09 - May - 2026 ] #Added
        public virtual void Method_UnCrouch()
        {
            _isCrouching = false;

            if (Method_CheckIfCanUnCrouch()) { }
            else { return; }

            _isCrouched = false;
            
            _characterController.center = _standCenterLocalPosition;
            _characterController.height = _characterControllerData.stCaracterControllerCapsuleSettings.standHeight;

            _targetPovLocalPostion = _standingPovLocalPosition;
        }

        private Vector3 capsuleBot;
        private Vector3 capsuleTop;

        // [ 09 - May - 2026 ] #Added
        protected virtual bool Method_CheckIfCanUnCrouch()
        {
            float capsuleRadius = _characterController.radius;
            float standingHeight = _characterControllerData.stCaracterControllerCapsuleSettings.standHeight;
            Vector3 standingCenter = _standCenterLocalPosition;

            // local to world
            Vector3 worldCenter = transform.TransformPoint(standingCenter);

            // compute capsule endpoints
            //...
            float halfHeight = (standingHeight * 0.5f) - capsuleRadius;
            
            //Bottom sphere center
            Vector3 bottomCenter = worldCenter - (transform.up * halfHeight);

            // Top sphere center
            Vector3 topCenter = worldCenter + (transform.up * halfHeight);

            capsuleBot = bottomCenter;
            capsuleTop = topCenter;

            // perform physics check
            bool blocked = Physics.CheckCapsule(bottomCenter, topCenter, capsuleRadius, _characterControllerData.stCaracterControllerCapsuleSettings.physicsChecksLayerMask, QueryTriggerInteraction.Ignore);
            
            //blocked = Physics.SphereCast(bottomCenter, capsuleRadius, transform.up, out _uncrouchHitInfo, (bottomCenter - topCenter).magnitude, _characterControllerData.stCaracterControllerCapsuleSettings.physicsChecksLayerMask, QueryTriggerInteraction.Ignore);
            //Debug.Log("uncrouchHitInfo : " + _uncrouchHitInfo.collider);

            if(blocked) { return false; }
            else { return true; }
        }

        // [ 09 - May - 2026 ] #Added
        protected void Method_PovSmoothTransition()
        {
            _YawPivot.localPosition = Vector3.SmoothDamp(_YawPivot.localPosition, _targetPovLocalPostion, ref _smoothPovTime, _characterControllerData.povSmoothTime);
        }

        // GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS | GIZMOS |
        private void OnDrawGizmos()
        {
            if (_drawGizmos)
            {
                Gizmos.color = Color.darkGreen;
                Gizmos.DrawLine(_YawPivot.position, _YawPivot.position + _YawPivot.forward * 1);

                Gizmos.color = Color.darkRed;
                Gizmos.DrawLine(_PitchPivot.position, _PitchPivot.position + _PitchPivot.forward * 1);

                Gizmos.color = Color.yellowNice;
                Gizmos.DrawLine(_CameraTransform.position, _CameraTransform.position + _CameraTransform.forward * 0.5f);
                
                if(Application.isPlaying == false) { return; }
                Gizmos.color = Color.white;
                Gizmos.DrawWireSphere(capsuleBot, _characterController.radius);
                Gizmos.DrawWireSphere(capsuleTop, _characterController.radius);

                // debug if can UnCroucj
                if (_isCrouching == false && _isCrouched == true)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(capsuleTop, _characterController.radius);
                }
            }
        }
    }
    // end class
}
//end namespace

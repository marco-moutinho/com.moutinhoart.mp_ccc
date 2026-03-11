using UnityEngine;
    
// Created on 27-Feb-2026
// last change on 02-Mar-2026

namespace MP_CCC
{
    [RequireComponent(typeof(CharacterMovementComponent))]
    [RequireComponent(typeof(CharacterGroundSensor))]
    public class CharacterControllerComponent : MonoBehaviour
    {
        [Header("[ Tuning ]")]
        [SerializeField] protected float _jumpQuequeTime = 0.15f;

        [Header("Camera")]

        [SerializeField] float _verticalOffset;
        [SerializeField] [Min(0.0f)] protected float _rotationSmoothFactor;
        [SerializeField] protected float _lookSensitivity;
        [SerializeField] protected float _LookPitchMin;
        [SerializeField] protected float _LookPitchMax;

        [Header("Movement")]
        [SerializeField, Min(0f)] protected float _walkSpeed = 3f;
        [SerializeField, Min(0f)] protected float _sprintSpeed = 6f;
        [SerializeField, Min(0f)] protected float _jumpHeight = 2f;
        //[SerializeField, Min(0f)] protected float _jumpDuration; // to implement in the fufure


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
        private void OnValidate()
        {
            _YawPivot.position = transform.position + (transform.up * _verticalOffset);
        }
        protected virtual void Start()
        {
            _CharacterMovementComp = GetComponent<CharacterMovementComponent>();
            _characterController = GetComponent<CharacterController>();
            _CharacterGroundSensor = GetComponent<CharacterGroundSensor>();
            
            _CharacterGroundSensor.Method_SetSphereCastRadius(_characterController.radius);

            _YawPivot.position = transform.position +( transform.up * _verticalOffset) ;

            _CharacterMovementComp.Method_SetCurrentMovementSpeed(_walkSpeed);
            _CharacterMovementComp.Method_SetJumpHeight(_jumpHeight);

            _CharacterMovementComp.Method_SetJumpQuequeDuration(_jumpQuequeTime);
        }

        protected virtual void Update()
        {
            // Look
            Method_HandleLook();

            // CharacterMovement
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
            _CharacterGroundSensor.Method_IsGrounded(outValue: out  lcIsGrounded);

            // update CharacterMovementComponent
            _CharacterMovementComp.Method_SetIsGrounded(lcIsGrounded);
            _CharacterMovementComp.Method_ReceiveGroundNormal(_CharacterGroundSensor.Method_GetGroundNormal());

        }
        protected virtual void Method_HandleLook()
        {
            // horizontal look
            _YawDelta += _ControllerInputLookVector.x * _lookSensitivity;
            //transform.Rotate(Vector3.up, _YawDelta);

            // Vertical Look
            _Pitch -= _ControllerInputLookVector.y * _lookSensitivity;
            _Pitch = Mathf.Clamp(_Pitch, _LookPitchMin, _LookPitchMax);
            // _PitchPivot.localEulerAngles = new Vector3(_Pitch, 0f, 0f);

            // calculate quaternions
            _targetYawQuaternion = Quaternion.Euler(0f ,_YawDelta, 0f);
            _targetPitchQuaternion = Quaternion.Euler(_Pitch, 0f, 0f);

            // smooth
            transform.rotation = Quaternion.Slerp(transform.rotation, _targetYawQuaternion, _rotationSmoothFactor * Time.deltaTime);
            _PitchPivot.localRotation = Quaternion.Slerp(_PitchPivot.localRotation, _targetPitchQuaternion, _rotationSmoothFactor * Time.deltaTime);
        }
        protected virtual void Method_HandleMove()
        {
            // Step 1 - Calculate movement direction
            Vector3 lcMovementDirection = (transform.forward * _ControllerInputMoveVector.y) + (transform.right * _ControllerInputMoveVector.x);

            // Step 2 -Send movement direction
            _CharacterMovementComp.Method_ReceiveMovementDirection(lcMovementDirection);

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
        public virtual void Method_ReceiveLookInput(in Vector2 inValue)
        {
            //inValue.Normalize();
            _ControllerInputLookVector = inValue;
        }

        public virtual void Method_ReceiveJumpInput()
        {
            _CharacterMovementComp.Method_Jump();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.darkGreen;
            Gizmos.DrawLine(_YawPivot.position, _YawPivot.position + _YawPivot.forward * 1);

            Gizmos.color = Color.darkRed;
            Gizmos.DrawLine(_PitchPivot.position, _PitchPivot.position + _PitchPivot.forward * 1);

            Gizmos.color = Color.yellowNice;
            Gizmos.DrawLine(_CameraTransform.position, _CameraTransform.position + _CameraTransform.forward * 0.5f);
        }
    }
    // end class
}
//end namespace

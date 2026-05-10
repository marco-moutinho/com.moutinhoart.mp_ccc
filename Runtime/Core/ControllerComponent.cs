using UnityEngine;
using UnityEngine.InputSystem;
using MP_CCC.Data;

// created on 28-Feb-2026
// [ 09 - May - 2026 ] #Changed
namespace MP_CCC
{
    public class ControllerComponent : MonoBehaviour
    {
        protected IA_Controls _controls;
        public CharacterControllerComponent _CharControlller;

        [SerializeField] protected PlayerControllerInputData _inputData;

        [SerializeField] private float _mouseSensitivity = 1;
        [SerializeField] private float _gamepadRightStickSensitivity = 1;
        [SerializeField]
        private bool _showDebugMsg = false;

        private bool _isUsingGamepad;

        protected virtual void Awake()
        {
            _controls = new IA_Controls();

            //_controls.CharacterMap.IA_Move.performed += ctx => _CharControlller.Method_ReceiveMoveInput(ctx.ReadValue<Vector2>());
            //_controls.CharacterMap.IA_Move.canceled += ctx => _CharControlller.Method_ReceiveMoveInput(Vector2.zero);

            // new method via ScriptableObjects
            _inputData.move.action.performed += ctx => _CharControlller.Method_ReceiveMoveInput(ctx.ReadValue<Vector2>());
            _inputData.move.action.canceled += ctx => _CharControlller.Method_ReceiveMoveInput(Vector2.zero);

            _controls.CharacterMap.IA_Look.started += ctx => Method_OnLook(ctx);
            _controls.CharacterMap.IA_Look.performed += ctx => Method_OnLook(ctx);
            _controls.CharacterMap.IA_Look.canceled += ctx => Method_OnLook(ctx);

            _controls.CharacterMap.IA_Jump.performed += ctx => _CharControlller.Method_ReceiveJumpInput();

            _controls.CharacterMap.IA_Sprint.performed += ctx => _CharControlller.Method_ReceiveStartToSprintInput();
            _controls.CharacterMap.IA_Sprint.canceled += ctx => _CharControlller.Method_ReceiveStopSprintInput();

            _inputData.crouch.action.performed += ctx => _CharControlller.Method_Crouch();
            _inputData.crouch.action.canceled += ctx => _CharControlller.Method_UnCrouch();

        }
        protected virtual void OnEnable()
        {
            _controls.Enable();
        }

        protected virtual void OnDisable()
        {
            _controls.Disable();
        }

        protected virtual void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // created on 02-Apr-2026
        protected virtual void Method_OnLook(InputAction.CallbackContext inCtx)
        {
            if (inCtx.started)
            {
                // detect device
                Method_CheckInputDeviceUsed(inCtx.control.device);
            }
            if (inCtx.canceled)
            {
                _CharControlller.Method_ReceiveLookInput(Vector2.zero, 0.0f);
            }

            // read input value
            Vector2 inputValue = inCtx.ReadValue<Vector2>();

            // sen
            float lcSensitivityValue;
            if (_isUsingGamepad)
            {
                lcSensitivityValue = _gamepadRightStickSensitivity;
            }
            else
            {
                lcSensitivityValue = _mouseSensitivity;
            }


            // send it to character
            _CharControlller.Method_ReceiveLookInput(inDirection: inputValue, inSensitivity: lcSensitivityValue);
        }

        // created on 02-Apr-2026
        private void Method_CheckInputDeviceUsed(InputDevice inDevice)
        {
            if (inDevice is Gamepad)
            {
                _isUsingGamepad = true;
                if (_showDebugMsg) { Debug.Log("Is Using GAMEPAD"); }
            }

            if (inDevice is Mouse)
            {
                _isUsingGamepad = false;
                if (_showDebugMsg) { Debug.Log("Is Using MOUSE"); }
            }
        }
    }
}
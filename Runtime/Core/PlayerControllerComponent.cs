using UnityEngine;
using MP_CoreUtilities.Interfaces;
namespace MP_CCC
{
    // created : 27-Feb-2026
    // last change : 28-Feb-2026
    public class PlayerControllerComponent : MonoBehaviour
    {
        protected IA_Controls _controls;
        protected GameObject _ControlGameObject;
        protected IControlable _iControlable;

        private void Awake()
        {
            _controls = new IA_Controls();
        }
        private void OnEnable()
        {
            _controls.Enable();
        }

        private void OnDisable()
        {
            _controls.Disable();
        }

        private void Start()
        {
            if (_ControlGameObject != null)
            {
                _iControlable = _ControlGameObject.GetComponent<IControlable>();

                if(_iControlable != null)
                {
                    Debug.LogError(_ControlGameObject + " Does not has IControlable implemented ");
                }
            }
        }

        // created on 28-Feb-2026
        protected void Method_HandleVector2DInput(in Vector2 inValue, out Vector2 outValue)
        {
            outValue = inValue.normalized;
        }

        // created on 28-Feb-2026
        protected void Method_HandleMovementInput(in Vector2 inVector)
        {
            Vector2 LcVector;
            Method_HandleVector2DInput(inVector, out LcVector);
            _iControlable.IMethod_HandleMoveInput(LcVector);
        }

        // created on 28-Feb-2026
        protected void Method_HandleLookInput(in Vector2 inVector)
        {
            Vector2 LcVector2D;
            Method_HandleVector2DInput(inVector, out LcVector2D);
            _iControlable.IMethod_HandleLookInput(LcVector2D);
        }
    }


}
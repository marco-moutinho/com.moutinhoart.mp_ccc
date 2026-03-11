using MP_CCC;
using UnityEngine;
// created on 28-Feb-2026
public class ControllerComponent : MonoBehaviour
{
    protected IA_Controls _controls;
    public CharacterControllerComponent _CharControlller;

    protected virtual void Awake()
    {
        _controls = new IA_Controls();

        _controls.CharacterMap.IA_Move.performed += ctx => _CharControlller.Method_ReceiveMoveInput(ctx.ReadValue<Vector2>());
        _controls.CharacterMap.IA_Move.canceled += ctx => _CharControlller.Method_ReceiveMoveInput(Vector2.zero);

        _controls.CharacterMap.IA_Look.performed += ctx => _CharControlller.Method_ReceiveLookInput(ctx.ReadValue<Vector2>());
        _controls.CharacterMap.IA_Look.canceled += ctx => _CharControlller.Method_ReceiveLookInput(Vector2.zero);

        _controls.CharacterMap.IA_Jump.performed += ctx => _CharControlller.Method_ReceiveJumpInput();

    }
    protected virtual void OnEnable()
    {
        _controls.Enable();
    }

    protected virtual void OnDisable()
    {
        _controls .Disable();
    }

    protected virtual void Start()
    {
        Cursor.lockState =CursorLockMode.Locked;
        Cursor.visible = false;
    }
}

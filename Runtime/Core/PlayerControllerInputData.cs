using UnityEngine;
using UnityEngine.InputSystem;
namespace MP_CCC.Data
{
    [CreateAssetMenu(fileName = "PlayerControllerInputData", menuName = "[ MP_CCC ]/PlayerControllerInputData")]
    public class PlayerControllerInputData : ScriptableObject
    {
        [Header("Movement")]
        public InputActionReference move;
        public InputActionReference look;
        public InputActionReference jump;
        public InputActionReference sprint;
        public InputActionReference crouch;

        [Header("Actions")]
        public InputActionReference interact;
        public InputActionReference primaryAction;
        public InputActionReference secondaryAction;
    }
}
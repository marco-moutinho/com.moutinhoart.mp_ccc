using UnityEngine;

// created on 02-Apr-2026
namespace MP_CCC.Data
{
    // [ 08 - May - 2026 ] #Added
    [System.Serializable]
    public struct StCharacterCapsuleSettings
    {
        public float standHeight;
        public float radius;
        public float crouchHeight;

        public LayerMask physicsChecksLayerMask;
    }

    // [ 09 - May - 2026 ] #Added
    [System.Serializable]
    public struct StLookSettings
    {
        [Range(0, 179)] public float maxPitch; // 80
        [Range(0, -179)] public float minPitch; // -90
        [Min(0.0f)] public float rotationSmoothFactor; // 23
    }

    [CreateAssetMenu(fileName = "CharacterControllerComponentData", menuName = "[ MP_CCC ]/Character Movement Data")]
    public class CharacterControllerComponentData : ScriptableObject
    {

        public float walkSpeed = 5;
        public float sprintSpeed = 10;

        public float crouchSpeed = 2.5f;
        public float crouchSprintSpeed = 3;

        public float jumpHeight = 2;
        public float jumpInputBuffer = 0.15f; //0.19f

        public float airControl { get; private set; } // TO DO // ALSO: why dont show on inspector?

        public StCharacterCapsuleSettings stCaracterControllerCapsuleSettings; // how to set base/default values
        public StLookSettings stLookSettings;

        public bool holdCrouch = false; // to handle how character handles crouch -Option A: crouched while hold ctrl | Option B : press ctrl/crouch to crouch and uncrouch
                                        // if option B maybe implement a second way of uncrouch, maybe on sprint or on jump idk...

        public float povSmoothTime;
    }
}
using UnityEngine;
namespace MP_CCC
{
    [RequireComponent(typeof(Transform))]
    // Created on 30-Mar-2026
    public class SpringArm : MonoBehaviour
    {
        [Header("[ Parameters ]")]

        [SerializeField]
        [Min(0)]
        protected float _lenght;

        [SerializeField]
        [Min(0)]
        protected float _CollisionSphereRadius;

        [SerializeField]
        [Min(0)]
        protected float _CollisionTraceRadius;

        [SerializeField] Transform _CameraTransform;
        protected float _CameraPitch;
        protected float _CameraYaw;

        //rto...
        protected Vector3 _StartPoint;
        protected Vector3 _EndPoint;

        public void Method_ExecuteFromController(in float inPitch, in float inYawDelta, in Vector3 inStartPoint)
        {
            _StartPoint = transform.position + inStartPoint;
            _EndPoint = _StartPoint - transform.forward * _lenght;

            _CameraPitch = inPitch;
            _CameraYaw += inYawDelta;

            // anteriormente no LateUpdate:
            //...

            // Camera rotation
            Quaternion lcCameraQuaternion = Quaternion.Euler(_CameraPitch, _CameraYaw, 0);

            // apply spring arm offset to the camera
            //_CameraTransform.SetPositionAndRotation (_EndPoint, lcCameraQuaternion);

            Vector3 targetPos = _EndPoint;
            Quaternion targetRot = lcCameraQuaternion;

            _CameraTransform.position = Vector3.Lerp(
                _CameraTransform.position,
                targetPos,
                1f - Mathf.Exp(-40f * Time.deltaTime)
            );

            _CameraTransform.rotation = Quaternion.Slerp(
                _CameraTransform.rotation,
                targetRot,
                1f - Mathf.Exp(-40f * Time.deltaTime)
            );

            /// TO DO
            /// rotate springarm
        }
        public virtual void Method_UpdateComponent( in Vector3 inValue)
        {
            _StartPoint = inValue;  
        }

        // DEBUG
        [Header("[ GIZMOS ]")]

        public Color colorOrigin;
        public float originGizmoRadius;
        public Color colorTrace;
        public Color colorCollisionSphere;

        private void OnDrawGizmos()
        {
            Gizmos.color = colorOrigin;
            Gizmos.DrawWireSphere(_StartPoint, originGizmoRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(_CameraTransform.position, _CameraTransform.position +_CameraTransform.forward * 1);
        }
    }
}
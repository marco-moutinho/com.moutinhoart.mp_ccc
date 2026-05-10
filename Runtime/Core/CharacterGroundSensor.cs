using UnityEngine;
namespace MP_CCC
{
    // Created on 02-Mar-2026
    // [ 08 - May - 2026 ] #LastChanged

    public class CharacterGroundSensor : MonoBehaviour
    {
        [Header("[ Parameters ]")]

        [SerializeField, Min(0.0f)]
        protected float _Range = 0.09f;

        [SerializeField]
        protected float _radiusMultiplier = 1f;

        [SerializeField]
        protected LayerMask _layerMask;

        [SerializeField]
        protected QueryTriggerInteraction _queryTriggerInteraction;

        // rto...
        protected bool _isGrounded;
        protected Vector3 _StartPoint;
        // should be +- the capsule radius;
        protected float _sphereCastRadius;
        protected RaycastHit _sensorHit;
        protected RaycastHit _SnapToGroundHit;
        protected Vector3 _GroundNormal;
        protected float _GroundAngle;

        [SerializeField] private bool _drawGizmos = true;

        // added on 02-Mar-2026
        public virtual void Method_Execute()
        {
            Method_HandleSensorRaycast();
            Method_SnapToGroundRayCast();
        }

        // added on 02-Mar-2026
        protected virtual void Method_HandleSensorRaycast()
        {
            float lcRadius = _sphereCastRadius * _radiusMultiplier;
            if(Physics.SphereCast(origin: _StartPoint, radius: lcRadius, direction: -transform.up, hitInfo: out _sensorHit, maxDistance: _Range ,layerMask: _layerMask, queryTriggerInteraction: _queryTriggerInteraction))
            {
                _isGrounded = true;
               // _GroundNormal = _sensorHit.normal;
            }
            else
            {
                _isGrounded = false; 
                //_GroundNormal = Vector3.zero;
            }
        }

        // added on 02-Mar-2026
        // [ 08 - May - 2026 ] #LastChanged
        public virtual void Method_ReturnIsGrounded(out bool outValue)
        {
            outValue = _isGrounded;
        }

        // added on 02-Mar-2026
        public virtual void Method_SetSphereCastRadius(in float inValue)
        {
            _sphereCastRadius = inValue;
        }

        // added on 02-Mar-2026
        public virtual void Method_SetTraceStartPointOffset(in Vector3 inValue)
        {
            _StartPoint =  inValue;
        }

        // added on 10-Mar-2026
        public Vector3 Method_GetGroundNormal()
        {
            return _SnapToGroundHit.normal;
        }

        // added on 10-Mar-2026
        protected void Method_SnapToGroundRayCast()
        {
            if(Physics.Raycast(origin: _StartPoint, direction: -transform.up, out _SnapToGroundHit,1, layerMask: _layerMask, queryTriggerInteraction: _queryTriggerInteraction))
            {
                _GroundNormal = _SnapToGroundHit.normal;
            }
        }

        // GIZMOS
        [Header("< GIZMOS >")]
        public Color _colorSphereCast;
        public Color _colorHit;

        protected virtual void OnDrawGizmos()
        {
            if (_drawGizmos)
            {
                // Draw CastSphere
                Gizmos.color = _colorSphereCast;
                float lcRadius = _sphereCastRadius * _radiusMultiplier;
                Gizmos.DrawWireSphere(_StartPoint, lcRadius);

                Gizmos.DrawWireSphere(_StartPoint + (-transform.up * _Range), lcRadius);

                Gizmos.DrawLine(_StartPoint, _StartPoint + (-transform.up * _Range));

                // Draw Hit
                Gizmos.color = _colorHit;
                Gizmos.DrawWireSphere(_sensorHit.point, 0.25f);

                // Ground Normal
                Gizmos.DrawLine(_SnapToGroundHit.point, _SnapToGroundHit.point + _SnapToGroundHit.normal * 2);

                // SnapToGround Line
                Gizmos.DrawLine(_StartPoint, _StartPoint + -transform.up * 2);
            }
        }
    }
}
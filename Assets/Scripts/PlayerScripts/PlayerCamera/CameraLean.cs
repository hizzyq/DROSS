using UnityEngine;

public class CameraLean : MonoBehaviour
{
    [SerializeField] private float attackDamping = 0.5f;
    [SerializeField] private float decayDamping = 0.3f;
    [SerializeField] private float walkStrength = 2f;
    [SerializeField] private float slideStrength = 0.6f;
    [SerializeField] private float strengthResponse = 5f;
    [SerializeField] private float maxLeanAcceleration = 7f;
    [SerializeField] private float clampSoftness = 2f;
    
    private Vector3 _dampedAcceleration;
    private Vector3 _dampedAccelerationVel;
    private float _smoothStrength;

    public void Initialize()
    {
        _smoothStrength = walkStrength;
    }

    public void UpdateLean(float deltaTime, bool sliding, Vector3 acceleration, Vector3 up)
    {
        var planarAcceleration = Vector3.ProjectOnPlane(acceleration, up);
        
        // Применяем ограничение максимального ускорения с плавностью
        var clampedAcceleration = planarAcceleration;
        if (clampedAcceleration.magnitude > maxLeanAcceleration)
        {
            var excess = clampedAcceleration.magnitude - maxLeanAcceleration;
            var softClampFactor = Mathf.Exp(-excess / clampSoftness);
            clampedAcceleration = clampedAcceleration.normalized * (maxLeanAcceleration + excess * softClampFactor);
        }
        
        var damping = planarAcceleration.magnitude > _dampedAcceleration.magnitude ? attackDamping : decayDamping; 
        _dampedAcceleration = Vector3.SmoothDamp
        (
            current: _dampedAcceleration,
            target: clampedAcceleration,
            currentVelocity: ref _dampedAccelerationVel,
            smoothTime: damping,
            maxSpeed: float.PositiveInfinity,
            deltaTime: deltaTime
        );
        
        var leanAxis = Vector3.Cross(_dampedAcceleration.normalized, up).normalized;
        transform.localRotation = Quaternion.identity;
        
        var effectiveStrength = sliding ? slideStrength : walkStrength;
        
        _smoothStrength = Mathf.Lerp(_smoothStrength, effectiveStrength, 1f - Mathf.Exp(-strengthResponse * deltaTime));
        
        transform.rotation = Quaternion.AngleAxis(_dampedAcceleration.magnitude * _smoothStrength, leanAxis) * transform.rotation;
    }
}

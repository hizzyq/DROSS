using UnityEngine;

public class CameraSpring : MonoBehaviour
{
    [Min(0.01f)] 
    [SerializeField] private float halflife = 0.075f;
    [Space]
    [SerializeField] private float frequency = 18f;
    [Space]
    [SerializeField] private float angularDisplacement = 2f;
    [SerializeField] private float linearDisplacement = 0.05f;
    private Vector3 _springPosition;
    private Vector3 _springVelocity;
    
    
    public void Initialize()
    {
        _springPosition = transform.position;
        _springVelocity = Vector3.zero;
    }

    public void UpdateSpring(float deltaTime, Vector3 up)
    {
        Spring(ref _springPosition, ref _springVelocity, transform.position, halflife, frequency, deltaTime);
        
        var localSpringPosition = _springPosition - transform.position;
        var springHeight = Vector3.Dot(localSpringPosition, up);
        
        transform.localEulerAngles = new Vector3(-springHeight * angularDisplacement, 0f, 0f);
    }
    
    public static void Spring(ref Vector3 current, ref Vector3 velocity, Vector3 target, float halflife, float frequency, float timeStep)
    {
        var dampingRatio = -Mathf.Log(0.5f) / (frequency * halflife);
        float f = 1.0f + 2.0f * timeStep * dampingRatio * frequency;
        float oo = frequency * frequency;
        float hoo = timeStep * oo;
        float hhoo = timeStep * hoo;
        float detInv = 1.0f / (f + hhoo);
        Vector2 detX = f * current + timeStep * velocity + hhoo * target;
        Vector2 detV = velocity + hoo * (target - current);
        current = detX * detInv;
        velocity = detV * detInv;
    }
}

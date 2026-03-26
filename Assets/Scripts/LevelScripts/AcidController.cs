using UnityEngine;

public class AcidController : MonoBehaviour
{
    [Header("Движение")]
    public float riseSpeed = 1.5f;
    public float maxHeight = 100f;

    [Header("Урон")]
    public int damagePerSecond = 10;
    private float _damageTimer = 0f;
    public float damageInterval = 0.5f;

    [Header("Настройки старта")]
    public float startDelay = 3f;
    public bool startOnAwake = true;
    
    [Header("Ускорение")]
    public float acceleration = 0f;
    public float maxSpeed = 6f;
    
    private bool _isRising = false;
    private float _currentSpeed;

    void Start()
    {
        _damageTimer = damageInterval;
        _currentSpeed = riseSpeed;
        if (startOnAwake)
            Invoke(nameof(StartRising), startDelay);
    }

    void Update()
    {
        if (!_isRising) return;
        if (transform.position.y >= maxHeight) return;

        _currentSpeed = Mathf.Min(_currentSpeed + acceleration * Time.deltaTime, maxSpeed);
        transform.position += Vector3.up * _currentSpeed * Time.deltaTime;
    }

    void OnTriggerStay(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player != null)
        {
            _damageTimer -= Time.deltaTime;
            if (_damageTimer <= 0f)
            {
                _damageTimer = damageInterval;
                player.TakeDamage(damagePerSecond);
            }
        }
    }

    public void StartRising() => _isRising = true;
    public void StopRising()  => _isRising = false;
    public float CurrentSpeed => _currentSpeed;
}
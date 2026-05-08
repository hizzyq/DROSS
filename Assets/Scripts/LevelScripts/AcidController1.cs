using UnityEngine;

public class AcidControllerr : MonoBehaviour
{
    [Header("Движение")]
    public float riseSpeed = 1.5f;
    public float maxHeight = 100f;

    [Header("Урон")]
    public int damagePerSecond = 10;
    [SerializeField]private float _damageTimer = 0f;
    public float damageInterval = 0.5f;

    [Header("Настройки старта")]
    public float startDelay = 3f;
    public bool startOnAwake = true;
    
    [Header("Ускорение")]
    public float acceleration = 0f;
    public float maxSpeed = 6f;
    
    [Header("Ссылки")]
    public Transform acid;
    public Player player;
    
    bool  _submerged;
    private bool _isRising = false;
    private float _currentSpeed;

    void Start()
    {
        _damageTimer = damageInterval;
        _currentSpeed = riseSpeed;
    }

    void Update()
    {
        _currentSpeed = Mathf.Min(_currentSpeed + acceleration * Time.deltaTime, maxSpeed);
        transform.position += Vector3.up * _currentSpeed * Time.deltaTime;
        
        _submerged = gameObject.GetComponentInParent<BoxCollider>().bounds.Contains(player.transform.position);
        if (_submerged)
        {
            _damageTimer -= Time.deltaTime;
            if (_damageTimer <= 0f)
            {
                _damageTimer = damageInterval;
                player.TakeDamage(damagePerSecond);
            }
        }
    }

    // void OnTriggerStay(Collider other)
    // {
    //     Debug.Log($"{other.name}");
    //     Player player = other.GetComponentInParent<Player>();
    //     
    //     if (player != null)
    //     {
    //         _damageTimer -= Time.deltaTime;
    //         if (_damageTimer <= 0f)
    //         {
    //             _damageTimer = damageInterval;
    //             player.TakeDamage(damagePerSecond);
    //         }
    //     }
    // }

    public void StartRising() => _isRising = true;
    public void StopRising()  => _isRising = false;
    public float CurrentSpeed => _currentSpeed;
}
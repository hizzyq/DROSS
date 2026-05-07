using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class EnemyBullet : MonoBehaviour
{
    private int _damage;
    private Rigidbody _rb;

    public float lifetime = 6f;

    private Collider _playerCollider;

    public void Init(int damage, float speed, Collider ownerCollider)
    {
        _damage = damage;
        _rb = GetComponent<Rigidbody>();

        _rb.useGravity = false;
        _rb.linearDamping = 0f;
        _rb.linearVelocity = transform.forward * speed;

        if (ownerCollider != null)
            Physics.IgnoreCollision(GetComponent<Collider>(), ownerCollider);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _playerCollider = playerObj.GetComponentInChildren<Collider>();

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (_playerCollider == null) return;
    
        if (GetComponent<Collider>().bounds.Intersects(_playerCollider.bounds))
        {
            _playerCollider.GetComponentInParent<Player>()?.TakeDamage(_damage);
            Destroy(gameObject);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 3 || collision.gameObject.layer == 6)
        {
            Destroy(gameObject);
        }
    }
}
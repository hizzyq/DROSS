using UnityEngine;

public class BURENYA : MonoBehaviour
{
    [SerializeField] public SFXEvent hit;
    [SerializeField] private int bulletLayer = 6;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == bulletLayer)
        {
            AudioManager.Play(hit);
        }
    }
}

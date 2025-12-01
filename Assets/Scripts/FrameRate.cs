using UnityEngine;

public class FrameRate : MonoBehaviour
{
    void Start()
    {
        Application.targetFrameRate = 60;
    }
}

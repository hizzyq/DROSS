using UnityEngine;
using System.Collections;

public class LevelExitTrigger : MonoBehaviour
{
    [Header("Destination")]
    public string nextSceneName;
    public Transform nextLevelSpawnPoint; // Точка, где игрок появится в новой сцене

    [Header("Transition Settings")]
    public float waitBeforeLoad = 1.0f; // Время на затухание экрана

    private bool isTransitioning = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isTransitioning && other.CompareTag("BodyPlayer"))
        {
            StartCoroutine(TransitionSequence(other.gameObject));
        }
    }

    private IEnumerator TransitionSequence(GameObject playerObj)
    {
        isTransitioning = true;
        Player player = playerObj.GetComponentInParent<Player>();
        CheckpointSaveSystem saveSystem = playerObj.GetComponentInParent<CheckpointSaveSystem>();

        // 1. Запускаем твой ScreenBlackout
        if (player.screenBlackout != null)
        {
            player.screenBlackout.enabled = true;
            player.screenBlackout.StartFade(); // Твой метод из ScreenBlackout.cs
        }

        // 2. Небольшая пауза, чтобы экран успел потемнеть
        yield return new WaitForSeconds(waitBeforeLoad);

        // 3. Вызываем переход
        Vector3 pos = nextLevelSpawnPoint != null ? nextLevelSpawnPoint.position : Vector3.zero;
        float rot = nextLevelSpawnPoint != null ? nextLevelSpawnPoint.rotation.eulerAngles.y : 0f;

        saveSystem.TransitionToLevel(player, nextSceneName, pos, rot);
    }
}
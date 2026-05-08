using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaveHUDManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI waveCounterUI;
    public TextMeshProUGUI enemyCounterUI;

    private readonly List<ZombieSpawnController> _spawners = new();

    public void RegisterSpawner(ZombieSpawnController spawner)
    {
        if (spawner != null && !_spawners.Contains(spawner))
            _spawners.Add(spawner);
    }

    private void Update()
    {
        int totalCurrentWave = 0;
        int totalMaxWaves = 0;
        int totalEnemiesAlive = 0;
        int totalEnemiesPerWave = 0;

        for (int i = 0; i < _spawners.Count; i++)
        {
            var s = _spawners[i];
            if (s == null) continue;

            totalCurrentWave += s.currentWave;
            totalMaxWaves += s.maxWaves;
            totalEnemiesAlive += s.currentZombiesAlive.Count;
            totalEnemiesPerWave += s.currentZombiesPerWave;
        }

        if (waveCounterUI != null)
            waveCounterUI.text = $"Waves {totalCurrentWave}/{totalMaxWaves}";

        if (enemyCounterUI != null)
            enemyCounterUI.text = $"Enemies {totalEnemiesAlive}/{totalEnemiesPerWave}";
    }
}
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class ZombieSpawnController : MonoBehaviour
{
    [Header("Wave Settings")]
    public int initialZombiesPerWave = 5;
    public int currentZombiesPerWave;
    public float spawnDelay = 0.5f;
    public int maxWaves = 5;

    [Header("Cooldown Settings")]
    public int currentWave = 0;
    public float waveCooldown = 10f;
    public bool inCooldown = false;
    public float cooldownCounter = 0f;

    [Header("Trigger Settings")]
    private bool wavesStarted = false;
    private bool allWavesCompleted = false;

    [Header("Buttons To Disable")]
    public List<PhysicalButton> buttonsToDisable = new List<PhysicalButton>();

    [Header("References")]
    public GameObject zombiePrefab;
    public TextMeshProUGUI cooldownCounterUI;
    public TextMeshProUGUI waveCounterUI;
    public TextMeshProUGUI enemyCounterUI;

    [Header("Spawn Points")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Runtime")]
    public List<Enemy> currentZombiesAlive = new List<Enemy>();

    private bool isSpawningWave = false;
    private int lastSpawnIndex = -1;

    private void Start()
    {
        currentZombiesPerWave = initialZombiesPerWave;
        cooldownCounter = waveCooldown;

        if (cooldownCounterUI != null)
        {
            cooldownCounterUI.gameObject.SetActive(false);
        }
        if (waveCounterUI != null) waveCounterUI.gameObject.SetActive(false);
        if (enemyCounterUI != null) enemyCounterUI.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (wavesStarted || allWavesCompleted)
            return;

        if (other.transform.root.CompareTag("Player"))
        {
            wavesStarted = true;

            SetButtonsState(false); // ��������� ������

            StartNextWave();
        }
        if (waveCounterUI != null) waveCounterUI.gameObject.SetActive(true);
        if (enemyCounterUI != null) enemyCounterUI.gameObject.SetActive(true);
    }

    private void StartNextWave()
    {
        if (currentWave >= maxWaves)
        {
            allWavesCompleted = true;
            if (waveCounterUI != null) waveCounterUI.gameObject.SetActive(false);
            if (enemyCounterUI != null) enemyCounterUI.gameObject.SetActive(false);
            SetButtonsState(true); // �������� ������ �������
            Debug.Log("��� ����� ���������.");
            return;
        }

        currentWave++;
        StartCoroutine(SpawnWave());
    }

    private IEnumerator SpawnWave()
    {
        if (zombiePrefab == null)
        {
            Debug.LogError("Zombie Prefab �� �������� � ����������.");
            yield break;
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("������ spawnPoints ����. ������ ����� ������ � ����������.");
            yield break;
        }

        isSpawningWave = true;

        for (int i = 0; i < currentZombiesPerWave; i++)
        {
            int randomIndex;

            do
            {
                randomIndex = Random.Range(0, spawnPoints.Count);
            }
            while (spawnPoints.Count > 1 && randomIndex == lastSpawnIndex);

            lastSpawnIndex = randomIndex;

            Transform randomSpawnPoint = spawnPoints[randomIndex];

            GameObject zombie = Instantiate(
                zombiePrefab,
                randomSpawnPoint.position,
                randomSpawnPoint.rotation
            );

            Enemy enemyScript = zombie.GetComponent<Enemy>();

            if (enemyScript != null)
            {
                currentZombiesAlive.Add(enemyScript);
            }
            else
            {
                Debug.LogWarning("� ���������� ����� ����������� ��������� Enemy.");
            }

            yield return new WaitForSeconds(spawnDelay);
        }

        isSpawningWave = false;
    }

    private void Update()
    {
        for (int i = currentZombiesAlive.Count - 1; i >= 0; i--)
        {
            Enemy zombie = currentZombiesAlive[i];

            if (zombie == null || zombie.isDead)
            {
                currentZombiesAlive.RemoveAt(i);
            }
        }

        if (!allWavesCompleted && currentZombiesAlive.Count == 0 && !inCooldown && !isSpawningWave && wavesStarted)
        {
            if (currentWave < maxWaves)
            {
                StartCoroutine(WaveCooldown());
            }
            else
            {
                allWavesCompleted = true;
                SetButtonsState(true); // �������� ������ �������
                Debug.Log("��� ����� ���������.");
            }
        }

        if (inCooldown)
        {
            cooldownCounter -= Time.deltaTime;

            if (cooldownCounter < 0f)
            {
                cooldownCounter = 0f;
            }
        }
        else
        {
            cooldownCounter = waveCooldown;
        }

        if (cooldownCounterUI != null)
        {
            cooldownCounterUI.text = cooldownCounter.ToString("F1");
        }
        if (waveCounterUI != null)
            waveCounterUI.text = $"Wave {currentWave}/{maxWaves}";

        if (enemyCounterUI != null)
            enemyCounterUI.text = $"Enemies {currentZombiesAlive.Count}/{currentZombiesPerWave}";
    }

    private IEnumerator WaveCooldown()
    {
        inCooldown = true;

        if (cooldownCounterUI != null)
        {
            cooldownCounterUI.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(waveCooldown);

        inCooldown = false;

        if (cooldownCounterUI != null)
        {
            cooldownCounterUI.gameObject.SetActive(false);
        }

        if (currentWave < maxWaves)
        {
            currentZombiesPerWave += 2;
            StartNextWave();
        }
        else
        {
            allWavesCompleted = true;
            if (waveCounterUI != null) waveCounterUI.gameObject.SetActive(false);
            if (enemyCounterUI != null) enemyCounterUI.gameObject.SetActive(false);
            SetButtonsState(true); 
            Debug.Log("��� ����� ���������.");
        }
    }

    private void SetButtonsState(bool state)
    {
        for (int i = 0; i < buttonsToDisable.Count; i++)
        {
            if (buttonsToDisable[i] != null)
            {
                buttonsToDisable[i].enabled = state;

                Collider buttonCollider = buttonsToDisable[i].GetComponent<Collider>();
                if (buttonCollider != null)
                {
                    buttonCollider.enabled = state;
                }

                Debug.Log(buttonsToDisable[i].name + " enabled = " + state);
            }
        }
    }
}
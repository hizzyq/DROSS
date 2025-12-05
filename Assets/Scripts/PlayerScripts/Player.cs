using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public int HP = 100;

    public GameObject bloodyScreen;
    public PlayerDeathManager deathManager;
    public TextMeshProUGUI playerHealthUI;
    public GameObject gameOverUI;
    public Camera mainCamera;
    public ScreenBlackout screenBlackout;

    public bool isDead;

    private void Start()
    {
        playerHealthUI.text = $"Health: {HP}";
    }

    public void TakeDamage(int damageAmount)
    {
        HP -= damageAmount;

        if (HP <= 0)
        {
            print("Player dead");
            PlayerDead();
            isDead = true;
        }
        else
        {
            print("Player hit");
            StartCoroutine(BloodyScreenEffect());
            playerHealthUI.text = $"Health: {HP}";
            SoundManager.Instance.playerChannel.PlayOneShot(SoundManager.Instance.playerHurt);
        }
    }

    public void PlayerDead()
    {
        SoundManager.Instance.playerChannel.PlayOneShot(SoundManager.Instance.playerDeath);

        GetComponent<Dashing>().enabled = false;
        GetComponent<PlayerMovementAdvanced>().enabled = false;
        GetComponent<Sliding>().enabled = false;
        GetComponent<WallRunning>().enabled = false;

        mainCamera.GetComponent<PlayerCam>().enabled = false;
        //mainCamera.GetComponent<Animator>().enabled = true;
        //mainCamera.transform.rotation = Quaternion.Euler(0, 0, 90);

        playerHealthUI.gameObject.SetActive(false);

        
        screenBlackout.enabled = true;
        screenBlackout.StartFade();
        StartCoroutine(ShowGameOverUI());
        deathManager.KillPlayer();
    }

    private IEnumerator ShowGameOverUI()
    {
        yield return new WaitForSeconds(1f);
        gameOverUI.gameObject.SetActive(true);
    }

    private IEnumerator BloodyScreenEffect()
    {
        if (bloodyScreen.activeInHierarchy == false)
        {
            bloodyScreen.SetActive(true);
        }

        var image = bloodyScreen.GetComponentInChildren<Image>();

        // Set the initial alpha value to 1 (fully visible).
        Color startColor = image.color;
        startColor.a = 1f;
        image.color = startColor;

        float duration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Calculate the new alpha value using Lerp.
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);

            // Update the color with the new alpha value.
            Color newColor = image.color;
            newColor.a = alpha;
            image.color = newColor;

            // Increment the elapsed time.
            elapsedTime += Time.deltaTime;

            yield return null; ; // Wait for the next frame.
        }

        if (bloodyScreen.activeInHierarchy)
        {
            bloodyScreen.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<AmmoBox>())
        {
            var ammoBox = other.gameObject.GetComponent<AmmoBox>();
            WeaponManager.Instance.PickupAmmo(ammoBox);
            Destroy(ammoBox.gameObject);
            ammoBox = null;
        }
    }
}

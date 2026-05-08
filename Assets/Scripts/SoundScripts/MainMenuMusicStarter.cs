using UnityEngine;

public class MainMenuMusicStarter : MonoBehaviour
{
    [Tooltip("Музыкальное состояние для главного меню.")]
    public MusicState menuState = MusicState.Explore;

    private void Start()
    {
        AudioManager.SetMusicState(menuState);
        AudioManager.SetCombatIntensity(0f); // чтобы стемы не разгонялись
    }
}
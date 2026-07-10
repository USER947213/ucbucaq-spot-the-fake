using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Tooltip("Звук правильного нахождения уловки")]
    public AudioSource successSound;
    
    [Tooltip("Звук промаха или окончания времени")]
    public AudioSource errorSound;
    
    [Tooltip("Обычный клик (опционально)")]
    public AudioSource clickSound;
    
    [Tooltip("Проигрыш (Game Over)")]
    public AudioSource gameoverSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySuccess()
    {
        if (successSound != null) successSound.Play();
    }

    public void PlayError()
    {
        if (errorSound != null) errorSound.Play();
    }

    public void PlayClick()
    {
        if (clickSound != null) clickSound.Play();
    }

    public void PlayGameOver()
    {
        if (gameoverSound != null) gameoverSound.Play();
    }
}

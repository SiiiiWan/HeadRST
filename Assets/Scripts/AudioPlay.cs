using UnityEngine;

public class AudioPlay : MonoBehaviour
{
    private static AudioSource[] Sounds;
    private static AudioSource ClickSound;
    
    private void Awake()
    {
        Sounds = gameObject.GetComponents<AudioSource>();

        ClickSound = Sounds[0];
    }

    public static void PlayClickSound()
    {
        ClickSound.Play();
    }
}

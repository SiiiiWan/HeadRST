
using System.Collections.Generic;
using UnityEngine;

public class AudioPlay : MonoBehaviour
{
    private static AudioSource[] Sounds;
    private static AudioSource ClickSound;
    // private static AudioSource ReverseClickSound;
    
    private void Awake()
    {
        Sounds = new List<AudioSource>().ToArray();

        Sounds = gameObject.GetComponents<AudioSource>();

        ClickSound = Sounds[0];
        // ReverseClickSound = Sounds[1];
    }

    public static void PlayClickSound()
    {
        ClickSound.Play();
    }

    // public static void PlayReverseClickSound()
    // {
    //     ReverseClickSound.Play();
    // }
}

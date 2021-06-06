using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public AudioClip clip;
    public string name;

    [Range(0.1f, 1f)]
    public float volume = 1.0f;

    [HideInInspector]
    public AudioSource source;
}

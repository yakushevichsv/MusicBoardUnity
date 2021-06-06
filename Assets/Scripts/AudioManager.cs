using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;

    // Start is called before the first frame update
    void Awake()
    {

        var index = 0;
        foreach (var sound in sounds)
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.volume = sound.volume;
            source.clip = sound.clip;
            sound.source = source;

            if (sound.name == null || sound.name.Length == 0)
                sound.name = string.Format("{0}", index); // TDOO: index..
            index += 1;
        }
    }

    public void Play(string name, bool force = false)
    {
        var sound = sounds.FirstOrDefault(sound => sound.name == name);
        if (sound == null)
            return;

        if (!sound.source.isPlaying || force)
        {
            sound.source.Stop();
            sound.source.Play();
        }
        /*else
        {
            var diff = sound.source.clip.length - sound.source.time;
            if (diff >= 0)
                sound.source.PlayDelayed(diff);
        }*/
    }

    public void Play(int index, bool force = false)
    {
        if (index < 0 || index >= sounds.Length)
            return;
        Play(sounds[index].name, force);
    }

    public void StopPlaying(string name)
    {
        var sound = sounds.FirstOrDefault(sound => sound.name == name);
        if (sound == null)
            return;

        if (sound.source.isPlaying)
            sound.source.Stop();
    }

    public void StopPlaying(int index)
    {
        if (index < 0 || index >= sounds.Length)
            return;
        StopPlaying(sounds[index].name);
    }
}

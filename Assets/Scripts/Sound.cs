using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Internal;

[Serializable]
public class Sound
{
    public string name;
    public bool loop;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume;
    [Range(0f, 3f)] public float pitch;
    [HideInInspector] public AudioSource source;
}
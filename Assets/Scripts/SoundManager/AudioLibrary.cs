using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(menuName = "Audio/Audio Library")]
public class AudioLibrary : ScriptableObject
{
    public List<AudioEvent> events;

    public AudioEvent Get(string id)
    {
        return events.FirstOrDefault(e => e.id == id);
    }
}

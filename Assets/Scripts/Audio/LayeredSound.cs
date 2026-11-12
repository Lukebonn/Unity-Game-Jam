using UnityEngine;
using UnityEngine.Audio;

// Plays one random clip from every layer at the same time, each with its own
// random volume and pitch. Example: a gunshot built from explosion + slam + mech
// + laser layers. Hook Play() up to a UnityEvent such as PlayerCombat.OnFire.
public class LayeredSound : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public string name;
        public AudioClip[] clips;
        [Range(0f, 1f)] public float minVolume = 1f;
        [Range(0f, 1f)] public float maxVolume = 1f;
        [Range(0.1f, 3f)] public float minPitch = 1f;
        [Range(0.1f, 3f)] public float maxPitch = 1f;
    }

    [SerializeField] private Layer[] layers;

    [Tooltip("Optional. Leave empty to play straight to the default output.")]
    [SerializeField] private AudioMixerGroup output;

    [Tooltip("How many sounds can overlap. When all are busy, the oldest one is cut off.")]
    [SerializeField] private int voices = 32;

    private AudioSource[] sources;
    private int nextSource;

    private void Awake()
    {
        // Each sound gets its own AudioSource so its pitch and volume don't
        // change sounds that are still ringing out from earlier shots.
        sources = new AudioSource[Mathf.Max(1, voices)];
        for (int i = 0; i < sources.Length; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.outputAudioMixerGroup = output;
            sources[i] = source;
        }
    }

    public void Play()
    {
        foreach (Layer layer in layers)
        {
            if (layer.clips.Length == 0) continue;

            AudioSource source = sources[nextSource];
            nextSource = (nextSource + 1) % sources.Length;

            source.clip = layer.clips[Random.Range(0, layer.clips.Length)];
            source.volume = Random.Range(layer.minVolume, layer.maxVolume);
            source.pitch = Random.Range(layer.minPitch, layer.maxPitch);
            source.Play();
        }
    }
}

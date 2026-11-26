using UnityEngine;

/// <summary>Lightweight generated game SFX; no external audio assets required.</summary>
public static class RunCicaakAudio
{
    private static AudioClip coinClip;
    private static AudioClip escapeClip;
    private static AudioClip caughtClip;

    public static void PlayCoin()
    {
        Play(ref coinClip, "RCC_Coin", new[] { 880f, 1320f }, 0.16f, 0.22f);
    }

    public static void PlayEscape()
    {
        Play(ref escapeClip, "RCC_Escape", new[] { 523f, 659f, 784f, 1046f }, 0.55f, 0.2f);
    }

    public static void PlayCaught()
    {
        Play(ref caughtClip, "RCC_Caught", new[] { 392f, 294f, 196f }, 0.42f, 0.18f);
    }

    private static void Play(ref AudioClip clip, string clipName, float[] notes, float duration, float volume)
    {
        if (PlayerPrefs.GetInt("RUN_CICAAK_SFX", 1) == 0) return;
        if (clip == null) clip = BuildClip(clipName, notes, duration);

        Vector3 position = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    private static AudioClip BuildClip(string clipName, float[] notes, float duration)
    {
        const int sampleRate = 22050;
        int count = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[count];

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)sampleRate;
            float notePosition = t / duration * notes.Length;
            int noteIndex = Mathf.Min(notes.Length - 1, Mathf.FloorToInt(notePosition));
            float localT = (notePosition - noteIndex) * duration / notes.Length;
            float attack = Mathf.Clamp01(localT / 0.018f);
            float release = Mathf.Clamp01((duration / notes.Length - localT) / 0.035f);
            float envelope = Mathf.Min(attack, release) * Mathf.Exp(-2.1f * t);
            samples[i] = Mathf.Sin(2f * Mathf.PI * notes[noteIndex] * t) * envelope * 0.7f;
        }

        AudioClip result = AudioClip.Create(clipName, count, 1, sampleRate, false);
        result.SetData(samples, 0);
        return result;
    }
}

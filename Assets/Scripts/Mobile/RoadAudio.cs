using UnityEngine;

namespace RenkYolu.Mobile
{
    internal sealed class RoadAudio
    {
        private readonly AudioSource source;
        private readonly AudioClip tick;
        private readonly AudioClip hop;
        private readonly AudioClip shard;
        private readonly AudioClip clear;
        private readonly AudioClip fail;

        internal RoadAudio(GameObject target)
        {
            source = target.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.volume = .36f;
            tick = Tone("Seçim", .065f, 420);
            hop = Tone("Zıplama", .10f, 470);
            shard = Tone("Kristal", .20f, 630, 880);
            clear = Tone("Başarı", .40f, 440, 550, 660, 880);
            fail = Tone("Tekrar", .26f, 310, 210);
        }

        internal bool Muted { set => source.mute = value; }
        internal void Select() => source.PlayOneShot(tick, .65f);
        internal void Hop() => source.PlayOneShot(hop, .67f);
        internal void Shard() => source.PlayOneShot(shard);
        internal void Result(bool won) => source.PlayOneShot(won ? clear : fail);

        private static AudioClip Tone(string title, float duration, params float[] notes)
        {
            const int rate = 22050;
            var samples = new float[Mathf.CeilToInt(rate * duration)];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i * notes.Length / (float)samples.Length;
                int index = (int)t;
                float phase = t - index;
                float time = phase * duration / notes.Length;
                float wave = 2 * Mathf.PI * notes[index] * time;
                float envelope = Mathf.Min(1, phase * 15) * Mathf.Pow(1 - phase, 1.65f);
                samples[i] = (Mathf.Sin(wave) * .6f + Mathf.Sin(wave * 2) * .10f) * envelope;
            }

            AudioClip clip = AudioClip.Create(title, samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

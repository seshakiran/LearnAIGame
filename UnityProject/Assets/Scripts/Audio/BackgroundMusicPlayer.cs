using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LearnAIGame.Audio
{
    /// Shuffles through every clip in Resources/Audio and loops the playlist
    /// indefinitely as background music — drop more mp3/wav/ogg files in that
    /// folder and they're picked up automatically, no code changes needed.
    public class BackgroundMusicPlayer : MonoBehaviour
    {
        private const string ResourceFolder = "Audio";
        private const float Volume = 0.35f;

        private AudioSource _source;
        private List<AudioClip> _playlist;

        public void Pause() => _source?.Pause();
        public void Resume() => _source?.UnPause();

        public static BackgroundMusicPlayer CreateAndPlay(Transform parent)
        {
            var go = new GameObject("BackgroundMusic");
            go.transform.SetParent(parent, false);
            var player = go.AddComponent<BackgroundMusicPlayer>();
            player.Init();
            return player;
        }

        private void Init()
        {
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.loop = false;
            _source.volume = Volume;
            _source.spatialBlend = 0f;

            var clips = Resources.LoadAll<AudioClip>(ResourceFolder);
            _playlist = new List<AudioClip>(clips);

            if (_playlist.Count == 0)
            {
                Debug.LogWarning($"BackgroundMusicPlayer: no audio clips found in Resources/{ResourceFolder}");
                return;
            }

            Shuffle(_playlist);
            StartCoroutine(PlayLoop());
        }

        private IEnumerator PlayLoop()
        {
            var index = 0;
            while (true)
            {
                _source.clip = _playlist[index];
                _source.Play();

                yield return new WaitWhile(() => _source.isPlaying);

                index++;
                if (index >= _playlist.Count)
                {
                    index = 0;
                    Shuffle(_playlist);
                }
            }
        }

        private static void Shuffle(IList<AudioClip> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}

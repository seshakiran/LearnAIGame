using System.Collections;
using LearnAIGame.Bootstrap;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace LearnAIGame.Video
{
    /// Plays the first VideoClip found in Resources/Video full-screen, with a skip
    /// button. If no clip has been added yet this is a no-op, so the loop keeps
    /// working during development — drop an mp4 into Assets/Resources/Video and
    /// it's picked up automatically, no code changes needed.
    public static class TopicVideoPlayer
    {
        private const string ResourceFolder = "Video";

        public static IEnumerator Play(Transform canvasParent)
        {
            var clips = Resources.LoadAll<VideoClip>(ResourceFolder);
            if (clips == null || clips.Length == 0)
            {
                Debug.LogWarning($"TopicVideoPlayer: no video clip found in Resources/{ResourceFolder} — skipping video step.");
                yield break;
            }

            var clip = clips[0];
            var panel = UIFactory.CreateFullScreenPanel(canvasParent, Color.black, "VideoPanel");

            var videoGo = new GameObject("VideoSurface", typeof(RectTransform), typeof(RawImage));
            videoGo.transform.SetParent(panel, false);
            var videoRect = videoGo.GetComponent<RectTransform>();
            videoRect.anchorMin = Vector2.zero;
            videoRect.anchorMax = Vector2.one;
            videoRect.offsetMin = new Vector2(0, 140);
            videoRect.offsetMax = new Vector2(0, -140);

            var renderTexture = new RenderTexture(Mathf.Max((int)clip.width, 16), Mathf.Max((int)clip.height, 16), 0);
            videoGo.GetComponent<RawImage>().texture = renderTexture;

            var playerGo = new GameObject("VideoPlayer", typeof(VideoPlayer));
            playerGo.transform.SetParent(panel, false);
            var videoPlayer = playerGo.GetComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = renderTexture;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            videoPlayer.isLooping = false;
            videoPlayer.clip = clip;

            // Direct mode alone doesn't play sound — the audio track has to be
            // explicitly enabled and given a volume, or it stays silent.
            videoPlayer.EnableAudioTrack(0, true);
            videoPlayer.SetDirectAudioVolume(0, 1f);
            videoPlayer.SetDirectAudioMute(0, false);

            var skipped = false;
            var finished = false;
            videoPlayer.loopPointReached += _ => finished = true;

            UIFactory.CreateButton(panel, "Skip ▶", new Vector2(0, -820), new Vector2(240, 80), () => skipped = true, Color.white, new Color(0f, 0f, 0f, 0.5f));

            videoPlayer.Prepare();
            yield return new WaitUntil(() => videoPlayer.isPrepared || skipped);

            if (!skipped)
            {
                videoPlayer.Play();
                yield return new WaitUntil(() => finished || skipped);
            }

            videoPlayer.Stop();
            Object.Destroy(panel.gameObject);
            Object.Destroy(renderTexture);
        }
    }
}

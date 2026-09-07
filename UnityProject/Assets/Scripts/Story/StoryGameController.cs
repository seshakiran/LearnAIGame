using System;
using System.Collections.Generic;
using LearnAIGame.Audio;
using LearnAIGame.Bootstrap;
using UnityEngine;
using UnityEngine.UI;

namespace LearnAIGame.Story
{
    /// Data-driven campaign player. Pages use measured vertical layout and a scroll
    /// viewport, so evidence and choices remain readable on narrow mobile screens.
    public class StoryGameController : MonoBehaviour
    {
        private StoryCampaign campaign;
        private StorySave save;
        private Canvas canvas;
        private RectTransform page, stack;
        private BackgroundMusicPlayer music;
        private bool muted;
        private StoryChapter Chapter => campaign.chapters[save.chapter];
        private StoryBeat Beat => Chapter.beats[save.beat];
        private readonly Color ink = GamePalette.TextLight;
        private readonly Color mutedInk = GamePalette.TextMuted;
        private readonly Color accent = GamePalette.Amber;

        private void Start()
        {
            canvas = UIFactory.CreateRootCanvas();
            var scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            var asset = Resources.Load<TextAsset>("last_train");
            if (asset == null)
            {
                NewPage("CONTENT UNAVAILABLE", "The case file could not be loaded.");
                Label("Restore Resources/last_train.json and reopen the game.");
                return;
            }
            campaign = JsonUtility.FromJson<StoryCampaign>(asset.text);
            save = StoryProgress.Load(campaign);
            music = BackgroundMusicPlayer.CreateAndPlay(transform);
            muted = PlayerPrefs.GetInt("LearnAIGame.Story.Muted", 0) == 1;
            if (muted) music.Pause();
            Home();
        }

        private void NewPage(string eyebrow, string title)
        {
            if (page != null) { page.gameObject.SetActive(false); Destroy(page.gameObject); }
            page = UIFactory.CreateFullScreenPanel(canvas.transform, GamePalette.BackgroundDeep, "StoryPage");
            var safe = new GameObject("SafeArea", typeof(RectTransform)).GetComponent<RectTransform>();
            safe.SetParent(page, false);
            var r = Screen.safeArea;
            safe.anchorMin = new Vector2(r.xMin / Screen.width, r.yMin / Screen.height);
            safe.anchorMax = new Vector2(r.xMax / Screen.width, r.yMax / Screen.height);
            safe.offsetMin = safe.offsetMax = Vector2.zero;
            safe.gameObject.AddComponent<StoryResponsiveLayout>();
            var viewport = UIFactory.CreateFullScreenPanel(safe, GamePalette.BackgroundDeep, "Viewport");
            viewport.offsetMin = new Vector2(16, 12);
            viewport.offsetMax = new Vector2(-16, -12);
            viewport.gameObject.AddComponent<RectMask2D>();
            var scroll = viewport.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40;
            stack = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter)).GetComponent<RectTransform>();
            stack.SetParent(viewport, false);
            stack.anchorMin = new Vector2(0, 1); stack.anchorMax = Vector2.one;
            stack.pivot = new Vector2(.5f, 1); stack.sizeDelta = Vector2.zero;
            var layout = stack.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 12; layout.padding = new RectOffset(0, 0, 10, 24);
            layout.childControlHeight = layout.childControlWidth = true;
            layout.childForceExpandHeight = false; layout.childForceExpandWidth = true;
            stack.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = stack; scroll.viewport = viewport;
            Label(eyebrow.ToUpperInvariant(), 20, accent);
            Label(title, 54, ink, true);
        }

        private Text Label(string value, int size = 28, Color? color = null, bool bold = false, Transform parent = null)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent ?? stack, false);
            var text = go.GetComponent<Text>();
            text.font = UIFactory.GetPlayfulFont(); text.fontSize = Mathf.Max(13, Mathf.RoundToInt(size / 1.65f));
            text.lineSpacing = 1.15f;
            text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            text.color = color ?? ink; text.text = value;
            text.supportRichText = false; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private RectTransform Card(Color? color = null)
        {
            var go = new GameObject("Dossier", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            go.transform.SetParent(stack, false);
            var image = go.GetComponent<Image>(); image.color = color ?? GamePalette.CardSurface;
            image.sprite = UIFactory.GetRoundedSprite(20); image.type = Image.Type.Sliced;
            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 14, 14); layout.spacing = 9;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            return go.GetComponent<RectTransform>();
        }

        private void Button(string text, Action action, bool primary = false, bool enabled = true)
        {
            var root = Card(primary ? accent : GamePalette.ChipSurface);
            var label = Label(text, 28, primary ? GamePalette.TextDark : ink, true, root);
            var b = root.gameObject.AddComponent<Button>();
            root.gameObject.AddComponent<LayoutElement>().minHeight = 48;
            b.targetGraphic = root.GetComponent<Image>(); b.interactable = enabled;
            if (!enabled) label.color = mutedInk;
            b.onClick.AddListener(() => action());
        }

        private void SceneArt(StoryBeat beat, Action back)
        {
            var texture = Resources.Load<Texture2D>(beat.artResource);
            if (texture == null) return;
            var root = Card();
            root.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(12, 12, 12, 12);
            var frame = new GameObject("Scene frame", typeof(RectTransform), typeof(LayoutElement));
            frame.transform.SetParent(root, false);
            frame.GetComponent<LayoutElement>().preferredHeight = 200;
            var go = new GameObject("Story illustration", typeof(RectTransform), typeof(RawImage), typeof(AspectRatioFitter));
            go.transform.SetParent(frame.transform, false);
            go.GetComponent<RawImage>().texture = texture;
            go.GetComponent<RawImage>().raycastTarget = false;
            var fitter = go.GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = (float)texture.width / texture.height;
            go.AddComponent<StoryArtwork>();
            Label("  ILLUSTRATED SCENE / NOT AN EVIDENCE RECORD", 19, accent, parent: root);
            Label("  " + beat.artCaption, 25, ink, parent: root);
            Button("Expand scene / full-screen view", () => ImmersiveScene(beat, back));
        }

        private void LearningGoal(StoryBeat beat)
        {
            if (string.IsNullOrEmpty(beat.learningObjective)) return;
            var card = Card();
            Label(beat.assessment ? "WHAT YOU ARE APPLYING" : "WHAT YOU ARE LEARNING", 20, GamePalette.Blue, true, card);
            Label(beat.learningObjective, 27, ink, parent: card);
        }

        private void ImmersiveScene(StoryBeat beat, Action back)
        {
            NewPage("Illustrated scene / not an evidence record", beat.title);
            var backdrop = new GameObject("Full-screen artwork", typeof(RectTransform), typeof(RawImage));
            var rect = backdrop.GetComponent<RectTransform>();
            rect.SetParent(page, false); rect.SetAsFirstSibling();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            backdrop.GetComponent<RawImage>().texture = Resources.Load<Texture2D>(beat.artResource);
            backdrop.GetComponent<RawImage>().raycastTarget = false;
            var artwork = backdrop.AddComponent<StoryArtwork>();
            artwork.fullScreen = true;
            artwork.focusX = beat.artFocusX;
            // A wash protects heading contrast; evidence and decisions stay in the
            // comic view so decorative art never competes with original records.
            stack.parent.GetComponent<Image>().color = new Color(0.02f, 0.04f, 0.07f, .65f);
            var space = new GameObject("Scene breathing room", typeof(RectTransform), typeof(LayoutElement));
            space.transform.SetParent(stack, false);
            space.GetComponent<LayoutElement>().preferredHeight = 180;
            var caption = Card(); Label(beat.artCaption, 30, ink, parent: caption);
            LearningGoal(beat);
            Button("Return to comic view", back, true);
        }

        private void Home()
        {
            NewPage("Learn AI / Story 01", campaign.title);
            Label(campaign.subtitle, 25, mutedInk);
            SceneArt(campaign.chapters[0].beats[0], Home);
            Label(campaign.premise, 30);
            Button(save.completed ? "View your case outcome" : save.decisions.Count > 0 || save.beat > 0 ? "Resume investigation" : "Take the call", () => { if (save.completed) Ending(); else ShowBeat(); }, true);
            Button("Case chapters & learning path", Chapters);
            Button("Evidence desk & decision record", () => Notebook(Home));
            Button(muted ? "Sound: off / turn on" : "Sound: on / turn off", () =>
            {
                muted = !muted; if (muted) music.Pause(); else music.Resume();
                PlayerPrefs.SetInt("LearnAIGame.Story.Muted", muted ? 1 : 0); PlayerPrefs.Save(); Home();
            });
            if (save.decisions.Count > 0 || save.beat > 0 || save.completed) Button("Restart this investigation", Restart);
            Label("Fictional police thriller · Read at your own pace · Progress saved on this device\nAll radio, camera and witness material is presented as readable case records. No prior AI knowledge needed.", 21, mutedInk);
        }

        private void Chapters()
        {
            NewPage("Case navigation", "One night. Six chapters.");
            for (int i = 0; i < campaign.chapters.Length; i++)
            {
                var chapter = campaign.chapters[i];
                var card = Card();
                Label($"{i + 1:00} / {chapter.time} / {(save.completed || i < save.chapter ? "CLOSED" : i == save.chapter ? "ACTIVE" : "UPCOMING")}", 20, accent, parent: card);
                Label(chapter.title, 32, ink, true, card);
                Label(chapter.summary, 25, mutedInk, parent: card);
                Label(chapter.concept, 22, GamePalette.Blue, parent: card);
            }
            Label("Beyond Last Train", 35, ink, true);
            Label("Future cases will build toward embeddings and retrieval design, agent planning, memory, multi-agent coordination, evaluation, model adaptation, and deployment tradeoffs. These cases are planned, not yet playable.", 25, mutedInk);
            Button("Back to command", Home);
        }

        private bool? PreviousSupported()
        {
            if (save.decisions.Count == 0) return null;
            var last = save.decisions[save.decisions.Count - 1];
            foreach (var chapter in campaign.chapters)
                foreach (var beat in chapter.beats)
                    if (beat.id == last.beatId) return beat.choices[last.choice].supported;
            return null;
        }

        private void ShowBeat()
        {
            if (save.completed) { Ending(); return; }
            var beat = Beat;
            var decision = save.Find(beat.id);
            if (decision != null) { Outcome(beat, decision.choice); return; }
            NewPage($"{Chapter.time} / CH {save.chapter + 1:00} / {Chapter.location}", beat.title);
            SceneArt(beat, ShowBeat);
            LearningGoal(beat);
            Label(beat.speaker, 22, GamePalette.Blue, true);
            var previous = PreviousSupported();
            var reaction = previous == false ? beat.afterError : previous == true ? beat.afterSuccess : null;
            if (!string.IsNullOrEmpty(reaction)) Label(reaction, 25, accent);
            Label(beat.body, 30);
            if (beat.evidence.Length > 0)
            {
                Label("OPEN THE ORIGINALS", 20, accent, true);
                foreach (var evidence in beat.evidence)
                {
                    var captured = evidence;
                    Button((save.inspected.Contains(evidence.id) ? "READ / " : "OPEN / ") + evidence.title, () => Evidence(captured));
                }
            }
            if (beat.choices.Length > 0)
            {
                Label(beat.question, 32, ink, true);
                bool ready = save.chapter == 0 || Array.TrueForAll(beat.evidence, e => save.inspected.Contains(e.id));
                if (!ready) Label("Inspect each source above before sending your decision.", 23, accent);
                if (beat.assessment) Label("FINAL BRIEFING / Feedback held until the case debrief.", 22, GamePalette.Blue);
                if (beat.interaction == "timeline") Timeline(beat);
                else for (int i = 0; i < beat.choices.Length; i++)
                {
                    int selected = i;
                    Button($"{i + 1:00} / {beat.choices[i].text}", () => Decide(selected), enabled: ready);
                }
            }
            else Button("Continue transmission", Next, true);
            Button("Evidence desk", () => Notebook(ShowBeat));
            Button("Save & return to command", Home);
        }

        private void Evidence(StoryEvidence evidence)
        {
            NewPage("Original record / " + evidence.id, evidence.title);
            Label(evidence.source, 24, GamePalette.Blue);
            LearningGoal(Beat);
            var card = Card(); Label(evidence.text, 30, ink, parent: card);
            Label("A record can itself contain an unverified claim. Compare what it says with who supplied it and what it actually establishes.", 22, mutedInk);
            Button("Mark reviewed & return", () =>
            {
                if (!save.inspected.Contains(evidence.id)) save.inspected.Add(evidence.id);
                StoryProgress.Store(campaign, save); ShowBeat();
            }, true);
        }

        private void Timeline(StoryBeat beat)
        {
            if (save.draftBeatId != beat.id || save.draftOrder == null)
            {
                save.draftBeatId = beat.id;
                save.draftOrder = new List<string>();
            }
            Label("TAP EVENTS / EARLIEST FIRST", 20, accent);
            for (int i = 0; i < save.draftOrder.Count; i++)
            {
                var item = Array.Find(beat.timelineItems, e => e.id == save.draftOrder[i]);
                Label($"{i + 1}. {item.title}", 26, GamePalette.Blue);
            }
            foreach (var item in beat.timelineItems)
            {
                var captured = item;
                if (save.draftOrder.Contains(item.id)) continue;
                Button(item.title + "\n" + item.text, () =>
                {
                    save.draftOrder.Add(captured.id);
                    StoryProgress.Store(campaign, save);
                    ShowBeat();
                });
            }
            if (save.draftOrder.Count > 0) Button("Undo last event", () =>
            {
                save.draftOrder.RemoveAt(save.draftOrder.Count - 1);
                StoryProgress.Store(campaign, save); ShowBeat();
            });
            Button("Submit timeline", () => Decide(StoryFlow.CorrectOrder(beat, save.draftOrder) ? 0 : 1),
                true, save.draftOrder.Count == beat.timelineItems.Length);
        }

        private void Decide(int selected)
        {
            if (save.Find(Beat.id) != null) return;
            save.decisions.Add(new StoryDecision { beatId = Beat.id, choice = selected,
                order = Beat.interaction == "timeline" ? save.draftOrder.ToArray() : null });
            StoryProgress.Store(campaign, save);
            Outcome(Beat, selected);
        }

        private void Outcome(StoryBeat beat, int selected)
        {
            var choice = beat.choices[selected];
            if (beat.interaction == "lead")
            {
                NewPage("Investigation", "Follow the lead.");
                Label(choice.response);
                Button("Open the lead", Next, true);
                return;
            }
            NewPage(beat.assessment ? "Briefing entry locked" : "Radio / response", beat.assessment ? "Recorded for review." : choice.supported ? "The team has your update." : "Command requests a correction.");
            LearningGoal(beat);
            Label("YOUR DECISION", 20, accent);
            Label(choice.text, 28);
            if (beat.assessment)
                Label("Your recommendation is saved. Review continues with fresh evidence; the debrief will explain all three recommendations.", 28, mutedInk);
            else
            {
                Label(choice.response, 30);
                var card = Card();
                Label("WHAT CHANGED", 20, accent, true, card);
                Label(choice.consequence, 26, ink, parent: card);
                var explanation = Card();
                Label("WHY THIS MATTERS", 20, GamePalette.Blue, true, explanation);
                Label(beat.simpleExplanation, 28, ink, parent: explanation);
                Label("TAKE IT WITH YOU", 20, accent, true, explanation);
                Label(beat.lesson, 25, mutedInk, parent: explanation);
                if (!choice.supported)
                {
                    foreach (var option in beat.choices)
                        if (option.supported) Label("CORRECTION TO CARRY FORWARD\n" + option.text, 25, accent);
                }
            }
            Button(beat.assessment ? "Continue briefing" : choice.supported ? "Return to the incident" : "Acknowledge correction & continue", Next, true);
            Button("Save & return to command", Home);
        }

        private void Next()
        {
            bool finishedOpening = save.chapter == 0 && save.beat == Chapter.beats.Length - 1;
            int next = StoryFlow.NextIndex(Chapter, save.beat, save.Find(Beat.id));
            if (next >= 0) save.beat = next;
            else if (save.chapter + 1 < campaign.chapters.Length) { save.chapter++; save.beat = 0; }
            else save.completed = true;
            StoryProgress.Store(campaign, save);
            if (finishedOpening) OpeningDebrief(); else ShowBeat();
        }

        private void OpeningDebrief()
        {
            NewPage("First playtest / chapter complete", "Would you take the next call?");
            Label("You selected a lead, reconstructed a timeline, and briefed the team. Your progress is saved.");
            Label("For this test, notice: did you want to know what happened next? Can you explain why the AI report needed checking?", 26, mutedInk);
            Button("Continue to The Wrong Face", ShowBeat, true);
            Button("Finish this playtest", Home);
        }

        private void Notebook(Action back)
        {
            NewPage("Command archive", "Evidence desk");
            bool any = false;
            foreach (var chapter in campaign.chapters)
            {
                foreach (var beat in chapter.beats)
                {
                    foreach (var evidence in beat.evidence)
                    {
                        if (!save.inspected.Contains(evidence.id)) continue;
                        any = true;
                        var card = Card(); Label(evidence.title, 27, ink, true, card);
                        Label(evidence.source, 21, GamePalette.Blue, parent: card);
                        Label(evidence.text, 25, ink, parent: card);
                    }
                    var decision = save.Find(beat.id);
                    if (decision == null) continue;
                    var entry = Card(GamePalette.ChipSurface);
                    Label("DECISION / " + beat.title, 23, accent, true, entry);
                    Label(beat.choices[decision.choice].text, 25, ink, parent: entry);
                    if (decision.order != null && beat.timelineItems != null)
                        foreach (var id in decision.order)
                        {
                            var item = Array.Find(beat.timelineItems, e => e.id == id);
                            if (item != null) Label(item.title + " / " + item.text, 23, mutedInk, parent: entry);
                        }
                    if (!beat.assessment || save.completed)
                        Label(beat.lesson, 24, mutedInk, parent: entry);
                    else Label("Feedback sealed until case debrief.", 22, mutedInk, parent: entry);
                }
            }
            if (!any) Label("Open source records during the investigation to collect them here.");
            Button("Return", back, true);
        }

        private void Ending()
        {
            int practice = 0, practiceTotal = 0, final = 0;
            foreach (var chapter in campaign.chapters)
                foreach (var beat in chapter.beats)
                {
                    var decision = save.Find(beat.id); if (decision == null || beat.interaction == "lead") continue;
                    bool supported = beat.choices[decision.choice].supported;
                    if (beat.assessment) { if (supported) final++; }
                    else { practiceTotal++; if (supported) practice++; }
                }
            string title = final == 3 ? "A case that holds up." : final == 2 ? "The gaps stay open." : "The briefing is recalled.";
            NewPage("22:06 / Last Train / Case debrief", title);
            Label("Leo is reunited with Nina. The field team confirms contact with the remaining reported civilians. The objects are handled by the appropriate responders. Attribution remains an active investigation.", 29);
            Label(final == 3 ? "Rivera uses your supported briefing to hand the case to the next shift. The report describes the coercive offer, preserves identification gaps, and keeps external actions under review." : final == 2 ? "Priya holds the disputed part of your briefing for correction. The next shift can use the supported portions, but one important recommendation needs further review." : "Priya recalls the briefing before it is distributed. The next shift receives the original evidence and a corrected handover. Your recommendations need supervised review.", 29);
            Label(practice >= practiceTotal * .75f ? "RIVERA: “You gave me facts I could work with. You told me when you didn’t know.”" : "RIVERA: “We had to correct several calls tonight. Keep the originals close. That is how we get better.”", 28, accent);
            Label($"Practice: {practice}/{practiceTotal} supported first decisions\nFinal briefing: {final}/3 supported recommendations", 28, GamePalette.Blue, true);
            Label("These are local learning results, not certification or a measure of policing competence. Replaying practices this same case; it is not a fresh assessment.", 22, mutedInk);
            foreach (var chapter in campaign.chapters)
                foreach (var beat in chapter.beats)
                {
                    if (!beat.assessment) continue;
                    var d = save.Find(beat.id); if (d == null) continue;
                    var card = Card();
                    Label(beat.title, 29, ink, true, card);
                    Label("YOUR CALL / " + beat.choices[d.choice].text, 25, mutedInk, parent: card);
                    Label(beat.choices[d.choice].response, 26, accent, parent: card);
                    Label(beat.simpleExplanation, 26, ink, parent: card);
                    Label(beat.lesson, 24, mutedInk, parent: card);
                }
            Label("TAKE IT OUTSIDE THE GAME", 21, accent, true);
            Label("When an AI gives you a confident claim, open the source and check that exact claim before passing it on. When it proposes an action, check who authorized it.", 29);
            Button("Review the complete evidence desk", () => Notebook(Ending), true);
            Button("Return to command", Home);
        }

        private void Restart()
        {
            NewPage("Restart investigation", "Reopen the case?");
            Label("This clears Last Train’s chapter position, decisions, and inspected evidence on this device. Other game progress is retained.");
            Button("Keep my progress", Home, true);
            Button("Clear this case & begin again", () => { save = StoryProgress.Fresh(campaign); StoryProgress.Store(campaign, save); ShowBeat(); });
        }
    }
}

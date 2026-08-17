using System;
using System.Collections.Generic;
using UnityEngine;

namespace LearnAIGame.Cards
{
    [Serializable]
    public class CardBurstData
    {
        public string topicId;
        public string topicTitle;
        public string cardType;

        // Grounded-incident framing (foundational as of the Deep Trace/ai-garden
        // fusion pass — every topic going forward, including future scope growth
        // like agents/multi-agent, is authored as a felt incident, not a bare
        // card burst) — see PLAN.md §13.2. Mirrors Deep Trace's five-beat spine:
        // Mission Briefing -> Cold Open -> Play -> Reveal -> Lock-In. Deliberately
        // no long-paragraph field here — Deep Trace's own decision log rejected
        // static briefing-heavy text in favor of terse, scannable beats, so every
        // one of these is meant to render as a single short line, not a paragraph.
        public string caseCode;         // "Case 01"
        public string caseTitle;        // short case name, e.g. "Confident and Wrong"
        public string skillLine;        // Mission Briefing: "Tonight's Skill: ..."
        public string missionLine;      // Mission Briefing: one framing sentence, player's role
        public string incidentHeadline; // Cold Open: dramatic alert headline, no teaching copy
        public List<string> stakesLines; // Cold Open: 2-3 short stat/impact callouts
        public string meterLabel;       // live incident-meter label, e.g. "Client Trust"
        public int meterStart = 50;     // 0-100, how much strain the incident starts under
        public string memoryLine;       // Reveal: short sticky takeaway
        public string reflectionPrompt; // Reveal: one reflective question, not a fact restated

        // Fires once, mid-burst, the first time the live meter crosses into its
        // danger zone (>=75) — the case interrupting to react to how the player is
        // actually doing, not a scripted beat that fires regardless of performance.
        public string escalationLine;

        public List<JudgmentCard> cards;
        public JudgmentCard checkpointCard;
        public string feynmanScript;

        public static CardBurstData LoadFromStreamingJson(TextAsset jsonAsset)
        {
            if (jsonAsset == null)
            {
                Debug.LogError("CardBurstData: no JSON TextAsset provided.");
                return null;
            }

            return JsonUtility.FromJson<CardBurstData>(jsonAsset.text);
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace LearnAIGame.Story
{
    [Serializable] public class StoryCampaign
    {
        public string id, title, subtitle, premise;
        public int version;
        public StoryChapter[] chapters;
    }

    [Serializable] public class StoryChapter
    {
        public string id, title, time, location, concept, summary;
        public StoryBeat[] beats;
    }

    [Serializable] public class StoryBeat
    {
        public string id, speaker, title, body, question, concept, lesson;
        public string afterError, afterSuccess;
        public bool assessment;
        public StoryEvidence[] evidence;
        public StoryChoice[] choices;
    }

    [Serializable] public class StoryEvidence
    {
        public string id, title, source, text;
    }

    [Serializable] public class StoryChoice
    {
        public string text, response, consequence;
        public bool supported;
    }

    [Serializable] public class StoryDecision
    {
        public string beatId;
        public int choice;
    }

    [Serializable] public class StorySave
    {
        public string campaignId;
        public int version, chapter, beat;
        public bool completed;
        public List<StoryDecision> decisions = new List<StoryDecision>();
        public List<string> inspected = new List<string>();

        public StoryDecision Find(string id) => decisions.Find(d => d.beatId == id);
    }

    public static class StoryProgress
    {
        private static string Key(StoryCampaign campaign) => "LearnAIGame.Story." + campaign.id;

        public static StorySave Fresh(StoryCampaign campaign) => new StorySave
        { campaignId = campaign.id, version = campaign.version };

        public static StorySave Load(StoryCampaign campaign)
        {
            try
            {
                var raw = PlayerPrefs.GetString(Key(campaign), "");
                var save = string.IsNullOrEmpty(raw) ? null : JsonUtility.FromJson<StorySave>(raw);
                if (save == null || save.campaignId != campaign.id || save.version != campaign.version ||
                    save.chapter < 0 || save.chapter >= campaign.chapters.Length || save.beat < 0 ||
                    save.beat >= campaign.chapters[save.chapter].beats.Length || save.decisions == null || save.inspected == null)
                    return Fresh(campaign);
                foreach (var decision in save.decisions)
                {
                    StoryBeat found = null;
                    foreach (var chapter in campaign.chapters)
                        foreach (var beat in chapter.beats)
                            if (beat.id == decision.beatId) found = beat;
                    if (found == null || found.choices == null || decision.choice < 0 || decision.choice >= found.choices.Length)
                        return Fresh(campaign);
                }
                return save;
            }
            catch (Exception) { return Fresh(campaign); }
        }

        public static void Store(StoryCampaign campaign, StorySave save)
        {
            PlayerPrefs.SetString(Key(campaign), JsonUtility.ToJson(save));
            PlayerPrefs.Save();
        }
    }
}

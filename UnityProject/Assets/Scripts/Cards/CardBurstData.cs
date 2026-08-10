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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace MoodieLand
{
    /// <summary>
    /// Central sprite lookup for cards and props. Keeping it in one asset means
    /// swapping a card picture in one place updates every level.
    /// </summary>
    [DisallowMultipleComponent]
    public class CardLibrary : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            public string id;
            public Sprite sprite;
        }

        public List<Entry> entries = new List<Entry>();

        Dictionary<string, Sprite> map;

        void Awake() => Build();

        void Build()
        {
            map = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
            foreach (var e in entries)
            {
                if (e == null || string.IsNullOrEmpty(e.id)) continue;
                map[e.id] = e.sprite;
            }
        }

        public Sprite Get(string id)
        {
            if (map == null) Build();
            if (string.IsNullOrEmpty(id)) return null;
            return map.TryGetValue(id, out var s) ? s : null;
        }

        public CardData Card(string id, string label)
        {
            return new CardData(id, label, Get(id));
        }
    }
}

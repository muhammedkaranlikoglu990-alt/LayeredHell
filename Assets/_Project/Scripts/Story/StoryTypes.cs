// Plain data types for the story layer (journals, radio, survivors, objective placements).
using System;
using System.Collections.Generic;

namespace LayeredHell
{
    [Serializable]
    public class JournalEntry
    {
        public string id;
        public int floor;
        public string title;
        public string author;
        public string text;

        public JournalEntry(string id, int floor, string title, string author, string text)
        {
            this.id = id; this.floor = floor; this.title = title; this.author = author; this.text = text;
        }
    }

    /// <summary>
    /// A radio transmission. triggerKey is one of: "start" (shortly after a new game begins), "floor_N" (first arrival on floor N,
    /// e.g. "floor_31", "floor_-2"), any objective key from CONTRACT section 3, or "ghost" (random eerie transmissions while playing).
    /// </summary>
    [Serializable]
    public class RadioLog
    {
        public string id;
        public string speaker;
        public string text;
        public string triggerKey;

        public RadioLog(string id, string speaker, string text, string triggerKey)
        {
            this.id = id; this.speaker = speaker; this.text = text; this.triggerKey = triggerKey;
        }
    }

    [Serializable]
    public class SurvivorDef
    {
        public string id;
        public string name;
        public int floor;
        public string role;
        public string profession;
        public string[] introLines;
        public string[] rescuedLines;

        public SurvivorDef(string id, string name, int floor, string role, string profession, string[] introLines, string[] rescuedLines)
        {
            this.id = id; this.name = name; this.floor = floor; this.role = role; this.profession = profession;
            this.introLines = introLines; this.rescuedLines = rescuedLines;
        }
    }

    /// <summary>
    /// Describes one interactive objective prop that FloorChunk places via ObjectiveInteractable.Create.
    /// Extensions over the CONTRACT fields: code, grants, requiresFlags (all optional, safe defaults).
    /// </summary>
    [Serializable]
    public class ObjectivePlacement
    {
        public string key;              // objective key raised on success
        public int floor;
        public string prompt;           // Turkish interaction prompt
        public string anchorHint;       // e.g. "terminal", "safe", "console", "locker", "vehicle", "vent", "desk"
        public ItemStack[] requires;    // items needed in inventory
        public bool consumeItems;       // consume the required items on success
        public string revealsMessage;   // toast / message after success
        public string code;             // optional keypad code ("" = none)
        public ItemStack[] grants;      // optional items given on success
        public string[] requiresFlags;  // objective keys that must already be raised (QuestManager.HasFlag)
        public string modelId;          // optional preferred model id (falls back to placeholder)

        public ObjectivePlacement(string key, int floor, string prompt, string anchorHint, ItemStack[] requires, bool consumeItems,
            string revealsMessage, string code = "", ItemStack[] grants = null, string[] requiresFlags = null, string modelId = "")
        {
            this.key = key; this.floor = floor; this.prompt = prompt; this.anchorHint = anchorHint;
            this.requires = requires ?? new ItemStack[0]; this.consumeItems = consumeItems;
            this.revealsMessage = revealsMessage; this.code = code ?? "";
            this.grants = grants ?? new ItemStack[0]; this.requiresFlags = requiresFlags ?? new string[0];
            this.modelId = modelId ?? "";
        }
    }
}

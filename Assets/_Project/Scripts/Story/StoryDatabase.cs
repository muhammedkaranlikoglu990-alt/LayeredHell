// 45: Layered Hell — StoryDatabase: static facade over 64 Turkish journals, 150 wall texts, 43 radio logs, 6 survivors, and floor objectives.
using System;
using System.Collections.Generic;

namespace LayeredHell
{
    public static class StoryDatabase
    {
        private static readonly List<JournalEntry> _journals = new List<JournalEntry>();
        private static readonly Dictionary<string, JournalEntry> _journalById = new Dictionary<string, JournalEntry>(StringComparer.Ordinal);
        private static readonly Dictionary<int, List<string>> _journalIdsByFloor = new Dictionary<int, List<string>>();
        private static readonly Dictionary<int, List<string>> _wallTextsByFloor = new Dictionary<int, List<string>>();
        private static readonly List<RadioLog> _radioLogs = new List<RadioLog>();
        private static readonly List<SurvivorDef> _survivors = new List<SurvivorDef>();
        private static readonly Dictionary<string, SurvivorDef> _survivorById = new Dictionary<string, SurvivorDef>(StringComparer.Ordinal);
        private static readonly List<ObjectivePlacement> _objectives = new List<ObjectivePlacement>();
        private static bool _initialized;

        public static IReadOnlyList<JournalEntry> Journals
        {
            get
            {
                EnsureInitialized();
                return _journals;
            }
        }

        public static IReadOnlyList<RadioLog> RadioLogs
        {
            get
            {
                EnsureInitialized();
                return _radioLogs;
            }
        }

        public static IReadOnlyList<SurvivorDef> Survivors
        {
            get
            {
                EnsureInitialized();
                return _survivors;
            }
        }

        public static List<string> GetJournalIdsForFloor(int floor)
        {
            EnsureInitialized();
            if (_journalIdsByFloor.TryGetValue(floor, out var ids))
                return new List<string>(ids);
            return new List<string>();
        }

        public static JournalEntry GetJournal(string id)
        {
            EnsureInitialized();
            if (!string.IsNullOrEmpty(id) && _journalById.TryGetValue(id, out var entry))
                return entry;
            return null;
        }

        public static List<string> GetWallTexts(int floor)
        {
            EnsureInitialized();
            if (_wallTextsByFloor.TryGetValue(floor, out var list) && list.Count > 0)
                return new List<string>(list);

            return new List<string>
            {
                "SİSİ SOLUMAYIN — HAVALANDIRMALARI KAPATIN!",
                "38-B GÜVENLİ EV — TELSİZ 88.7 FM",
                "DIŞARIDAN MÜHÜRLENDİK, KİMSE YARDIMA GELMİYOR"
            };
        }

        public static SurvivorDef GetSurvivor(string id)
        {
            EnsureInitialized();
            if (!string.IsNullOrEmpty(id) && _survivorById.TryGetValue(id, out var def))
                return def;
            return null;
        }

        public static List<SurvivorDef> GetSurvivorsOnFloor(int floor)
        {
            EnsureInitialized();
            var result = new List<SurvivorDef>();
            for (int i = 0; i < _survivors.Count; i++)
            {
                if (_survivors[i].floor == floor)
                    result.Add(_survivors[i]);
            }
            return result;
        }

        public static List<ObjectivePlacement> GetObjectivesForFloor(int floor)
        {
            EnsureInitialized();
            var result = new List<ObjectivePlacement>();
            for (int i = 0; i < _objectives.Count; i++)
            {
                if (_objectives[i].floor == floor)
                    result.Add(_objectives[i]);
            }
            return result;
        }

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;

            _journals.Clear();
            _journalById.Clear();
            _journalIdsByFloor.Clear();
            _wallTextsByFloor.Clear();
            _radioLogs.Clear();
            _survivors.Clear();
            _survivorById.Clear();
            _objectives.Clear();

            StoryJournals_Part1.Populate(_journals);
            StoryJournals_Part2.Populate(_journals);

            for (int i = 0; i < _journals.Count; i++)
            {
                var j = _journals[i];
                _journalById[j.id] = j;
                if (!_journalIdsByFloor.TryGetValue(j.floor, out var list))
                {
                    list = new List<string>();
                    _journalIdsByFloor[j.floor] = list;
                }
                list.Add(j.id);
            }

            StoryWallAndRadio.PopulateWallTexts(_wallTextsByFloor);
            StoryWallAndRadio.PopulateRadioLogs(_radioLogs);
            StoryWallAndRadio.PopulateSurvivors(_survivors);

            for (int i = 0; i < _survivors.Count; i++)
            {
                _survivorById[_survivors[i].id] = _survivors[i];
            }

            StoryWallAndRadio.PopulateObjectives(_objectives);
        }
    }
}

using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FMODTrackLookup", menuName = "FMOD/Track Lookup")]
public class FMODTrackLookup : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string ProviderKey;
        public EventReference Event;
    }

    public List<Entry> Entries = new();

    public bool TryGet(string providerKey, out EventReference reference)
    {
        var e = Entries.Find(x => x.ProviderKey == providerKey);
        if (e != null && !e.Event.IsNull)
        {
            reference = e.Event;
            return true;
        }

        reference = default;
        return false;
    }
}

using UnityEngine;

/// <summary>
/// Runtime relay attached to the two scene-authored Hac Am combat zones.
/// It owns no collider and only forwards the Player trigger to the encounter.
/// </summary>
[DisallowMultipleComponent]
public sealed class HacAmCombatZoneRelay : MonoBehaviour
{
    private HacAmEncounterGate encounter;

    public void Initialize(HacAmEncounterGate owner)
    {
        encounter = owner;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && other.CompareTag("Player"))
            encounter?.NotifyPlayerEnteredCombatZone(other);
    }
}

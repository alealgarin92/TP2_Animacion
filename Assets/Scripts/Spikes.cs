using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Amount of damage dealt to the player on each tick (matching standard enemy hit).")]
    public int damageAmount = 10;

    [Tooltip("Time in seconds between damage ticks.")]
    public float damageInterval = 2.0f;

    // Track active damage coroutines for colliding players
    private Dictionary<Damageable, Coroutine> activeCoroutines = new Dictionary<Damageable, Coroutine>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null && player.IsAlive)
        {
            Damageable damageable = player.GetComponent<Damageable>();
            if (damageable != null)
            {
                if (!activeCoroutines.ContainsKey(damageable))
                {
                    Coroutine coroutine = StartCoroutine(DealPeriodicDamage(damageable));
                    activeCoroutines.Add(damageable, coroutine);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            Damageable damageable = player.GetComponent<Damageable>();
            if (damageable != null)
            {
                if (activeCoroutines.ContainsKey(damageable))
                {
                    StopCoroutine(activeCoroutines[damageable]);
                    activeCoroutines.Remove(damageable);
                }
            }
        }
    }

    private IEnumerator DealPeriodicDamage(Damageable damageable)
    {
        // First tick of damage happens immediately on contact
        damageable.Hit(damageAmount);
        Debug.Log("[Spikes] Player entered spikes! Dealt initial damage: " + damageAmount + ". Health: " + damageable.Health);

        while (damageable.IsAlive)
        {
            yield return new WaitForSeconds(damageInterval);

            if (damageable.IsAlive)
            {
                damageable.Hit(damageAmount);
                Debug.Log("[Spikes] Player standing on spikes! Dealt periodic damage: " + damageAmount + ". Health: " + damageable.Health);
            }
        }
    }

    private void OnDisable()
    {
        // Clear all active coroutines when the script is disabled or scene unloaded
        StopAllCoroutines();
        activeCoroutines.Clear();
    }
}

using UnityEngine;

public class HeartReward : MonoBehaviour
{
    [Header("Healing Settings")]
    [Tooltip("Amount of health to restore (each heart UI icon represents 10 HP)")]
    public int healAmount = 10;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object is the player
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            Damageable playerDamageable = collision.GetComponent<Damageable>();
            if (playerDamageable != null && playerDamageable.IsAlive)
            {
                // Heal the player
                playerDamageable.Heal(healAmount);
                
                // Destroy the parent GameObject (Heart_reward) so the entire object disappears
                if (transform.parent != null && (transform.parent.name == "Heart_reward" || transform.parent.name.StartsWith("Heart_reward")))
                {
                    Destroy(transform.parent.gameObject);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}

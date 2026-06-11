using System.Collections.Generic;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;
    public GameObject heartPrefab;

    private List<GameObject> _hearts = new List<GameObject>();
    private float _lastMaxHealth = -1f;
    private float _lastCurrentHealth = -1f;

    private void Start()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }
        
        if (player != null)
        {
            SetupHearts();
        }
    }

    private void Update()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
            if (player == null) return;
        }

        // Re-setup if max health changes (e.g. upgrades)
        if (Mathf.Approximately(player.maxHealth, _lastMaxHealth) == false)
        {
            SetupHearts();
        }

        // Toggle hearts if current health changes
        if (Mathf.Approximately(player.currentHealth, _lastCurrentHealth) == false)
        {
            UpdateHearts(player.currentHealth);
        }
    }

    public void SetupHearts()
    {
        // Destroy old hearts
        foreach (var heart in _hearts)
        {
            if (heart != null)
            {
                Destroy(heart);
            }
        }
        _hearts.Clear();

        _lastMaxHealth = player.maxHealth;
        _lastCurrentHealth = player.currentHealth;

        int totalHearts = Mathf.CeilToInt(player.maxHealth / 10f);
        for (int i = 0; i < totalHearts; i++)
        {
            if (heartPrefab != null)
            {
                GameObject newHeart = Instantiate(heartPrefab, transform);
                _hearts.Add(newHeart);
            }
        }

        UpdateHearts(player.currentHealth);
    }

    private void UpdateHearts(float currentHealth)
    {
        _lastCurrentHealth = currentHealth;
        int activeHeartsCount = Mathf.CeilToInt(currentHealth / 10f);
        
        for (int i = 0; i < _hearts.Count; i++)
        {
            if (_hearts[i] != null)
            {
                _hearts[i].SetActive(i < activeHeartsCount);
            }
        }
    }
}

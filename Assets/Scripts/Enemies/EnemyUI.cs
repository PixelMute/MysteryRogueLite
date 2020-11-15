using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EnemyUI : MonoBehaviour
{
    // Display
    public GameObject enemyCanvasPrefab;
    [HideInInspector] public GameObject enemyCanvas; // The actual object, not the prefab
    protected HealthBarScript healthBar;
    protected FadeObjectScript healthBarFade;
    protected FadeObjectScript exclamationPointFade;
    public float yOffsetForCanvas = 1f;

    /// <summary>
    /// Initialize the enemy's canvas
    /// </summary>
    public void Initialize()
    {
        enemyCanvas = Instantiate(enemyCanvasPrefab, transform);
        enemyCanvas.transform.position += new Vector3(0, yOffsetForCanvas, 0);
        healthBar = enemyCanvas.GetComponentInChildren<HealthBarScript>();

        healthBarFade = healthBar.GetComponentsInParent<FadeObjectScript>(true)[0];
        exclamationPointFade = enemyCanvas.GetComponentsInChildren<FadeObjectScript>(true)[0];
    }

    /// <summary>
    /// Update the health bar with the new values
    /// </summary>
    /// <param name="health">Enemy's current health</param>
    /// <param name="maxHealth">Enemy's max health</param>
    public void UpdateHealthBar(int health, int maxHealth)
    {
        healthBar.UpdateHealthDisplay(health, maxHealth);
    }

    public void UpdateHealthBar(Health health)
    {
        UpdateHealthBar(health.CurrentHealth, health.MaxHealth);
    }

    /// <summary>
    /// Fades the enemy's health bar
    /// </summary>
    public void FadeHealthBar()
    {
        healthBarFade.StartFadeCycle(.3f, 1.2f);
    }

    /// <summary>
    /// Fades the enemy's exclaimation point
    /// </summary>
    public void FadeExclaimationPoint()
    {
        exclamationPointFade.StartFadeCycle(1, 1.25f);
    }

    /// <summary>
    /// Makes the enemy's health bar visible
    /// </summary>
    public void DisplayHealthBar()
    {
        healthBarFade.StopFade(1f);
        enemyCanvas.SetActive(true);
    }

    /// <summary>
    /// Hides the enemy's health bar
    /// </summary>
    public void HideHealthBar()
    {
        HideHealthBar(-1, -1);
    }

    // Set speedMult to <= 0 for instant
    public void HideHealthBar(float buffer, float speedMult)
    {
        if (speedMult > 0)
        {
            healthBarFade.StartFadeCycle(buffer, speedMult);
        }
        else
        {
            healthBarFade.StopFade(0f);
            enemyCanvas.SetActive(false);
        }
    }
}


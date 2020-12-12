using System;
using UnityEngine;

[Serializable]
public class Health : MonoBehaviour
{
    public int CurrentHealth { get; set; }
    public int MaxHealth { get { return maxHealth; } set { maxHealth = value; } }
    [SerializeField]
    private int maxHealth;

    public void Start()
    {
        if (CurrentHealth == 0)
        {
            CurrentHealth = MaxHealth;
        }
    }

    public void TakeDamage(int damage)
    {
        CurrentHealth -= damage;
    }

    public void Heal(int healingAmount)
    {
        CurrentHealth += healingAmount;
        CurrentHealth = Math.Min(CurrentHealth, MaxHealth);
    }

    public bool IsDead { get => CurrentHealth <= 0; }


}


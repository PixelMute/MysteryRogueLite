using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get { return maxHealth; } private set { maxHealth = value; } }
    [SerializeField]
    private int maxHealth;

    public void Start()
    {
        CurrentHealth = MaxHealth;
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


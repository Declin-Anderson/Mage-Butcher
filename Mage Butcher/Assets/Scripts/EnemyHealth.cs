using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField]public int startHealth = 3;
    private int currentHealth;

    void Awake()
    {
        currentHealth = startHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth-=damageAmount;
        if (currentHealth <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}

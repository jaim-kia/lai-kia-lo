using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    public int health;
    public int maxHealth;

    public Sprite heartEmpty;
    public Sprite heartOne;
    public Sprite heartTwo;
    public Sprite heartThree;
    public Sprite heartFull;
    public Image[] hearts;

    public PlayerStats playerStats;
    private float filled;
    private float total;
    private int missingAmount;
    private int partialAmount;
    private bool partialExists;
    private bool partialRendered = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        health = PlayerStats.Instance.returnHealth();
        maxHealth = PlayerStats.Instance.returnMaxHealth();

        filled = Mathf.Floor(health / 4);
        // empty = Mathf.Floor((maxHealth - health) / 4);
        total = Mathf.Ceil(maxHealth / 4);
        missingAmount = ((maxHealth - health) % 4);
        partialExists = missingAmount != 0;
        partialRendered = false;
        

        for(int i = 0; i < (hearts.Length); i++)
        {
            if(i < filled)
            {
                hearts[i].sprite = heartFull;
            }
            else if(partialExists & !partialRendered)
            {
                partialAmount = 4 - missingAmount;
                if (partialAmount == 1)
                    hearts[i].sprite = heartOne;
                else if (partialAmount == 2)
                    hearts[i].sprite = heartTwo;
                else if (partialAmount == 3)
                    hearts[i].sprite = heartThree;
                
                partialRendered = true;
            }
            else
            {
                hearts[i].sprite = heartEmpty;
            }

            if(i < total)
            {
                hearts[i].enabled = true;
            }
            else
            {
                hearts[i].enabled = false;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HealthDisplay : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] GameObject healthbarParent = null;
    [SerializeField] Image healthbarImage = null;

    private void Awake()
    {
        health.ClientOnHealthUpdate += HandleHealthUpdated;
    }

    private void OnDestroy()
    {
        health.ClientOnHealthUpdate -= HandleHealthUpdated;
    }


    private void OnMouseEnter()
    {
        healthbarParent.SetActive(true);
    }

    private void OnMouseExit()
    {
        healthbarParent.SetActive(false);
    }



    private void HandleHealthUpdated(int currentHealth, int maxHealth)
    {
        healthbarImage.fillAmount = 1f * currentHealth / maxHealth;
    }

}

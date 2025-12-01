using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] private float maxHP = 10f;
    [SerializeField] private ParticleSystem deathEffect;
    [SerializeField] private List<GameObject> dropItems;
    public float _maxHP => maxHP;
    private float currentHP;
    public float _currentHP => currentHP;

    private void Start()
    {
        currentHP = maxHP;
    }


    void Update()
    {

        if(currentHP <= 0)
        {
            Dead();
        }
    }

    public void TakeDMG(float damage)
    {
        currentHP -= damage;
    }

    public void Dead()
    {
        SoundManager.Instance.PlaySFX("sfx_destroywood");
        Instantiate(deathEffect,transform.position, transform.rotation);
        foreach(var item in dropItems)
        {
            GameObject drop = Instantiate(item, transform.position, Quaternion.identity);

            Rigidbody rb = drop.GetComponent<Rigidbody>(); if (rb != null)
            {
                // Random outward direction
                Vector3 forceDir = (Random.insideUnitSphere + Vector3.up).normalized;

                // Apply explosion-like force
                rb.AddExplosionForce(100f, transform.position, 2f);

                rb.linearDamping = 3f;
                rb.angularDamping = 8f;
            }


        }

        Destroy(gameObject);
    }

}

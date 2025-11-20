using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int bulletDamage;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Target"))
        {
            print("hit " + collision.gameObject.name + " !");

            CreateBulletImpactEffect(collision);

            Destroy(gameObject);
        }
        if (collision.gameObject.layer == 3)
        {
            print("hit ground!");

            CreateBulletImpactEffect(collision);

            Destroy(gameObject);
        }
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.gameObject.GetComponent<Enemy>().isDead == false)
            {
                collision.gameObject.GetComponent<Enemy>().TakeDamage(bulletDamage);
            }

            print("hit zombie!");

            CreateBloodSprayEffect(collision);

            Destroy(gameObject);
        }
    }

    private void CreateBloodSprayEffect(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];

        GameObject bloodSprayPrefab = Instantiate(
            GlobalReferences.Instance.bloodSprayEffect,
            contact.point,
            Quaternion.LookRotation(contact.normal));

        bloodSprayPrefab.transform.SetParent(collision.gameObject.transform);
    }

    void CreateBulletImpactEffect(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];

        GameObject hole = Instantiate(
            GlobalReferences.Instance.bulletImpactPrefabEffect, 
            contact.point, 
            Quaternion.LookRotation(contact.normal));

        hole.transform.SetParent(collision.gameObject.transform);
    }
}

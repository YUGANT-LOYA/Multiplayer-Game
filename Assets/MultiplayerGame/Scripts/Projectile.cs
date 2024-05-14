using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;

    [Header("Prefabs")] [SerializeField] private Transform _defaultImpact = null;


    private float _damage = 1f;
    private bool _initialized = false;
    private Character _shooter = null;
    private Rigidbody _rigidbody = null;
    private Collider _collider = null;

    private void Awake()
    {
        Initialize();
    }

    void Initialize()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;
        _rigidbody = GetComponent<Rigidbody>();

        if (_rigidbody == null)
        {
            _rigidbody = gameObject.AddComponent<Rigidbody>();
        }

        _rigidbody.useGravity = false;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        _collider = gameObject.GetComponent<Collider>();

        if (_collider == null)
        {
            _collider = gameObject.AddComponent<SphereCollider>();
        }

        _collider.isTrigger = false;
        _collider.tag = "Projectile";
    }


    public void Initialize(Character shooter, Vector3 target, float damage)
    {
        Initialize();

        _shooter = shooter;
        _damage = damage;

        transform.LookAt(target);
        _rigidbody.velocity = transform.forward.normalized * _speed;
        Destroy(gameObject, 5f);
    }


    private void OnCollisionEnter(Collision other)
    {
        if ((_shooter != null && other.transform.root == _shooter.transform.root) ||
            other.gameObject.CompareTag("Projectile"))
        {
            Physics.IgnoreCollision(_collider, other.collider);
            return;
        }

        Character character = other.transform.root.GetComponent<Character>();

        if (character != null)
        {
            //Reduce Player Health
            character.ApplyDamage(_shooter, other.transform, _damage);
        }
        else if (_defaultImpact != null)
        {
            Transform impact = Instantiate(_defaultImpact, other.contacts[0].point,
                Quaternion.FromToRotation(Vector3.up, other.contacts[0].normal));

            Destroy(impact.gameObject, 10f);
        }


        Destroy(gameObject);
    }
}
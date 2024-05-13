using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Item
{
    public enum Handle
    {
        OneHanded = 1,
        TwoHanded = 2
    }

    [Header("Settings")] [SerializeField] private Handle _type = Handle.TwoHanded;
    public Handle type => _type;
    [SerializeField] private float _damage = 1f;
    [SerializeField] private float _fireRate = 0.2f;
    [SerializeField] private int _clipSize = 30;

    [SerializeField] private float _handKick = 5f;
    public float handKick => _handKick;

    [SerializeField] private float _bodyKick = 5f;
    public float bodyKick => _bodyKick;

    [SerializeField] private Vector3 _leftHandPosition = Vector3.zero;
    public Vector3 leftHandPosition => _leftHandPosition;

    [SerializeField] private Vector3 _leftHandRotation = Vector3.zero;
    public Vector3 leftHandRotation => _leftHandRotation;

    [SerializeField] private Vector3 _rightHandPosition = Vector3.zero;
    public Vector3 rightHandPosition => _rightHandPosition;

    [SerializeField] private Vector3 _rightHandRotation = Vector3.zero;
    public Vector3 rightHandRotation => _rightHandRotation;

    private float _fireTimer = 0f;

    [Header("References")] [SerializeField]
    Transform _muzzle = null;

    [SerializeField] private ParticleSystem _flash = null;

    [Header("Prefabs")] [SerializeField] private Projectile _projectile = null;

    private void Awake()
    {
        _fireTimer += Time.realtimeSinceStartup;
    }

    public bool Shoot(Character character, Vector3 target)
    {
        float passedTime = Time.realtimeSinceStartup - _fireTimer;

        if (passedTime >= _fireRate)
        {
            _fireTimer = Time.realtimeSinceStartup;

            //Bullet Logic
            Projectile projectile = Instantiate(_projectile, _muzzle.position, Quaternion.identity);
            projectile.Initialize(character, target, _damage);

            if (_flash != null)
            {
                _flash.Play();
            }

            return true;
        }

        return false;
    }
}
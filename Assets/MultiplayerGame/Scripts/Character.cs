using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using StarterAssets;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Character : NetworkBehaviour
{
    public bool isLocalPlayer;

    [SerializeField] private string _id = "";
    public string id => _id;


    [SerializeField] private Transform _weaponHolder = null;

    private Animator _animator;

    private Weapon _weapon;
    public Weapon weapon => _weapon;

    public Ammo _ammo = null;
    public Ammo ammo => _ammo;

    private List<Item> _items = new List<Item>();
    private RigManager _rigManager = null;
    private Rigidbody[] _ragdollRigidBodies = null;
    private Collider[] _ragdollColliders = null;
    private Weapon _weaponToEquip = null;

    private bool _reloading = false;
    public bool reloading => _reloading;

    private bool _switchingWeapon = false;
    public bool switchingWeapon => _switchingWeapon;

    private float _health = 100f;
    public float health => _health;

    private bool _grounded;
    private bool _walking = false;
    private float _speedAnimationMultiplier = 0;
    private bool _aiming = false;
    private bool _sprinting = false;
    private float _aimLayerWeight = 0f;
    private Vector2 _aimedMovingAnimationInput = Vector2.zero;
    private float aimRigWeight = 0f;
    private float leftHandWeight = 0f;
    private Vector3 _aimTarget;
    private Vector3 _lastPosition = Vector3.zero;

    private ulong _clientID = 0;
    private bool _initialized = false;

    public bool isGrounded
    {
        get => _grounded;
        set => _grounded = value;
    }

    public bool walking
    {
        get => _walking;
        set => _walking = value;
    }

    public float speedAnimationMultiplier
    {
        get => _speedAnimationMultiplier;
        set => _speedAnimationMultiplier = value;
    }

    public bool aiming
    {
        get => _aiming;
        set => _aiming = value;
    }

    public bool sprinting
    {
        get => _sprinting;
        set => _sprinting = value;
    }

    public Vector3 aimTarget
    {
        get => _aimTarget;
        set => _aimTarget = value;
    }

    private void Awake()
    {
        _ragdollColliders = GetComponentsInChildren<Collider>();
        _ragdollRigidBodies = GetComponentsInChildren<Rigidbody>();

        if (_ragdollRigidBodies != null)
        {
            foreach (Rigidbody rb in _ragdollRigidBodies)
            {
                rb.mass *= 50f;
            }
        }

        if (_ragdollColliders != null)
        {
            foreach (Collider col in _ragdollColliders)
            {
                col.isTrigger = false;
            }
        }

        SetRagDollStatus(false);

        _animator = GetComponent<Animator>();
        _rigManager = GetComponent<RigManager>();
    }

    public void InitializeServer(Dictionary<string, int> items, List<string> itemIds, ulong clientId)
    {
        if (_initialized)
            return;

        _initialized = true;
        _clientID = clientId;

        _Initialize(items, itemIds);
    }


    [ClientRpc]
    public void InitializeClientRpc(string itemsJson, string itemIdsJson, ulong clientId)
    {
        if (_initialized)
            return;

        _initialized = true;
        _clientID = clientId;

        if (isLocalPlayer)
        {
            SetLayer(transform, LayerMask.NameToLayer("LocalPlayer"));
        }
        else
        {
            SetLayer(transform, LayerMask.NameToLayer("NetworkPlayer"));
        }

        Dictionary<string, int> items = JsonMapper.ToObject<Dictionary<string, int>>(itemsJson);
        List<string> itemIds = JsonMapper.ToObject<List<string>>(itemIdsJson);

        if (items != null && itemIds != null)
        {
            _Initialize(items, itemIds);
        }
    }

    private void Update()
    {
        bool armed = _weapon != null;

        _aimLayerWeight = Mathf.Lerp(_aimLayerWeight,
            _switchingWeapon || (armed && (_aiming || _reloading)) ? 1f : 0f,
            10f * Time.deltaTime);

        _animator.SetLayerWeight(1, _aimLayerWeight);

        aimRigWeight = Mathf.Lerp(aimRigWeight, armed && (_aiming && !_reloading) ? 1f : 0f,
            10f * Time.deltaTime);

        leftHandWeight = Mathf.Lerp(leftHandWeight,
            armed && !_switchingWeapon && !_reloading &&
            (_aiming || (_grounded && _weapon.type == Weapon.Handle.TwoHanded))
                ? 1f
                : 0f, 10f * Time.deltaTime);

        _rigManager.aimTarget = _aimTarget;
        _rigManager.aimWeight = aimRigWeight;
        _rigManager.leftHandWeight = leftHandWeight;

        if (_sprinting)
        {
            _speedAnimationMultiplier = 3f;
        }
        else if (walking)
        {
            _speedAnimationMultiplier = 1f;
        }
        else
        {
            _speedAnimationMultiplier = 2f;
        }

        Vector3 deltaPos = transform.InverseTransformDirection(transform.position - _lastPosition).normalized;

        _aimedMovingAnimationInput = Vector2.Lerp(_aimedMovingAnimationInput,
            new Vector2(deltaPos.x, deltaPos.z) * _speedAnimationMultiplier, 10f * Time.deltaTime);

        _animator.SetFloat("AimSpeed_X", _aimedMovingAnimationInput.x);
        _animator.SetFloat("AimSpeed_Y", _aimedMovingAnimationInput.y);
        _animator.SetFloat("Armed", armed ? 1f : 0f);
        _animator.SetFloat("Aimed", _aiming ? 1f : 0f);
    }


    private void LateUpdate()
    {
        _lastPosition = transform.position;
    }

    void SetRagDollStatus(bool enabled)
    {
        if (_ragdollRigidBodies != null)
        {
            for (int i = 0; i < _ragdollRigidBodies.Length; i++)
            {
                _ragdollRigidBodies[i].isKinematic = !enabled;
            }
        }
    }

    private void _Initialize(Dictionary<string, int> items, List<string> itemIds)
    {
        if (_items != null && PrefabManager.singleton != null)
        {
            int firstWeaponIndex = -1;
            int i = 0;

            foreach (var itemData in items)
            {
                Item itemPrefab = PrefabManager.singleton.GetItemPrefab(itemData.Key);

                if (itemPrefab != null && itemData.Value > 0)
                {
                    Item item = Instantiate(itemPrefab, transform);
                    item.networkId = itemIds[i];
                    if (item.GetType() == typeof(Weapon))
                    {
                        Weapon weapon = (Weapon)item;

                        weapon.transform.SetParent(_weaponHolder);
                        weapon.transform.localPosition = weapon.rightHandPosition;
                        weapon.transform.localEulerAngles = weapon.rightHandRotation;

                        if (firstWeaponIndex < 0)
                        {
                            firstWeaponIndex = _items.Count;
                        }
                    }
                    else if (item.GetType() == typeof(Ammo))
                    {
                        Ammo ammoItem = (Ammo)item;
                        ammoItem.amount = itemData.Value;
                    }

                    item.gameObject.SetActive(false);
                    _items.Add(item);
                }

                i++;
            }

            if (firstWeaponIndex >= 0 && _weapon == null)
            {
                _weaponToEquip = (Weapon)_items[firstWeaponIndex];
                OnEquip();
            }
        }
    }

    public void ChangeWeapon(float direction)
    {
        //Debug.Log("Change Weapon Called !");
        int x = direction > 0 ? 1 : direction < 0 ? -1 : 0;

        if (x != 0 && !_switchingWeapon)
        {
            if (x > 0)
            {
                NextWeapon();
            }
            else
            {
                PrevWeapon();
            }
        }
    }

    void NextWeapon()
    {
        int first = -1;
        int currWeapon = -1;

        for (int i = 0; i < _items.Count; i++)
        {
            Item item = _items[i];

            if (item != null && item.GetType() == typeof(Weapon))
            {
                if (_weapon != null && item.gameObject == _weapon.gameObject)
                {
                    currWeapon = i;
                }
                else
                {
                    if (currWeapon >= 0)
                    {
                        EquipWeapon((Weapon)item);
                        return;
                    }
                    else if (first < 0)
                    {
                        first = i;
                    }
                }
            }
        }

        if (first >= 0)
        {
            EquipWeapon((Weapon)_items[first]);
        }
    }

    void PrevWeapon()
    {
        int last = -1;
        int currWeapon = -1;

        for (int i = _items.Count - 1; i >= 0; i--)
        {
            Item item = _items[i];

            if (item != null && item.GetType() == typeof(Weapon))
            {
                if (_weapon != null && item.gameObject == _weapon.gameObject)
                {
                    currWeapon = i;
                }
                else
                {
                    if (currWeapon >= 0)
                    {
                        EquipWeapon((Weapon)item);
                        return;
                    }
                    else if (last < 0)
                    {
                        last = i;
                    }
                }
            }
        }

        if (last >= 0)
        {
            EquipWeapon((Weapon)_items[last]);
        }
    }

    public void EquipWeapon(Weapon weapon)
    {
        if (_switchingWeapon || weapon == null)
        {
            return;
        }

        _weaponToEquip = weapon;

        if (_weapon != null)
        {
            HolsterWeapon();
        }
        else
        {
            _switchingWeapon = true;
            _animator.SetTrigger("Equip");
        }
    }

    private void _EquipWeapon()
    {
        if (_weaponToEquip != null)
        {
            _weapon = _weaponToEquip;
            _weaponToEquip = null;

            if (_weapon.transform.parent != _weaponHolder)
            {
                _weapon.transform.SetParent(_weaponHolder);
                _weapon.transform.localPosition = _weapon.rightHandPosition;
                _weapon.transform.localEulerAngles = _weapon.rightHandRotation;
            }

            _rigManager.SetLeftHandGripData(_weapon.leftHandPosition, _weapon.leftHandRotation);
            _weapon.gameObject.SetActive(true);

            _ammo = null;

            foreach (Item item in _items)
            {
                if (item != null && item.GetType() == typeof(Ammo) && _weapon.ammoId == item.id)
                {
                    _ammo = (Ammo)item;
                    break;
                }
            }
        }
    }

    //Switching Weapon Animator Event Func.
    public void OnEquip()
    {
        _EquipWeapon();
    }

    public void HolsterWeapon()
    {
        if (_switchingWeapon)
        {
            return;
        }

        if (_weapon != null)
        {
            _switchingWeapon = true;
            _animator.SetTrigger("Holster");
        }
    }

    private void _HolsterWeapon()
    {
        if (_weapon != null)
        {
            _weapon.gameObject.SetActive(false);
            _weapon = null;
            _ammo = null;
        }
    }

    //Switching Weapon Animator Event Func.
    public void OnHolster()
    {
        _HolsterWeapon();

        if (_weaponToEquip != null)
        {
            OnEquip();
        }
    }

    public void Reload()
    {
        if (_weapon != null && !reloading && _weapon.ammo < _weapon.clipSize && _ammo != null && _ammo.amount > 0)
        {
            _animator.SetTrigger("Reload");
            _reloading = true;
        }
    }

    public void ReloadFinished()
    {
        if (_weapon != null && _weapon.ammo < _weapon.clipSize && _ammo != null && _ammo.amount > 0)
        {
            int amount = _weapon.clipSize - weapon.ammo;

            if (_ammo.amount < amount)
            {
                amount = _ammo.amount;
            }

            _ammo.amount -= amount;
            _weapon.ammo += amount;
        }

        _reloading = false;
    }

    public void HolsterFinished()
    {
        _switchingWeapon = false;
    }

    public void EquipFinished()
    {
        _switchingWeapon = false;
    }

    public void ApplyDamage(Character shooter, Transform hit, float damage)
    {
        if (_health > 0f)
        {
            _health -= damage;

            if (_health <= 0f)
            {
                _health = 0f;
                SetRagDollStatus(true);
                Destroy(_rigManager);
                Destroy(GetComponent<RigBuilder>());
                Destroy(_animator);

                ThirdPersonController thirdPersonController = GetComponent<ThirdPersonController>();

                if (thirdPersonController != null)
                {
                    Destroy(thirdPersonController);
                }

                CharacterController characterController = GetComponent<CharacterController>();

                if (characterController != null)
                {
                    Destroy(characterController);
                }

                Destroy(this);
            }
        }
    }

    public void SetLayer(Transform root, int layer)
    {
        Transform[] children = root.GetComponentsInChildren<Transform>();

        foreach (Transform child in children)
        {
            child.gameObject.layer = layer;
        }
    }
}
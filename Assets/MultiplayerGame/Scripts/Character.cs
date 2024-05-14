using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private Transform _weaponHolder = null;

    private Animator _animator;

    private Weapon _weapon;
    public Weapon weapon => _weapon;

    public Ammo _ammo = null;
    public Ammo ammo => _ammo;

    private List<Item> _items = new List<Item>();
    private RigManager _rigManager = null;

    private Weapon _weaponToEquip = null;

    private bool _reloading = false;
    public bool reloading => _reloading;

    private bool _switchingWeapon = false;
    public bool switchingWeapon => _switchingWeapon;


    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigManager = GetComponent<RigManager>();
        Initialize(new Dictionary<string, int>
            { { "Rifle", 1 }, { "RifleBullet", 1000 }, { "Pistol", 1 }, { "PistolBullet", 1000 } });
        //Initialize(new Dictionary<string, int> { { "Rifle", 1 }, { "Bullet", 1000 } });
        //Initialize(new Dictionary<string, int> { { "Pistol", 1 }, { "PistolBullet", 1000 } });
    }

    public void Initialize(Dictionary<string, int> items)
    {
        if (_items != null && PrefabManager.singleton != null)
        {
            int firstWeaponIndex = -1;

            foreach (var itemData in items)
            {
                Item itemPrefab = PrefabManager.singleton.GetItemPrefab(itemData.Key);

                if (itemPrefab != null && itemData.Value > 0)
                {
                    for (int i = 1; i <= itemData.Value; i++)
                    {
                        Item item = Instantiate(itemPrefab, transform);
                        bool done = false;

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
                            done = true;
                        }

                        item.gameObject.SetActive(false);
                        _items.Add(item);

                        if (done)
                        {
                            break;
                        }
                    }
                }
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
        
        //Dir > 0, Val = 1
        //Dir < 0, Val = -1
        //Dir = 0, Val = 0
        int x = direction > 0 ? 1 : direction < 0 ? -1 : 0;
        
        if (x != 0 && !_switchingWeapon)
        {
            int prevWeapon = -1;
            int currentWeapon = -1;
            int nextWeapon = -1;

            for (int i = 0; i < _items.Count; i++)
            {
                Item item = _items[i];
                if (item != null && item.GetType() == typeof(Weapon))
                {
                    if (item.gameObject == _weapon.gameObject)
                    {
                        currentWeapon = i;
                    }
                    else
                    {
                        if (currentWeapon < 0 && prevWeapon < 0)
                        {
                            prevWeapon = i;
                        }

                        if (currentWeapon >= 0 && nextWeapon < 0)
                        {
                            nextWeapon = i;
                        }
                    }
                }
            }

            int targetWeapon = -1;

            if (x > 0)
            {
                if (nextWeapon >= 0)
                {
                    targetWeapon = nextWeapon;
                }
                else if (prevWeapon >= 0)
                {
                    targetWeapon = prevWeapon;
                }
            }
            else
            {
                if (prevWeapon >= 0)
                {
                    targetWeapon = prevWeapon;
                }
                else if (nextWeapon >= 0)
                {
                    targetWeapon = nextWeapon;
                }
            }

            if (targetWeapon >= 0)
            {       
                //Debug.Log("Target Weapon Equipping !");
                EquipWeapon((Weapon)_items[targetWeapon]);
            }
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
    }
}
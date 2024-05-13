using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private Transform _weaponHolder = null;
    private Weapon _weapon;

    public Weapon weapon => _weapon;

    private List<Item> _items = new List<Item>();
    private RigManager _rigManager = null;

    private void Awake()
    {
        _rigManager = GetComponent<RigManager>();
        Initialize(new Dictionary<string, int> { { "Rifle", 1 } });
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
                            done = true;
                            Ammo ammo = (Ammo)item;
                            ammo.amount = itemData.Value;
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
                EquipWeapon((Weapon)_items[firstWeaponIndex]);
            }
        }
    }

    public void EquipWeapon(Weapon weapon)
    {
        if (_weapon != null)
        {
            HolsterWeapon();
        }

        if (weapon != null)
        {
            if (weapon.transform.parent != _weaponHolder)
            {
                weapon.transform.SetParent(_weaponHolder);
                weapon.transform.localPosition = weapon.rightHandPosition;
                weapon.transform.localEulerAngles = weapon.rightHandRotation;
            }

            _rigManager.SetLeftHandGripData(weapon.leftHandPosition, weapon.leftHandRotation);
            weapon.gameObject.SetActive(true);
            _weapon = weapon;
        }
    }

    public void HolsterWeapon()
    {
        if (_weapon != null)
        {
            _weapon.gameObject.SetActive(false);
            _weapon = null;
        }
    }

    public void ApplyDamage(Character shooter,Transform hit,float damage)
    {
        
    }
}
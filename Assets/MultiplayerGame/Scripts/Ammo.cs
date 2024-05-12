using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : Item
{
    private int _amount = 0;
    public int amount
    {
        get => _amount;
        set => _amount = value;
    }
    
}

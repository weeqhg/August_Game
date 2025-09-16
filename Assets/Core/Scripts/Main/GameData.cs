using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    public float currentHealth;
    public int countKey;
    public string weaponConfigId;
    public List<string> accessoryConfigIds = new List<string>();
}

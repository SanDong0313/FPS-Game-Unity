using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KillInfo
{
    public string Killer;
    public string Killed;
    public string KillMethod;
    public bool byHeadShot = false;
}
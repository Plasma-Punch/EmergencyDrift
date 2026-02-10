using System;
using UnityEngine;

public class SpawnPatientEventArgs : EventArgs
{
    public GameObject SpawnLocation;
    public int BleedOutRate;
}

using System;
using System.Collections;
using System.Collections.Generic;
using Oculus.Interaction;
using UnityEngine;

public class CoLocationSynchronizer : MonoBehaviour
{
  public NetworkPlayerSpawner networkPlayerSpawner;
 
  public void Synchronize()
  {
    Debug.Log(networkPlayerSpawner.GetSpawnedPlayerPrefab().transform.position);
  }
  
}

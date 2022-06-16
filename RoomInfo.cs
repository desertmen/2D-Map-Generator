using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Roominfo")]
public class RoomInfo : ScriptableObject
{
    [System.Serializable]
    public struct EnemyToSpawn
    {
        public GameObject enemy;
        public int count;
    }

    public EnemyToSpawn[] enemiesToSpawn;
}

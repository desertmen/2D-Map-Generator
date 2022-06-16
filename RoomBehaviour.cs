using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBehaviour : MonoBehaviour
{
    GateScript[] gates;
    List<GameObject> enemies;
    
    [HideInInspector]
    public MapCreator.Room room;
    [HideInInspector]
    public MapCreator mapCreator;

    bool isCleaned = false;
    bool enemiesSpawned = false;
    bool gatesClosed = false;
    Transform playerTransform;
    private void Update()
    {
        if(isCleaned == true || enemiesSpawned == false)
        {
            return;
        }
        bool enemiesAlive = false;
        if(gatesClosed == false)
        {
            if (isInside(playerTransform.position))
            {
                closeGates();
                spawnEnemies();
                gatesClosed = true;
            }
            else
            {
                return;
            }
        }
        foreach(GameObject enemy in enemies)
        {
            if(enemy != null)
            {
                enemiesAlive = true;
            }
        }
        if (enemiesAlive == false)
        {
            isCleaned = true;
            openGates();
        }
    }

    private void Start()
    {
        List<GateScript> gatesList = new List<GateScript>();
        for(int i = 0; i < transform.childCount; i++)
        {
            if(transform.GetChild(i).GetComponent<GateScript>() != null)
            {
                gatesList.Add(transform.GetChild(i).GetComponent<GateScript>());
            }
        }
        gates = gatesList.ToArray();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isCleaned || enemiesSpawned || enabled == false)
        {
            return;
        }
        playerTransform = collision.gameObject.transform;
        enemiesSpawned = true;
    }

    private void spawnEnemies()
    {
        RoomInfo roomInfo = room.RoomInfo;
        enemies = new List<GameObject>();
        Vector2 roomSize = room.Size * mapCreator.scale;
        Vector3 pos = transform.position - new Vector3(roomSize.x, roomSize.y, 0) / 2f;
        foreach (RoomInfo.EnemyToSpawn enemyToSpawn in roomInfo.enemiesToSpawn)
        {
            for (int i = 0; i < enemyToSpawn.count; i++)
            {
                Vector3 spawnPos = pos;
                Collider2D enemyCollider = enemyToSpawn.enemy.GetComponent<Collider2D>();
                Vector3 colliderSize = enemyToSpawn.enemy.transform.localScale.x * enemyCollider.bounds.extents;
                spawnPos.x += Random.Range(0 + colliderSize.x, roomSize.x - colliderSize.x);
                spawnPos.y += Random.Range(0 + colliderSize.y, roomSize.y - colliderSize.y);
                enemies.Add(Instantiate(enemyToSpawn.enemy, spawnPos, Quaternion.identity));
            }
        }
        enemiesSpawned = true;
    }

    private void closeGates()
    {
        for (int i = 0; i < gates.Length; i++)
        {
            gates[i].close();
        }
    }

    private void openGates()
    {
        for (int i = 0; i < gates.Length; i++)
        {
            gates[i].open();
        }
    }

    private bool isInside(Vector3 playerPos)
    {
        float scale = mapCreator.scale;
        Vector3 playerToCenterDist = transform.position - playerPos;
        if(Mathf.Abs(playerToCenterDist.x) < Mathf.Abs(room.Size.x)/2f * scale - scale && Mathf.Abs(playerToCenterDist.y) < Mathf.Abs(room.Size.y) * scale / 2f - scale)
        {
            return true;
        }
        return false;
    }
}

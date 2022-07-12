using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomBehaviour : MonoBehaviour
{
    GateScript[] gates;
    List<GameObject> enemies;
    
    [HideInInspector]
    public Room room;
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
        RoomInfo roomInfo = room.roomInfo;
        enemies = new List<GameObject>();
        Vector2 roomSize = (Vector2)room.size * mapCreator.scale;
        Vector3 pos = transform.position;
        foreach (RoomInfo.EnemyToSpawn enemyToSpawn in roomInfo.enemiesToSpawn)
        {
            for (int i = 0; i < enemyToSpawn.count; i++)
            {
                Vector3 spawnPos = pos;
                SpriteRenderer enemySprite = enemyToSpawn.enemy.GetComponent<SpriteRenderer>();
                if(enemySprite == null)
                {
                    enemySprite = searchChildren<SpriteRenderer>(enemyToSpawn.enemy.transform)[0];
                }
                Vector3 colliderSize = enemySprite.bounds.extents;
                spawnPos.x += Random.Range(-roomSize.x/2f + colliderSize.x, roomSize.x/2f - colliderSize.x);
                spawnPos.y += Random.Range(-roomSize.y/2f + colliderSize.y, roomSize.y/2f- colliderSize.y);
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
        if(Mathf.Abs(playerToCenterDist.x) < room.size.x/2f * scale - scale && Mathf.Abs(playerToCenterDist.y) < room.size.y * scale / 2f - scale)
        {
            return true;
        }
        return false;
    }

    private List<T> searchChildren<T>(Transform parent)
    {
        List<T> list = new List<T>();
        searchChildren<T>(parent, list);
        return list;
    }

    private void searchChildren<T>(Transform parent, List<T> list)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            var component = parent.GetChild(i).GetComponent<T>();
            if (component != null)
            {
                if (component.ToString().Equals("null") == false)
                {
                    list.Add(component);
                }
            }
            searchChildren(parent.GetChild(i), list);
        }
    }
}

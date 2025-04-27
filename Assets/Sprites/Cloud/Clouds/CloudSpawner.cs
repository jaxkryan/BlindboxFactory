using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CloudSpawner : MonoBehaviour
{
    public Transform cloudParent;
    public Tilemap targetTilemap;
    public Transform verticalSpawnMarker;     // Sprite hoặc cột chỉ điểm vị trí spawn
    public float spawnInterval = 5f;
    public float spawnChance = 0.5f;
    public float speed = 1f;
    public bool spawnRight = true;

    private List<GameObject> cloudList = new List<GameObject>();
    private Camera mainCamera;
    private float yMin;
    private float yMax;
    private float mapWidthWorld = 20f;

    private void Start()
    {
        mainCamera = Camera.main;
        UpdateSpawnHeight();
        GetCloudsFromParent();
        CalculateMapWidth();
        InvokeRepeating(nameof(SpawnCloud), 0f, spawnInterval);
    }

    void GetCloudsFromParent()
    {
        cloudList.Clear();
        foreach (Transform child in cloudParent)
        {
            child.gameObject.SetActive(false);
            cloudList.Add(child.gameObject);
        }
    }

    void CalculateMapWidth()
    {
        if (targetTilemap != null)
        {
            var bounds = targetTilemap.cellBounds;
            Vector3 startWorld = targetTilemap.CellToWorld(new Vector3Int(bounds.xMin, 0, 0));
            Vector3 endWorld = targetTilemap.CellToWorld(new Vector3Int(bounds.xMax, 0, 0));
            mapWidthWorld = Mathf.Abs(endWorld.x - startWorld.x);

            // Gán lại vị trí của thanh dọc nếu có
            if (verticalSpawnMarker != null)
            {
                Vector3 spawnX = spawnRight ? startWorld : endWorld;
                verticalSpawnMarker.position = new Vector3(spawnX.x, verticalSpawnMarker.position.y, verticalSpawnMarker.position.z);
                // Optional: đặt chính cả spawner object theo marker
                transform.position = verticalSpawnMarker.position;
            }
        }
    }

    void UpdateSpawnHeight()
    {
        if (targetTilemap != null)
        {
            var bounds = targetTilemap.cellBounds;

            Vector3 bottomWorld = targetTilemap.CellToWorld(new Vector3Int(0, bounds.yMin, 0));
            Vector3 topWorld = targetTilemap.CellToWorld(new Vector3Int(0, bounds.yMax, 0));
            
            yMin = bottomWorld.y;
            yMax = topWorld.y;
        }
        
    }


    void SpawnCloud()
    {
        UpdateSpawnHeight();

        if (Random.value <= spawnChance)
        {
            
            GameObject cloud = GetInactiveCloud();
            if (cloud != null)
            {
                
                float yPos = Random.Range(yMin, yMax);
                Vector3 spawnPos = new Vector3(transform.position.x, yPos, 0f);

                cloud.transform.position = spawnPos;
                cloud.SetActive(true);
                if(cloud.active == true)
                {
                    //Debug.Log(spawnChance + "spawned");
                }
                CloudMover mover = cloud.GetComponent<CloudMover>();
                if (mover == null)
                    mover = cloud.AddComponent<CloudMover>();

                mover.speed = speed * (spawnRight ? 1 : -1);
                mover.travelDistance = mapWidthWorld;
            }
        }
    }

    GameObject GetInactiveCloud()
    {
        foreach (var cloud in cloudList)
        {
            if (!cloud.activeInHierarchy)
                return cloud;
        }
        return null;
    }
}

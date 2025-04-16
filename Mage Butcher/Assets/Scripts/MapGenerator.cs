using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] public GameObject[] roomPrefabs; // Array of different room prefabs
    [SerializeField] public GameObject startingRoom; // The first room to spawn
    [SerializeField] public GameObject corridorPrefab; // The corridors between rooms
    [SerializeField] public int maxRooms = 10; // Limit the number of rooms

    [SerializeField] private List<Transform> availableDoorways = new List<Transform>();
    [SerializeField] private List<GameObject> spawnedRooms = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateMap();

    }

    void Update()
    {
        foreach (Transform doorway in availableDoorways)
        {
            Debug.DrawRay(doorway.position + doorway.forward * 0.5f, doorway.forward * 10f, Color.yellow); // Always shows the ray
        }
    }

    void GenerateMap()
    {
        // Spawn the starting room
        GameObject startRoom = Instantiate(startingRoom, Vector3.zero, Quaternion.identity);
        Room startRoomScript = startRoom.GetComponent<Room>();
        spawnedRooms.Add(startRoom);
        availableDoorways.AddRange(startRoomScript.doorways);

        int attempts = 0;
        while (spawnedRooms.Count < maxRooms && attempts < 100)
        {
            attempts++;
            Debug.Log(availableDoorways.Count);
            // Pick random spawn point
            if (availableDoorways.Count == 0)
            {
                Debug.LogWarning("No more doorways available!");
                return;
            }
            // 1. Pick a random spawn point from existing doorways
            Transform spawnPoint = GetRandomDoorway();
            spawnPoint.parent.GetComponent<Room>().doorways.Remove(spawnPoint);
            availableDoorways.Remove(spawnPoint);
            

            // 2. Pick a random room prefab and instantiate
            GameObject newRoom = Instantiate(roomPrefabs[Random.Range(0, roomPrefabs.Length)]);
            Room newRoomScript = newRoom.GetComponent<Room>();

            // 3. Pick a doorway on the new room
            Transform newDoorway = newRoomScript.doorways[Random.Range(0, newRoomScript.doorways.Count)];

            // 4. Rotate the new room so its doorway faces opposite the spawn point
            Quaternion from = newDoorway.rotation;
            Quaternion to = Quaternion.LookRotation(-spawnPoint.forward, Vector3.up);
            Quaternion rotationDelta = to * Quaternion.Inverse(from);

            // APPLY rotation to new room
            newRoom.transform.rotation = rotationDelta * newRoom.transform.rotation;

            // 5. AFTER ROTATING, calculate new doorway's world offset
            Vector3 offset = newDoorway.position - newRoom.transform.position;

            // 6. Apply position to line up the doorways
            newRoom.transform.position = spawnPoint.position - offset;

            // Check for collision
            Collider[] overlaps = Physics.OverlapBox(newRoom.transform.position, Vector3.one * 25); // half room size
            if (overlaps.Length > 0)
            {
                Debug.Log("Destoryed the new room");
                Destroy(newRoom);
                continue;
            }

            // Success — add new room
            spawnedRooms.Add(newRoom);

            // Remove used doorway and add remaining new doorways
            newRoomScript.doorways.Remove(newDoorway);

            // ✅ NEW: Remove or plug any doorways that are blocked
            List<Transform> validDoorways = new List<Transform>();
            foreach (Transform doorway in newRoomScript.doorways)
            {
                if (IsDoorwayBlocked(doorway))
                {
                    // Option 1: Remove
                    Debug.Log("Hit One");
                    availableDoorways.Remove(doorway);
                }
                else
                {
                    validDoorways.Add(doorway);
                }
            }
            availableDoorways.AddRange(validDoorways);

            // Check existing doorways for blockages caused by this new room
            List<Transform> stillValid = new List<Transform>();
            foreach (Transform existingDoorway in availableDoorways)
            {
                if (!IsDoorwayBlocked(existingDoorway))
                {
                    stillValid.Add(existingDoorway);
                }
                else
                {
                    // Optionally spawn a wall cap
                    Debug.Log("Hit a previous");
                    Destroy(existingDoorway.gameObject);
                }
            }
            availableDoorways = stillValid;
            
        }

        foreach (Transform doorway in availableDoorways)
        {
            doorway.gameObject.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    /// <summary>
    /// Gets a random doorway from a room to spawn the next room from
    /// </summary>
    /// <returns> Returns the doorway that was randomly selected </returns>
    Transform GetRandomDoorway()
    {
        return availableDoorways[Random.Range(0, availableDoorways.Count)];
    }

    /// <summary>
    /// Checks whether a doorway for the room is currently blocked
    /// </summary>
    /// <param name="doorway"></param>
    /// <returns></returns>
    bool IsDoorwayBlocked(Transform doorway)
    {
        Collider doorwayCollider = doorway.GetComponent<SphereCollider>();

        if (doorwayCollider == null)
        {
            Debug.LogWarning("Doorway has no collider attached: " + doorway.name);
            return true; // Assume blocked to be safe
        }

        Bounds bounds = doorwayCollider.bounds;
        Collider[] overlaps = Physics.OverlapSphere(doorway.position, 2f);
        if(overlaps.Length > 0)
        {
            Debug.Log("We are hitting stuff");
            Debug.Log(doorway.gameObject.name);
            for(int i = 0; i <overlaps.Length; i++)
            {
                Debug.Log(overlaps[i].name);
            }
        }

        foreach (Collider col in overlaps)
        {
            if (col.CompareTag("Doorway") || col.CompareTag("Wall") && col.transform != doorway)
            {
                return true;
            }
        }
        return false;
    }
}

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Generates the Map that player will walk through
/// </summary>
public class MapGenerator : MonoBehaviour
{
    [SerializeField] public GameObject[] roomPrefabs; // Array of different room prefabs
    [SerializeField] public GameObject startingRoom; // The first room to spawn
    [SerializeField] public GameObject corridorPrefab; // The corridors between rooms
    [SerializeField] public int maxRooms = 10; // Limit the number of rooms

    [SerializeField] private List<Transform> availableDoorways = new List<Transform>(); // The Current available doorways that a room could spawn from
    [SerializeField] private List<GameObject> spawnedRooms = new List<GameObject>(); // All of the rooms that have currently spawned in the dungeon

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateMap();

    }

    /// <summary>
    /// Generates the map by starting with the spawn point then creating rooms attaching to random open doorways in the maze
    /// </summary>
    void GenerateMap()
    {
        // Spawn the starting room
        int roomCount = 0;
        GameObject startRoom = Instantiate(startingRoom, Vector3.zero, Quaternion.identity);
        Room startRoomScript = startRoom.GetComponent<Room>();
        // Naming the room the starting room
        startRoom.name = "Room " + roomCount++ + " (Start)";
        spawnedRooms.Add(startRoom);
        // Adding the number of doors that the start room will have open
        availableDoorways.AddRange(startRoomScript.doorways);

        Debug.Log($"Initial doors to process: {availableDoorways.Count}"); // How many doors are available to use

        while (spawnedRooms.Count < maxRooms)
        {
            Debug.Log($"Attempting to spawn room {spawnedRooms.Count + 1} of {maxRooms}"); // Current room attempting to be spawned in console

            // Checking to see if there are no doors left to use and if so breaking the loop
            if (availableDoorways.Count == 0)
            {
                Debug.LogWarning("No more doorways available!");
                return;
            }
            // 1. Pick a random spawn point from existing doorways
            Transform spawnPoint = GetRandomDoorway();
            spawnPoint.parent.GetComponent<Room>().doorways.Remove(spawnPoint);

            // Removing the door chosen from the available pool
            availableDoorways.Remove(spawnPoint);
            Debug.Log($"Removed door: {spawnPoint.transform.parent.name}.{spawnPoint.name}"); // Naming the Door removed in the console
            

            // 2. Pick a random room prefab and instantiate
            GameObject tempRoom = Instantiate(roomPrefabs[Random.Range(0, roomPrefabs.Length - 1)]);
            Room tempRoomScript = tempRoom.GetComponent<Room>();

            // 3. Pick a doorway on the new room
            int randomDoor = Random.Range(0, tempRoomScript.doorways.Count - 1);
            Transform tempDoorway = tempRoomScript.doorways[randomDoor];

            // 4. Rotate the new room so its doorway faces opposite the spawn point
            Quaternion from = tempDoorway.rotation;
            Quaternion to = Quaternion.LookRotation(-spawnPoint.forward, Vector3.up);
            Quaternion rotationDelta = to * Quaternion.Inverse(from);

            // APPLY rotation to new room
            tempRoom.transform.rotation = rotationDelta * tempRoom.transform.rotation;

            // 5. AFTER ROTATING, calculate new doorway's world offset
            Vector3 offset = tempDoorway.position - tempRoom.transform.position;

           // 6. Apply position to line up the doorways
            tempRoom.transform.position = spawnPoint.position - offset;

            // Before placing the final room, calculate the position for the corridor
           /* Vector3 doorwayA = spawnPoint.position;
            Vector3 doorwayB = tempDoorway.position - offset + spawnPoint.position; // adjusted to new position
            Vector3 corridorPosition = Vector3.Lerp(doorwayA, doorwayB, 0.5f);
            Vector3 corridorDirection = (doorwayB - doorwayA).normalized;
            Quaternion corridorRotation = Quaternion.LookRotation(corridorDirection, Vector3.up);

            // Optional: Adjust based on your corridor prefab length
            float corridorLength = Vector3.Distance(doorwayA, doorwayB);

            // Instantiate corridor
            GameObject corridor = Instantiate(corridorPrefab, corridorPosition, corridorRotation);
            //corridor.transform.localScale = new Vector3(corridor.transform.localScale.x, corridor.transform.localScale.y, corridorLength);*/

            // Check for collision
            Collider[] overlaps = Physics.OverlapBox(tempRoom.transform.position, Vector3.one * 25); // half room size
            if (overlaps.Length > 0)
            {
                // Overlap was found so it destroys the room and goes to the next attempt in the loop
                Debug.Log("Destroyed the new room");
                Destroy(tempRoom);
                continue;
            }

            // 7. Building a room in the location of where tempRoom goes to so the physics can look for overlap
            GameObject newRoom = Instantiate(tempRoom, tempRoom.transform.position, rotationDelta);
            newRoom.name = "Room " + roomCount++;
            Room newRoomScript = newRoom.GetComponent<Room>();
            Transform newDoorway = newRoomScript.doorways[randomDoor];
            Debug.Log($"Placed Room Location Spawned: {newRoom.transform.position}"); //Checking the position instantiated to make sure its the right spot

            // Destroy the temproom because its no longer needed
            Destroy(tempRoom);

            // Success — add new room
            spawnedRooms.Add(newRoom);

            // Remove used doorway and add remaining new doorways
            newRoomScript.doorways.Remove(newDoorway);
            availableDoorways.AddRange(newRoomScript.doorways);

            Debug.Log($"Available doors to process: {availableDoorways.Count}"); // Number of doors that still open

        }

        // Closing all the doors still available so that the rooms are encapsulated
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
        return availableDoorways[Random.Range(0, availableDoorways.Count - 1)];
    }
}
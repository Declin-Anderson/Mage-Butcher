using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the info for anything that the room would need
/// </summary>
public class Room : MonoBehaviour
{
    // The doorways that are still available for use
    [SerializeField] public List<Transform> doorways;
    // If the Room is occupied *Still figuring out what I want to do with this*
    [SerializeField] public bool isOccupied = false;
}

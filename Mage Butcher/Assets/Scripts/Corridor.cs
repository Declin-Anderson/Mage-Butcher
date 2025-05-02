using UnityEngine;

/// <summary>
/// Holds the information for the corridors that will connect rooms
/// </summary>
public class Corridor : MonoBehaviour
{
    // The front end of the corridor
    [SerializeField] public Transform entryPoint;
    // The Back end of the corridor
    [SerializeField] public Transform exitPoint;
}

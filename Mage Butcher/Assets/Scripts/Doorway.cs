using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Info for the doorways that the rooms have
/// </summary>
public class Doorway : MonoBehaviour
{
    // If the doorway is currently being used by another object
    [SerializeField] public bool isUsed = false;
    // The room that is the parent to the object
    [SerializeField] public Room parentRoom;

    /// <summary>
    /// Checking to see if another doorway or a wall is blocking this doorway
    /// </summary>
    /// <param name="other"></param>
    public void OnTriggerStay(Collider other)
    {
        // If the doorway is colliding with another door or wall
        if (other.tag == "Doorway" || other.tag == "Wall")
        {
            isUsed = true;
        }
    }
}

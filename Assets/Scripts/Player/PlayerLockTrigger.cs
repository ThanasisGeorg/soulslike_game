using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLockTrigger : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform playerObj;

    [Header("Bear1")]
    [SerializeField] private Transform bear1Obj;

    [Header("Bear2")]
    [SerializeField] private Transform bear2Obj;

    [Header("Bear3")]
    [SerializeField] private Transform bear3Obj;

    private bool canLock = false;
    private Vector3 bearPositionToLock;
    private Vector3 bear1Position;
    private Vector3 bear2Position;
    private Vector3 bear3Position;

    void Start()
    {
        
    }

    void Update()
    {
        bear1Position = new(
            bear1Obj.position.x,
            playerObj.position.y,
            bear1Obj.position.z
        );

        bear2Position = new(
            bear2Obj.position.x,
            playerObj.position.y,
            bear2Obj.position.z
        );

        bear3Position = new(
            bear3Obj.position.x,
            playerObj.position.y,
            bear3Obj.position.z
        );
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BearTriggerZone1"))
        {
            canLock = true;
            bearPositionToLock = bear1Position;
        }
        else if (other.CompareTag("BearTriggerZone2"))
        {
            canLock = true;
            bearPositionToLock = bear2Position;
        }
        else if (other.CompareTag("BearTriggerZone3"))
        {
            canLock = true;
            bearPositionToLock = bear3Position;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("BearTriggerZone1"))
        {
            canLock = false;
        }
        else if (other.CompareTag("BearTriggerZone2"))
        {
            canLock = false;
        }
        else if (other.CompareTag("BearTriggerZone3"))
        {
            canLock = false;
        }
    }

    public bool CanLock()
    {
        return canLock;
    }

    public Vector3 BearPosition()
    {
        return bearPositionToLock;
    }
}

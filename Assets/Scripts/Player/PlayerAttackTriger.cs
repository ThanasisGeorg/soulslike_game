using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackTriger : MonoBehaviour
{
    [SerializeField] private BearStatus bear1Status;
    [SerializeField] private BearStatus bear2Status;
    [SerializeField] private BearStatus bear3Status;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Other: " + other.gameObject.name);
        if (other.gameObject.name.Equals("Bear1Obj"))
        {
            bear1Status.TakeDamage(10f);
        }
        else if (other.gameObject.name.Equals("Bear2Obj"))
        {
            bear2Status.TakeDamage(10f);
        }
        else if (other.gameObject.name.Equals("Bear3Obj"))
        {
            bear3Status.TakeDamage(10f);
        }
        else
        {
            return;
        }
    }
}

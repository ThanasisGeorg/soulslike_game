using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemAttackTrigger : MonoBehaviour
{
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private PlayerActions playerActions;
    private bool isBlocking;

    void Start()
    {
        
    }

    void Update()
    {
        isBlocking = playerActions.IsBlocking();
        Debug.Log("blocking: " + isBlocking);
    }
    private void OnCollisionEnter(Collision other)
    {
        if (isBlocking == false)
        {
            playerStatus.TakeDamage(10f);
        }
    }
}

using System.Collections;
using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private PlayerActions playerActions;

    private void OnCollisionEnter(Collision other)
    {
        if (playerActions.IsBlocking() == false)
        {
            playerStatus.TakeDamage(2f);
        }
    }
}

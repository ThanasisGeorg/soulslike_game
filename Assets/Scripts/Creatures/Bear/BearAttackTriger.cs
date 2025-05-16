using System.Collections;
using UnityEngine;

public class AttackTrigger : MonoBehaviour
{
    [SerializeField] private PlayerStatus playerStatus;

    private void OnCollisionEnter(Collision other)
    {
        playerStatus.TakeDamage(10f);
    }
}

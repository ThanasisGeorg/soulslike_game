using UnityEngine;

public class GolemTrigger : MonoBehaviour
{
    [SerializeField] private GameObject golemCanvas;
    public GolemActions golemActions;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            golemCanvas.SetActive(true);
            golemActions.StartChasing();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            golemCanvas.SetActive(false);
            golemActions.StopChasing();
        }
    }
}

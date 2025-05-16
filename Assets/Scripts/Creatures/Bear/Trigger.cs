using UnityEngine;

public class Trigger : MonoBehaviour
{
    [SerializeField] private GameObject bearCanvas;
    public BearActions bearActions;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bearCanvas.SetActive(true);
            bearActions.StartChasing();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bearCanvas.SetActive(false);
            bearActions.StopChasing();
        }
    }
}

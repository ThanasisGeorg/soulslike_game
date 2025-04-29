using UnityEngine;
using Cinemachine;

public class CameraCollision : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook freeLookCamera;
    [SerializeField] private Transform target; // Player or object the camera follows
    [SerializeField] private LayerMask collisionLayers; // Set this to "Default" or the layers you want to block the camera
    [SerializeField] private float minDistance = 1f; // Minimum allowed distance from target
    [SerializeField] private float maxDistance = 5f; // Default camera distance
    [SerializeField] private float smoothSpeed = 10f;

    private Vector3 currentVelocity;

    private void LateUpdate()
    {
        if (!freeLookCamera || !target) return;

        Vector3 direction = (freeLookCamera.transform.position - target.position).normalized;
        float targetDistance = maxDistance;
        
        // Raycast to detect walls
        if (Physics.Raycast(target.position, direction, out RaycastHit hit, maxDistance, collisionLayers))
        {
            targetDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
        }

        // Adjust FreeLook camera distance
        freeLookCamera.m_Orbits[1].m_Radius = Mathf.Lerp(freeLookCamera.m_Orbits[1].m_Radius, targetDistance, Time.deltaTime * smoothSpeed);
        freeLookCamera.m_Orbits[0].m_Radius = targetDistance * 0.75f; // Adjust top orbit
        freeLookCamera.m_Orbits[2].m_Radius = targetDistance * 1.25f; // Adjust bottom orbit
    }
}

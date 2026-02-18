using UnityEngine;

public class MinimapCameraBehaviour : MonoBehaviour
{
    [SerializeField] private Transform mainCam;

    private float fixedY;
    private float fixedZ;

    private void Awake()
    {
        if (mainCam == null && Camera.main != null)
            mainCam = Camera.main.transform;

        fixedY = transform.position.y;
        fixedZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (mainCam == null) return;

        transform.position = new Vector3(mainCam.position.x, fixedY, fixedZ);
    }
}


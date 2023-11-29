using UnityEngine;
using UnityEngine.Serialization;

public sealed class CameraController : MonoBehaviour
{
    [SerializeField] private Transform tilemap; // Reference to your tilemap object
    [SerializeField] private float padding = 1f; // Padding around the tilemap

    private Camera _mainCamera;

    private void Start() => _mainCamera = GetComponent<Camera>();

    /// <summary>
    /// Sets the camera it's position and look size.
    /// </summary>
    public void SetCameraPosition()
    {
        var tilemapBounds = CalculateTilemapBounds();
        MoveAndZoomCamera(tilemapBounds);
    }

    /// <summary>
    /// Calculates the tilemap size.
    /// </summary>
    /// <returns>The size of the tilemap</returns>
    private Bounds CalculateTilemapBounds()
    {
        var tilemapRenderers = tilemap.GetComponentsInChildren<Renderer>();

        if (tilemapRenderers.Length == 0) return new Bounds(tilemap.position, Vector3.zero);

        var bounds = tilemapRenderers[0].bounds;

        for (int i = 1; i < tilemapRenderers.Length; i++)
        {
            bounds.Encapsulate(tilemapRenderers[i].bounds);
        }

        return bounds;
    }

    /// <summary>
    /// Sets the zoom of the camera at the good distance, with the tilemap it's size.
    /// </summary>
    /// <param name="targetBounds">The tilemap size.</param>
    private void MoveAndZoomCamera(Bounds targetBounds)
    {
        var targetPosition = new Vector3( targetBounds.center.y > targetBounds.center.x ? (-targetBounds.center.y)+targetBounds.center.x: targetBounds.center.x/10, targetBounds.center.y, targetBounds.center.z);

        var targetSize = Mathf.Max(targetBounds.size.x, targetBounds.size.y) * 0.55f;

        var cameraTransform = _mainCamera.transform;
        cameraTransform.position = new Vector3(targetPosition.x, targetPosition.y, cameraTransform.position.z);
        _mainCamera.orthographicSize = targetSize + padding;
    }
}

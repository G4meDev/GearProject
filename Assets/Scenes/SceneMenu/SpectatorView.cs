using UnityEngine;

public class SpectatorView : MonoBehaviour
{
    public Canvas canvas;

    public void SetupCamera(Camera camera)
    {
        canvas.worldCamera = camera;
    }
    
    private void SpectateNeighbourVehicle(int offset)
    {
        var allVehicles = SceneManager.Get().allVehicles;
        var vehicleCamera = SceneManager.Get().vehicleCamera;

        if (allVehicles.Count == 0)
        {
            return;
        }

        Vehicle vehicle;

        if (vehicleCamera.vehicle && allVehicles.Contains(vehicleCamera.vehicle))
        {
            int i = allVehicles.IndexOf(vehicleCamera.vehicle) + offset;
            i = (i % allVehicles.Count + allVehicles.Count) % allVehicles.Count;
            vehicle = allVehicles[i];
        }

        else
        {
            vehicle = SceneManager.Get().allVehicles[0];
        }

        vehicleCamera.vehicle = vehicle;
    }

    public void SpectateNextVehicle()
    {
        SpectateNeighbourVehicle(1);
    }

    public void SpectatePreviousVehicle()
    {
        SpectateNeighbourVehicle(-1);
    }
}

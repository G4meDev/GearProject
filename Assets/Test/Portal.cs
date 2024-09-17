using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public GameObject target;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger");

        SC_TestVehicle vehicle = other.gameObject.transform.root.GetComponent<SC_TestVehicle>();
        
        vehicle.vehicleProxy.MovePosition(target.transform.position);
        vehicle.vehicleProxy.MoveRotation(target.transform.rotation);
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

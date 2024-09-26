using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    public GameObject target;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger");

        Vehicle vehicle = other.gameObject.transform.root.GetComponent<Vehicle>();
        
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

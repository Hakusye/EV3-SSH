using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchSensor : MonoBehaviour
{   
    public bool parking;
    void Start() {
        parking = false;
    }
 
    // Start is called before the first frame update
    void OnTriggerStay(Collider other) {
        if(other.CompareTag("wall")) parking = true;
    }
}

using UnityEngine;

public class DoorStabilizer : MonoBehaviour
{
    private HingeJoint hinge;
    
    void Start()
    {
        hinge = GetComponent<HingeJoint>();
    }

    void FixedUpdate()
    {
        // Apply slight force to stabilize the door
        if (Mathf.Abs(hinge.angle) > 5f)
        {
            float torque = -hinge.angle * 0.5f; // Light damping
            hinge.GetComponent<Rigidbody>().AddTorque(transform.up * torque);
        }
    }
}
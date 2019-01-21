using KinematicCharacterController.Examples;
using UnityEngine;

public class ExplosivePlacement : MonoBehaviour
{
    public float PlacementOffset = 5;

    public ExampleCharacterCamera OurCamera;
    public GameObject ExplostiveTemplate;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var heading = OurCamera.Camera.transform.forward.normalized;
            var targetPoint = OurCamera.transform.position + heading * PlacementOffset;

            Instantiate(ExplostiveTemplate, targetPoint, Quaternion.identity);
        }    
    }
}

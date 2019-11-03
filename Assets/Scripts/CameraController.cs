using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    
    private const int MOVE_SPEED = 2;
    private const int ROT_SPEED = 50;
    private const int ZOOM_SPEED = 60;

    private new Camera camera;

    void Start () {
        camera = GetComponent<Camera>();
	}

    void Update() {

        //Movement
        if (Input.GetKey(KeyCode.Space))
            camera.transform.position += camera.transform.up.normalized * MOVE_SPEED;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            camera.transform.position += camera.transform.up.normalized * -1 * MOVE_SPEED;
        if (Input.GetKey(KeyCode.W))
            camera.transform.position += camera.transform.forward.normalized * MOVE_SPEED;
        if (Input.GetKey(KeyCode.S))
            camera.transform.position += camera.transform.forward.normalized * -1 * MOVE_SPEED;
        if (Input.GetKey(KeyCode.A))
            camera.transform.position += camera.transform.right.normalized * -1 * MOVE_SPEED;
        if (Input.GetKey(KeyCode.D))
            camera.transform.position += camera.transform.right.normalized * MOVE_SPEED;

        //Rotation
        if (Input.GetKey(KeyCode.Mouse1))
        { 
            //Mouse X -> Rotate camera about World space Y axis
            camera.transform.Rotate(new Vector3(
                0,
                Input.GetAxis("Mouse X") * -1 * ROT_SPEED * Time.deltaTime,
                0),
                Space.World
            );

            //Mouse Y -> Rotate camera about local space X axis
            camera.transform.Rotate(new Vector3(
                Input.GetAxis("Mouse Y") * ROT_SPEED * Time.deltaTime,
                0,
                0),
                Space.Self
            );
        }

        //Zoom
        //Removed this because it was weird
        //Maybe reimplement as a toggle between a "zoomed" FOV and a "standard" FOV by pressing/releasing a Zoom key
        /*
        float fov = camera.fieldOfView;
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
            fov -= ZOOM_SPEED * Time.deltaTime;
        if(Input.GetAxis("Mouse ScrollWheel") < 0)
            fov += ZOOM_SPEED * Time.deltaTime;

        camera.fieldOfView = Mathf.Clamp(fov, 1, 120);
        */
    }
}

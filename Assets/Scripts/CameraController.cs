using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

	Camera camera;

	public float turnSpeed = 2.0f;      // Speed of camera turning when mouse moves in along an axis
	public float panSpeed = 2.0f;       // Speed of the camera when being panned
	public float zoomSpeed = 1.0f;      // Speed of the camera going back and forth
	public float translateSpeed = 0.015f;
	private Vector3 mouseOrigin;    // Position of cursor when mouse dragging starts
	private bool isPanning;     // Is the camera being panned?
	private bool isRotating;    // Is the camera being rotated?
	private bool isZooming;     // Is the camera zooming?

	public void InitCameraController()
	{
		camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

		transform.position = camera.transform.position;
		transform.rotation = camera.transform.rotation;
		camera.transform.parent = transform;
	}

	void Update()
	{

		if (Input.GetKey("up"))
		{
			transform.Translate(new Vector3(0, 0, 1) * translateSpeed);
		}

		if (Input.GetKey("down"))
		{
			transform.Translate(new Vector3(0, 0, -1) * translateSpeed);
		}

		if (Input.GetKey("left"))
		{
			transform.Translate(new Vector3(-1, 0, 0) * translateSpeed);
		}

		if (Input.GetKey("right"))
		{
			transform.Translate(new Vector3(1, 0, 0) * translateSpeed);
		}


		// Get the left mouse button
		if (Input.GetMouseButtonDown(0))
		{
			// Get mouse origin
			mouseOrigin = Input.mousePosition;
			isRotating = true;
		}

		// Get the right mouse button
		if (Input.GetMouseButtonDown(1))
		{
			// Get mouse origin
			mouseOrigin = Input.mousePosition;
			isPanning = true;
		}

		// Get the middle mouse button
		if (Input.GetMouseButtonDown(2))
		{
			// Get mouse origin
			mouseOrigin = Input.mousePosition;
			isZooming = true;
		}

		// Disable movements on button release
		if (!Input.GetMouseButton(0)) isRotating = false;
		if (!Input.GetMouseButton(1)) isPanning = false;
		if (!Input.GetMouseButton(2)) isZooming = false;

		// Rotate camera along X and Y axis
		if (isRotating)
		{
			Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

			transform.RotateAround(transform.position, transform.right, -pos.y * turnSpeed);
			transform.RotateAround(transform.position, Vector3.up, pos.x * turnSpeed);
		}

		// Move the camera on it's XY plane
		if (isPanning)
		{
			Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

			//Vector3 move = new Vector3(pos.x * panSpeed, pos.y * panSpeed, 0);
			Vector3 move = new Vector3(0, pos.y * panSpeed, 0);
			transform.Translate(move, Space.Self);
		}

		// Move the camera linearly along Z axis
		if (isZooming)
		{
			Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

			Vector3 move = pos.y * zoomSpeed * transform.forward;
			transform.Translate(move, Space.World);
		}
	}
}
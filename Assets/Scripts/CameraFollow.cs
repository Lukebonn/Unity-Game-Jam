
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	[Header("Follow Settings")]
	[SerializeField] private Transform target;
	[SerializeField] private float smoothTime = 0.25f;

	private Vector3 offset;
	private Vector3 velocity;
	private float cameraHeight;

	void Start()
	{
		if (target == null)
		{
			Debug.LogError("Camera target is missing!");
			enabled = false;
			return;
		}

		offset = transform.position - target.position;

		// Keep camera at a constant height
		cameraHeight = transform.position.y;
	}

	void LateUpdate()
	{
		if (target == null) return;

		// Follow player on X and Z only
		Vector3 desiredPosition = new Vector3(
			target.position.x + offset.x,
			cameraHeight,
			target.position.z + offset.z
		);

		transform.position = Vector3.SmoothDamp(
			transform.position,
			desiredPosition,
			ref velocity,
			smoothTime
		);
	}
}


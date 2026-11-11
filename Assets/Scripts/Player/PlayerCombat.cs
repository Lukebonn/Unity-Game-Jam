
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(LineRenderer))]
public class PlayerCombat : MonoBehaviour
{
	[Header("Input")]
	[SerializeField] private InputActionAsset inputActions;

	[Header("Movement")]
	public float moveSpeed = 6f;

	[Header("Shooting")]
	public float bulletsPerSecond = 6f;
	public int damagePerBullet = 10;
	public float bulletRange = 35f;

	public UnityEvent OnFire;

	private CharacterController controller;
	private Camera cam;
	private LineRenderer bulletLine;

	private InputActionMap playerMap;
	private InputAction moveAction;
	private InputAction pointAction;
	private InputAction fireAction;

	private float nextFireTime;
	private Coroutine bulletEffect;

	private void Awake()
	{
		controller = GetComponent<CharacterController>();
		cam = Camera.main;
		bulletLine = GetComponent<LineRenderer>();

		// Configure bullet visuals
		bulletLine.positionCount = 2;
		bulletLine.useWorldSpace = true;
		bulletLine.startWidth = 0.07f;
		bulletLine.endWidth = 0.07f;
		bulletLine.startColor = Color.yellow;
		bulletLine.endColor = Color.yellow;
		bulletLine.enabled = false;

		// Find our input actions
		if (inputActions == null)
		{
			Debug.LogError("Assign PlayerControls to PlayerCombat!");
			enabled = false;
			return;
		}

		playerMap = inputActions.FindActionMap("Player");

		if (playerMap == null)
		{
			Debug.LogError("Player action map not found!");
			enabled = false;
			return;
		}

		moveAction = playerMap.FindAction("Move");
		pointAction = playerMap.FindAction("Point");
		fireAction = playerMap.FindAction("Fire");

		if (moveAction == null ||
			pointAction == null ||
			fireAction == null)
		{
			Debug.LogError("Missing Move, Point, or Fire action!");
			enabled = false;
		}
	}

	private void OnEnable()
	{
		// Start listening for input
		playerMap?.Enable();
	}

	private void OnDisable()
	{
		// Stop listening for input
		playerMap?.Disable();

		if (bulletLine != null)
			bulletLine.enabled = false;
	}

	private void Update()
	{
		if (cam == null)
			return;

		HandleMovement();

		Vector3 aimDirection = GetAimDirection();

		if (aimDirection.sqrMagnitude > 0.01f)
		{
			transform.rotation =
				Quaternion.LookRotation(aimDirection);

			if (fireAction.IsPressed())
			{
				TryShoot(aimDirection);
			}
		}
	}

	private void HandleMovement()
	{
		// Read WASD using the New Input System
		Vector2 input = moveAction.ReadValue<Vector2>();

		Vector3 forward = cam.transform.forward;
		Vector3 right = cam.transform.right;

		forward.y = 0f;
		right.y = 0f;

		forward.Normalize();
		right.Normalize();

		Vector3 movement =
			forward * input.y +
			right * input.x;

		controller.SimpleMove(
			movement.normalized * moveSpeed
		);
	}

	private Vector3 GetAimDirection()
	{
		// Read mouse position from our Point action
		Vector2 mousePosition =
			pointAction.ReadValue<Vector2>();

		Ray ray = cam.ScreenPointToRay(mousePosition);

		Plane aimPlane = new Plane(
			Vector3.up,
			transform.position
		);

		if (aimPlane.Raycast(ray, out float distance))
		{
			Vector3 target = ray.GetPoint(distance);

			Vector3 direction =
				target - transform.position;

			direction.y = 0f;

			return direction.normalized;
		}

		return Vector3.zero;
	}

	private void TryShoot(Vector3 direction)
	{
		if (Time.time < nextFireTime)
			return;

		nextFireTime = Time.time +
			1f / Mathf.Max(0.1f, bulletsPerSecond);

		Shoot(direction);
	}

	private void Shoot(Vector3 direction)
	{
		OnFire?.Invoke();

		// Spawn point just in front of player
		Vector3 bulletOrigin =
			transform.position +
			Vector3.up * 0.2f +
			direction * 0.65f;

		Vector3 bulletEnd =
			bulletOrigin + direction * bulletRange;

		// Check if our bullet hits anything
		if (Physics.Raycast(
			bulletOrigin,
			direction,
			out RaycastHit hit,
			bulletRange))
		{
			bulletEnd = hit.point;

			// Damage any class inheriting EnemyBase
			EnemyBase enemy =
				hit.collider.GetComponentInParent<EnemyBase>();

			if (enemy != null)
			{
				enemy.TakeDamage(damagePerBullet);
			}
		}

		// Display the tracer
		if (bulletEffect != null)
		{
			StopCoroutine(bulletEffect);
		}

		bulletEffect = StartCoroutine(
			ShowBullet(bulletOrigin, bulletEnd)
		);
	}

	private IEnumerator ShowBullet(
		Vector3 start,
		Vector3 end)
	{
		bulletLine.SetPosition(0, start);
		bulletLine.SetPosition(1, end);

		bulletLine.enabled = true;

		yield return new WaitForSeconds(0.06f);

		bulletLine.enabled = false;
		bulletEffect = null;
	}
}

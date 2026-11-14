
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
	public float moveSpeed = 6f;                // top walking speed
	public float moveSpeedPerPoint = 0.15f;     // walk speed added per rolled point (d6)
	public float accelerationTime = 1f;         // seconds from standing still to top speed
	public float sprintCostPerSecond = 100f;    // a full starting stamina bar is 1 second of sprint
	private const float SprintMultiplier = 4f;  // sprint is 4x walk
	public float gravity = -30f;                // about 3x real gravity, so you drop into a hole fast

	[Header("Shooting")]
	public float bulletsPerSecond = 1.2f;
	public float fireRatePercentPerPoint = 3f;  // each rolled point: 3% faster than it is now (a die is about 10%)
	public int damagePerBullet = 10;
	public float damagePerPoint = 1f;
	public float bulletRange = 35f;

	public UnityEvent OnFire;

	// For the speedometer
	public float CurrentSpeed { get; private set; }
	public bool IsSprinting { get; private set; }

	private CharacterController controller;
	private Camera cam;
	private LineRenderer bulletLine;
	private PlayerStamina stamina;
	private PlayerStar star;

	private InputActionMap playerMap;
	private InputAction moveAction;
	private InputAction pointAction;
	private InputAction fireAction;
	private InputAction sprintAction;

	private Vector3 velocity;                   // sideways speed, built up and bled off gradually
	private float verticalVelocity;
	private Vector3 lastSafePosition;           // the last spot with floor under us, to climb back out of a hole
	private int groundMask;

	private float nextFireTime;
	private Coroutine bulletEffect;

	private void Awake()
	{
		controller = GetComponent<CharacterController>();
		// Speed builds up gradually, so the first frames' moves are tiny: don't let the
		// controller throw away moves under 1 mm (its default) at high frame rates
		controller.minMoveDistance = 0f;
		cam = Camera.main;
		bulletLine = GetComponent<LineRenderer>();

		// Sprint needs stamina, and the lucky star needs its own script. Add them if this player
		// doesn't have them yet (e.g. in a new level scene).
		stamina = GetComponent<PlayerStamina>();
		if (stamina == null)
			stamina = gameObject.AddComponent<PlayerStamina>();
		star = GetComponent<PlayerStar>();
		if (star == null)
			star = gameObject.AddComponent<PlayerStar>();

		lastSafePosition = transform.position;
		groundMask = ~LayerMask.GetMask("Player", "Enemy");   // anything solid but us and enemies is floor

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
		sprintAction = playerMap.FindAction("Sprint");

		if (moveAction == null ||
			pointAction == null ||
			fireAction == null ||
			sprintAction == null)
		{
			Debug.LogError("Missing Move, Point, Fire or Sprint action!");
			enabled = false;
		}
	}

	private void OnEnable()
	{
		// Start listening for input
		playerMap?.Enable();

		SignalBus.OnLvlMovement += HandleMoveLvlUp;
		SignalBus.OnLvlRPM += HandleRPMLvlUp;
		SignalBus.OnLvlDamage += HandleDamageLvlUp;
	}

	private void OnDisable()
	{
		// Stop listening for input
		playerMap?.Disable();

		SignalBus.OnLvlMovement -= HandleMoveLvlUp;
		SignalBus.OnLvlRPM -= HandleRPMLvlUp;
		SignalBus.OnLvlDamage -= HandleDamageLvlUp;

		if (bulletLine != null)
			bulletLine.enabled = false;
	}

	private void Update()
	{
		if (cam == null || PauseMenu.IsPaused)
			return;

		HandleMovement();

		// Safety net: fell off the world somewhere a hole's trigger didn't catch
		if (transform.position.y < -20f)
			ClimbOutOfHole();

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

		Vector3 wish = forward * input.y + right * input.x;
		if (wish.sqrMagnitude > 1f) wish.Normalize();

		// Sprint while Sprint (Left Shift) is held and we're moving. Shooting doesn't slow you.
		// A lucky star makes it cost half as much.
		bool wantsToSprint = sprintAction.IsPressed() && wish.sqrMagnitude > 0.01f;
		IsSprinting = wantsToSprint && stamina.TryUse(sprintCostPerSecond * Time.deltaTime / star.Multiplier);

		float topSpeed = moveSpeed * (IsSprinting ? SprintMultiplier : 1f) * star.Multiplier;

		// Speed builds up over about a second, and turning bleeds it off: the velocity is steered
		// toward where you want to go at a limited rate, so a sharp turn has to slow down first.
		Vector3 target = wish * topSpeed;
		float rate = topSpeed / Mathf.Max(0.05f, accelerationTime);
		// Slowing down (letting go, or after a sprint) is quicker than speeding up, so you don't slide
		if (target.sqrMagnitude < velocity.sqrMagnitude)
			rate = Mathf.Max(rate, velocity.magnitude * 2f / Mathf.Max(0.05f, accelerationTime));
		velocity = Vector3.MoveTowards(velocity, target, rate * Time.deltaTime);

		// Gravity: pressed to the floor normally, dropping fast over a hole
		if (controller.isGrounded && verticalVelocity < 0f)
			verticalVelocity = -2f;
		verticalVelocity += gravity * Time.deltaTime;

		CollisionFlags hits = controller.Move((velocity + Vector3.up * verticalVelocity) * Time.deltaTime);

		// How fast we actually moved, without the falling part.
		// Running into a wall takes away the speed going into it.
		Vector3 moved = controller.velocity;
		moved.y = 0f;
		if ((hits & CollisionFlags.Sides) != 0)
			velocity = moved;
		CurrentSpeed = moved.magnitude;

		// Remember where we last stood on solid floor
		if (controller.isGrounded && !OverHole())
			lastSafePosition = transform.position;
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
			1f / Mathf.Max(0.1f, bulletsPerSecond * star.Multiplier);

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

		// Check if our bullet hits anything. Enemy hitboxes are triggers (see CameraHitbox);
		// enemy bullets sit on the Ignore Raycast layer, so they never block ours.
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
				enemy.TakeDamage(Mathf.RoundToInt(damagePerBullet * star.Multiplier));
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

	// SignalBus listeners. The int is the number the die rolled.
	private void HandleMoveLvlUp(int roll)
	{
		moveSpeed += roll * moveSpeedPerPoint;
	}

	private void HandleRPMLvlUp(int roll)
	{
		bulletsPerSecond *= 1f + roll * fireRatePercentPerPoint / 100f;
	}

	private void HandleDamageLvlUp(int roll)
	{
		damagePerBullet += Mathf.RoundToInt(roll * damagePerPoint);
	}

	// True when there's no floor right under the middle of the player
	private bool OverHole()
	{
		Vector3 centre = transform.position + controller.center;
		return !Physics.Raycast(centre, Vector3.down, controller.height / 2f + 0.5f,
			groundMask, QueryTriggerInteraction.Ignore);
	}

	// A Trench calls this when the player falls in: back to the last safe spot, with half the health
	public void ClimbOutOfHole()
	{
		controller.enabled = false;   // a CharacterController ignores teleports while it's on
		transform.position = lastSafePosition;
		controller.enabled = true;
		velocity = Vector3.zero;
		verticalVelocity = 0f;

		PlayerHealth health = GetComponent<PlayerHealth>();
		if (health != null)
			health.LoseHalfHealth();
	}
}

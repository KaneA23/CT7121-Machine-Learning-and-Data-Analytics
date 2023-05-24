using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Zombie : Agent {

	#region Variables

	[SerializeField] private string targetTag;

	[Header("Movement")]
	[Tooltip("Force to apply when moving")]
	[SerializeField] private float moveForce = 15f;

	[Tooltip("Speed to rotate around Y-Axis")]
	[SerializeField] private float yawSpeed = 100f;
	private float smoothYawChange = 0f;

	[Tooltip("Agent's camera")]
	[SerializeField] private Camera agentCam;

	[Header("Machine Learning")]
	[SerializeField] private bool trainingMode;

	[SerializeField, Tooltip("Transform at agent's eyes")] private Transform eyes;

	private Rigidbody rb;

	private Spawner spawner;
	private Transform target;
	private bool isFrozen = false;

	[SerializeField] private LayerMask objectsToAvoid;
	Vector3 potentialPos;

	[SerializeField] private float sphereRadius = 1f;
	private RaycastHit hit;

	#endregion Variables

	// Start is called before the first frame update
	private void Start() {
		potentialPos = Vector3.zero;
	}

	private void FixedUpdate() {
		if (!trainingMode) {
			return;
		}

		Collider[] hitObstacles = Physics.OverlapSphere(transform.position, sphereRadius, objectsToAvoid.value);
		if (hitObstacles.Length > 0) {
			AddReward(-5f / 1000f);
		}
	}

	/// <summary>
	/// Set up the agent
	/// </summary>
	public override void Initialize() {
		rb = GetComponent<Rigidbody>();
		spawner = GetComponentInParent<Spawner>();

		// If not training mode, no max play, play forever
		if (!trainingMode) {
			MaxStep = 0;
		}
	}

	/// <summary>
	/// Reset agent when a new agent starts
	/// </summary>
	public override void OnEpisodeBegin() {
		target = spawner.target;
		if (trainingMode) {
			spawner.ResetSpawn();
		}

		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;

		bool inFrontOfTarget = true;
		if (trainingMode) {
			inFrontOfTarget = Random.value > 0.5f;
		}

		MoveToSafePosition(inFrontOfTarget);
	}

	/// <summary>
	/// Called when an action is received from either the player input or the ANN
	/// 
	/// vectorAction[i] represents:
	///		Index 0: move Vector X (+1 = right, -1 = left)
	///		Index 1: move Vector Z (+1 = forward, -1 = backward)
	///		Index 2: yaw angle (+1 = turn right, -1 = turn left)
	/// </summary>
	/// <param name="actions">Actions the player can take</param>
	public override void OnActionReceived(ActionBuffers actions) {
		if (isFrozen) {
			return;
		}

		ActionSegment<float> vectorAction = actions.ContinuousActions;
		Vector3 move = new Vector3(vectorAction[0], 0f, vectorAction[1]);

		// Add force in the direction of the move vector
		rb.AddForce(move * moveForce);

		// Get the current rotation
		Vector3 rotationVector = transform.rotation.eulerAngles;

		// Calculate yaw rotation
		float yawChange = vectorAction[2];
		smoothYawChange = Mathf.MoveTowards(smoothYawChange, yawChange, 2f * Time.fixedDeltaTime);

		float yaw = rotationVector.y + smoothYawChange * Time.fixedDeltaTime * yawSpeed;

		// Apply the new rotation
		transform.rotation = Quaternion.Euler(0f, yaw, 0f);
	}

	/// <summary>
	/// Collect vector observations from the environment
	/// </summary>
	/// <param name="sensor">The vector sensors</param>
	public override void CollectObservations(VectorSensor sensor) {
		// Observe the agent's local rotation (4 observations)
		sensor.AddObservation(transform.localRotation.normalized);

		// Observe normalised vector pointing to the target (3 observations)
		Vector3 toTarget = target.GetComponent<Target>().TargetCenterPosition - eyes.position;
		sensor.AddObservation(toTarget.normalized);

		sensor.AddObservation(Vector3.Dot(toTarget.normalized, -target.transform.position.normalized));

		sensor.AddObservation(Vector3.Dot(eyes.forward.normalized, -target.transform.position.normalized));

		sensor.AddObservation(toTarget.magnitude);  // distance to target

		// 10 total observations
	}

	/// <summary>
	/// Allows player insteads instead of using the ANN
	/// </summary>
	/// <param name="actionsOutBuffer">An output action array</param>
	public override void Heuristic(in ActionBuffers actionsOutBuffer) {
		ActionSegment<float> actionsOut = actionsOutBuffer.ContinuousActions;

		// Create placeholders for all movement/turning
		Vector3 forward = Vector3.zero;
		Vector3 left = Vector3.zero;
		float yaw = 0f;

		// Forward/backward
		if (Input.GetKey(KeyCode.W)) {
			forward = transform.forward;
		} else if (Input.GetKey(KeyCode.S)) {
			forward = -transform.forward;
		}

		// Left/right
		if (Input.GetKey(KeyCode.A)) {
			left = -transform.right;
		} else if (Input.GetKey(KeyCode.D)) {
			left = transform.right;
		}

		// Turn up/down
		if (Input.GetKey(KeyCode.LeftArrow)) {
			yaw = -1f;
		} else if (Input.GetKey(KeyCode.RightArrow)) {
			yaw = 1f;
		}

		// Combine the movement vectors and normalize
		Vector3 combined = (forward + left).normalized;

		// Add the movement values and yaw to the actionsOut array
		actionsOut[0] = combined.x;
		actionsOut[1] = combined.z;
		actionsOut[2] = yaw;
	}

	/// <summary>
	/// Prevent the agent from moving around and taking actions
	/// </summary>
	public void FreezeAgent() {
		Debug.Assert(trainingMode == false, "Freeze not supported in training mode");
		isFrozen = true;
		rb.Sleep();
	}

	/// <summary>
	/// Resume the agent movement and actions
	/// </summary>
	public void UnfreezeAgent() {
		Debug.Assert(trainingMode == false, "Unfreeze not supported in training mode");
		isFrozen = false;
		rb.WakeUp();
	}

	/// <summary>
	/// Finds somewhere to spawn the player within training room bounds
	/// </summary>
	/// <param name="a_isInFrontOfTarget">Chance of being placed in front of target to increase change of collecting reward</param>
	private void MoveToSafePosition(bool a_isInFrontOfTarget) {
		bool isSafePositionFound = false;
		int attemptsMade = 0;

		Quaternion potentialRot = new Quaternion();

		while (!isSafePositionFound && attemptsMade < 100) {
			attemptsMade++;

			if (a_isInFrontOfTarget) {
				float distanceFromTarget = Random.Range(1.1f, 1.5f);
				potentialPos = target.transform.position + (new Vector3(1, 0, 1) * distanceFromTarget);
				Vector3 toTarget = target.GetComponent<Target>().TargetCenterPosition - potentialPos;
				potentialRot = Quaternion.LookRotation(toTarget, Vector3.up);
			} else {
				float radius = Random.Range(2f, 5f);

				Quaternion direction = Quaternion.Euler(0f, Random.Range(-180f, 180f), 0);

				potentialPos = spawner.transform.position + direction * Vector3.forward * radius;

				float yaw = Random.Range(-180f, 180f);
				potentialRot = Quaternion.Euler(0f, yaw, 0);
			}

			Collider[] colliders = Physics.OverlapSphere(potentialPos, 1f, objectsToAvoid);
			isSafePositionFound = (colliders.Length == 0);
		}

		potentialPos.y = 1f;
		transform.SetPositionAndRotation(potentialPos, potentialRot);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="collision"></param>
	private void OnCollisionEnter(Collision collision) {
		if (trainingMode) {
			if (collision.collider.CompareTag(targetTag)) {
				AddReward(1f);
			}

			if (collision.collider.CompareTag("Obstacles") || collision.collider.CompareTag("Wall")) {
				AddReward(-0.5f);
			}

			OnCollisionEnterStay(collision);
		}
	}

	private void OnCollisionStay(Collision collision) {
		if (trainingMode) {
			OnCollisionEnterStay(collision);
		}
	}

	private void OnCollisionEnterStay(Collision collision) {
		//if (collision.collider.CompareTag("Obstacles") || collision.collider.CompareTag("Wall"))
		//{
		//    AddReward(-5f / 1000f);
		//}
	}

	//private void OnCollisionExit(Collision collision) {
	//	if (collision.collider.CompareTag("Obstacles") || collision.collider.CompareTag("Wall")) {
	//		AddReward(0.1f);
	//	}
	//}

	private void OnDrawGizmos() {
		Gizmos.DrawWireSphere(potentialPos, 1f);

		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, sphereRadius);
	}
}

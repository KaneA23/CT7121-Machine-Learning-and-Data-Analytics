using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Zombie : Agent {
	#region Variables

	[Header("Movement")]
	[Tooltip("Force to apply when moving")]
	public float moveForce = 15f;

	[Tooltip("Speed to rotate around Y-Axis")]
	public float yawSpeed = 100f;
	private float smoothYawChange = 0f;

	[Tooltip("Agent's camera")]
	public Camera agentCam;

	[Header("Machine Learning")]
	public bool trainingMode;

	private Rigidbody rb;

	private bool isFrozen = false;

	#endregion Variables

	// Start is called before the first frame update
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	/// <summary>
	/// Set up the agent
	/// </summary>
	public override void Initialize() {
		rb = GetComponent<Rigidbody>();

		// If not training mode, no max play, play forever
		if (!trainingMode) {
			MaxStep = 0;
		}
	}

	/// <summary>
	/// Reset agent when a new agent starts
	/// </summary>
	public override void OnEpisodeBegin() {
		rb.velocity = Vector3.zero;
		rb.angularVelocity = Vector3.zero;
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
}

using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Zombie : Agent
{

    #region Variables

    [SerializeField] private string target;

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

    private Rigidbody rb;

    private Spawner spawner;

    private bool isFrozen = false;

    [SerializeField] private LayerMask objectsToAvoid;
    Vector3 potentialPos;

    #endregion Variables

    // Start is called before the first frame update
    void Start()
    {
        potentialPos = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Set up the agent
    /// </summary>
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        spawner = GetComponentInParent<Spawner>();

        // If not training mode, no max play, play forever
        if (!trainingMode)
        {
            MaxStep = 0;
        }
    }

    /// <summary>
    /// Reset agent when a new agent starts
    /// </summary>
    public override void OnEpisodeBegin()
    {
        if (trainingMode)
        {
            spawner.ResetSpawn();
        }

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        bool inFrontOfTarget = true;
        if (trainingMode)
        {
            inFrontOfTarget = Random.value > 0.5f;
        }

        //MoveToSafePosition(inFrontOfTarget);
        transform.localPosition = Vector3.zero;
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
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isFrozen)
        {
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
    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe the agent's local rotation (4 observations)
        sensor.AddObservation(transform.localRotation.normalized);
    }

    /// <summary>
    /// Allows player insteads instead of using the ANN
    /// </summary>
    /// <param name="actionsOutBuffer">An output action array</param>
    public override void Heuristic(in ActionBuffers actionsOutBuffer)
    {
        ActionSegment<float> actionsOut = actionsOutBuffer.ContinuousActions;

        // Create placeholders for all movement/turning
        Vector3 forward = Vector3.zero;
        Vector3 left = Vector3.zero;
        float yaw = 0f;

        // Forward/backward
        if (Input.GetKey(KeyCode.W))
        {
            forward = transform.forward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            forward = -transform.forward;
        }

        // Left/right
        if (Input.GetKey(KeyCode.A))
        {
            left = -transform.right;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            left = transform.right;
        }

        // Turn up/down
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            yaw = -1f;
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
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
    public void FreezeAgent()
    {
        Debug.Assert(trainingMode == false, "Freeze not supported in training mode");
        isFrozen = true;
        rb.Sleep();
    }

    /// <summary>
    /// Resume the agent movement and actions
    /// </summary>
    public void UnfreezeAgent()
    {
        Debug.Assert(trainingMode == false, "Unfreeze not supported in training mode");
        isFrozen = false;
        rb.WakeUp();
    }

    private void MoveToSafePosition(bool a_isInFrontOfTarget)
    {
        bool isSafePositionFound = false;
        int attemptsMade = 0;

        Quaternion potentialRot = new Quaternion();

        Transform target = spawner.target;

        if (a_isInFrontOfTarget)
        {
            float xDistanceFromTarget;
            float zDistanceFromTarget;

            if (Random.value > 0.5f)
            {
                xDistanceFromTarget = Random.Range(1f, 2f);
            }
            else
            {
                xDistanceFromTarget = Random.Range(-1f, -2f);
            }

            if (Random.value > 0.5f)
            {
                zDistanceFromTarget = Random.Range(1f, 2f);
            }
            else
            {
                zDistanceFromTarget = Random.Range(-1f, -2f);
            }

            potentialPos = target.position + (target.forward * xDistanceFromTarget) + (target.right * zDistanceFromTarget);
            //Debug.Log(target.localPosition.ToString());
            //Debug.Log(potentialPos.ToString());

            if (potentialPos.x >= 11f || potentialPos.z >= 11f || potentialPos.x <= -11f || potentialPos.z <= -11f)
            {
                a_isInFrontOfTarget = false;
                //Debug.Log("Out of bounds");
            }
        }

        while (!isSafePositionFound && attemptsMade < 100)
        {
            attemptsMade++;

            if (a_isInFrontOfTarget)
            {
                Vector3 toTarget = target.localPosition - potentialPos;
                potentialRot = Quaternion.LookRotation(toTarget, Vector3.up);
            }
            else
            {
                potentialPos = new Vector3(Random.Range(-10f, 10f), 1f, Random.Range(-10f, 10f));
                potentialRot = Quaternion.Euler(0f, Random.Range(-180f, 180f), 0);
            }

            Collider[] colliders = Physics.OverlapSphere(potentialPos, 1f, objectsToAvoid);

            isSafePositionFound = (colliders.Length == 0);
            //Debug.Log("Safe: " + isSafePositionFound.ToString());
        }

        //Debug.Log("In front: " + a_isInFrontOfTarget.ToString());
        potentialPos.y = 1f;

        transform.localPosition = potentialPos;
        transform.rotation = potentialRot;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (trainingMode)
        {
            if (collision.collider.CompareTag(target))
            {
                AddReward(1f);
            }

            if (collision.collider.CompareTag("Obstacles"))
            {
                AddReward(-0.1f);
            }

            if (collision.collider.CompareTag("Wall"))
            {
                AddReward(-0.25f);
            }

            if (collision.collider.name == "Ceiling")
            {
                AddReward(-1f);
            }

            OnCollisionEnterStay(collision);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (trainingMode)
        {
            OnCollisionEnterStay(collision);
        }
    }

    private void OnCollisionEnterStay(Collision collision)
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(potentialPos, 1f);
    }
}

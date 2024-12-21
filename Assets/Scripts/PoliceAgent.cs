using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class PoliceAgent : Agent
{
    Rigidbody rBody;
    public Transform Thief;
    public GameObject[] Obstacles;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset position and velocity if the agent falls
        if (this.transform.localPosition.y < 0)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
        }

        // Randomize thief position
        Thief.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observa la posici�n del ladr�n
        sensor.AddObservation(Thief.localPosition);

        // Observa la posici�n del polic�a
        sensor.AddObservation(this.transform.localPosition);

        // Observa la velocidad del polic�a
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);

        // Observa las posiciones de los obst�culos
        foreach (var obstacle in Obstacles)
        {
            sensor.AddObservation(obstacle.transform.localPosition);
        }
    }

    public float forceMultiplier = 10;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Mover el polic�a seg�n las acciones recibidas
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * forceMultiplier);

        // Calcular la distancia al ladr�n
        float distanceToThief = Vector3.Distance(this.transform.localPosition, Thief.localPosition);

        // Recompensa por acercarse al ladr�n
        AddReward(-distanceToThief * 0.01f);

        // Penalizaci�n si choca con un obst�culo
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                AddReward(-0.1f);
            }
        }

        // Penalizaci�n si se cae
        if (this.transform.localPosition.y < 0)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}


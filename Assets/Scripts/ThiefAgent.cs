using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Collections.Generic;

public class ThiefAgent : Agent
{
    Rigidbody rBody;
    public Transform Police; // El polic�a que persigue al ladr�n
    public GameObject[] ObstacleGroups; // Obst�culos en el entorno
    private Transform[] Obstacles;
    private Vector3 initialPosition;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();

        // Almacena la posici�n inicial del agente
        initialPosition = this.transform.localPosition;

        var obstacleList = new List<Transform>();
        foreach (var group in ObstacleGroups)
        {
            foreach (Transform child in group.transform)
            {
                obstacleList.Add(child);
            }
        }
        Obstacles = obstacleList.ToArray();
    }

    public override void OnEpisodeBegin()
    {
        // Reset position and velocity if the agent falls
        if (this.transform.localPosition.y < -2)
        {
            rBody.angularVelocity = Vector3.zero;
            rBody.velocity = Vector3.zero;
            this.transform.localPosition = initialPosition;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observa la posici�n del polic�a
        sensor.AddObservation(Police.localPosition / 8f);

        // Observa la posici�n del ladr�n
        sensor.AddObservation(this.transform.localPosition / 8f);

        // Observa la velocidad del ladr�n
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);

        // Observa las posiciones de los obst�culos
        foreach (var obstacle in Obstacles)
        {
            sensor.AddObservation(obstacle.transform.localPosition);
        }
    }

    public float forceMultiplier = 50;
    public float maxDistance = 10f; // Distancia m�xima para huir
    public float policeTouchDistance = 1f; // Distancia en la que el polic�a toca al ladr�n

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Obtener direcci�n de huida
        Vector3 directionToPolice = Police.localPosition - this.transform.localPosition;
        Vector3 escapeDirection = -directionToPolice.normalized;

        // Aplicar la acci�n de movimiento
        Vector3 controlSignal = new Vector3(actionBuffers.ContinuousActions[0], 0, actionBuffers.ContinuousActions[1]);
        controlSignal = controlSignal.normalized * forceMultiplier; // Normalizamos para mantener la direcci�n

        rBody.AddForce(escapeDirection * forceMultiplier); // Fuerza para huir del polic�a

        // Calcular la distancia al polic�a
        float distanceToPolice = Vector3.Distance(this.transform.localPosition, Police.localPosition);

        // Recompensa por alejarse del polic�a
        if (distanceToPolice > maxDistance)
        {
            AddReward(0.05f); // Recompensa por moverse lejos
        }

        // Penalizaci�n si se acerca al polic�a
        if (distanceToPolice < policeTouchDistance)
        {
            SetReward(-1.0f); // Penalizaci�n por ser atrapado
            EndEpisode(); // Reinicia el episodio
        }

        // Penalizaci�n si no se mueve
        if (controlSignal.magnitude < 0.01f)
        {
            AddReward(-0.05f); // Penalizaci�n por no moverse
        }

        // Penalizaci�n si choca con un obst�culo
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                AddReward(-0.1f); // Penalizaci�n por chocar con un obst�culo
            }
        }

        // Penalizaci�n si se cae
        if (this.transform.localPosition.y < 0)
        {
            SetReward(-1.0f); // Penalizaci�n por caerse
            EndEpisode(); // Reinicia el episodio
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}
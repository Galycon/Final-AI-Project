using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System.Collections.Generic;

public class PoliceAgent : Agent
{
    Rigidbody rBody;
    public Transform Target;
    public Transform[] SpawnPoints;
    private Vector3 Map = new Vector3(0, 0, 0);
    private Vector3 MapSize = new Vector3(45, 0, 80);
    public GameObject[] ObstacleGroups; // Obstáculos en el entorno
    private Transform[] Obstacles;
    private Vector3 initialPosition;
    public float forceMultiplier = 10;

    // Boundaries of the city (dynamically set)
    private float minX;
    private float maxX;
    private float minZ;
    private float maxZ;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();

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

        // Calcular los límites usando el centro del mapa y el tamaño
        if (Map != null)
        {
            Vector3 center = Map; // Obtén la posición del Transform del mapa
            minX = center.x - MapSize.x / 2;
            maxX = center.x + MapSize.x / 2;
            minZ = center.z - MapSize.z / 2;
            maxZ = center.z + MapSize.z / 2;

            Debug.Log("Map Boundaries:");
            Debug.Log("Min X: " + minX + " Max X: " + maxX);
            Debug.Log("Min Z: " + minZ + " Max Z: " + maxZ);
        }
        else
        {
            Debug.LogError("MapCenter is not assigned!");
        }
    }

    public override void OnEpisodeBegin()
    {
        // Reset the agent's momentum and position
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = initialPosition;
        this.transform.localRotation = Quaternion.Euler(0, 0, 0);

        // Move the target to a random spawn point
        if (SpawnPoints.Length > 0)
        {
            // Seleccionar un punto de spawn aleatorio
            int randomIndex = Random.Range(0, SpawnPoints.Length);
            Target.position = SpawnPoints[randomIndex].position; // Usamos la posición del Transform
            Debug.Log("Target spawned at: " + Target.position);
        }
        else
        {
            Debug.LogError("No spawn points assigned!");
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);

        // Observa las posiciones de los obstáculos
        foreach (var obstacle in Obstacles)
        {
            sensor.AddObservation(obstacle.transform.localPosition);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * forceMultiplier);

        // Limitar la posición en Y para evitar movimientos no deseados
        Vector3 fixedPosition = transform.localPosition;
        fixedPosition.y = initialPosition.y; // Mantén la altura inicial
        transform.localPosition = fixedPosition;

        // Obtener la dirección de movimiento
        Vector3 movementDirection = new Vector3(controlSignal.x, 0, controlSignal.z);

        // Si hay movimiento en el eje X o Z, rotar en el eje Z
        if (movementDirection.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(movementDirection.z, movementDirection.x) * Mathf.Rad2Deg; // Calcular el ángulo en grados
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle); // Aplicar solo rotación en el eje Z
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // Suavizar la rotación
        }

        // Limitar la rotación en los ejes X e Y
        Vector3 fixedRotation = transform.eulerAngles;
        fixedRotation.x = -90f; // Ángulo fijo en X
        fixedRotation.y = 0f;   // Ángulo fijo en Y
        transform.eulerAngles = fixedRotation;

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // Reached target
        if (distanceToTarget < 1.42f)
        {
            Debug.Log("Agent reached target");

            SetReward(1.0f);
            EndEpisode();
        }

        // Penalización si choca con un obstáculo
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                Debug.Log("Agent collision with obstacle");

                AddReward(-0.1f); // Penalización por chocar con un obstáculo
                EndEpisode();
            }
        }

        // Check if the agent is out of bounds
        if (this.transform.localPosition.x < minX || this.transform.localPosition.x > maxX ||
            this.transform.localPosition.z < minZ || this.transform.localPosition.z > maxZ)
        {
            Debug.Log("Agent out of bounds");
            Debug.Log("Agent position: " + transform.localPosition);
            SetReward(-0.1f); // Penalize for going out of bounds
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

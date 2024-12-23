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
    public GameObject[] ObstacleGroups; // Obst�culos en el entorno
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

        // Calcular los l�mites usando el centro del mapa y el tama�o
        if (Map != null)
        {
            Vector3 center = Map; // Obt�n la posici�n del Transform del mapa
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

        rBody.centerOfMass = new Vector3(0, 0, 0);
    }

    public override void OnEpisodeBegin()
    {
        // Reset the agent's momentum and position
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = initialPosition;

        // Move the target to a random spawn point
        if (SpawnPoints.Length > 0)
        {
            // Seleccionar un punto de spawn aleatorio
            int randomIndex = Random.Range(0, SpawnPoints.Length);
            Target.position = SpawnPoints[randomIndex].position; // Usamos la posici�n del Transform
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

        // Observa las posiciones de los obst�culos
        foreach (var obstacle in Obstacles)
        {
            sensor.AddObservation(obstacle.transform.localPosition);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Acciones
        float forwardSignal = actionBuffers.ContinuousActions[0]; // Avanzar o retroceder
        float turnSignal = actionBuffers.ContinuousActions[1];    // Girar izquierda/derecha

        // Movimiento hacia adelante o atr�s (corregido para ignorar inclinaci�n en X)
        Vector3 forwardDirection = transform.forward;
        Vector3 forwardMovement = forwardDirection * forwardSignal * forceMultiplier;
        rBody.AddForce(forwardMovement, ForceMode.Force);

        //// Restringir la velocidad lateral
        //Vector3 localVelocity = transform.InverseTransformDirection(rBody.velocity);
        //localVelocity.x = 0; // Eliminar movimiento lateral
        //rBody.velocity = transform.TransformDirection(localVelocity);

        // Giro
        float turnSpeed = 100f; // Ajusta la velocidad de giro seg�n sea necesario
        rBody.angularVelocity = new Vector3(0, turnSignal * turnSpeed * Mathf.Deg2Rad, turnSignal * turnSpeed * Mathf.Deg2Rad);

        // Limitar la posici�n en Y para evitar movimientos no deseados
        Vector3 fixedPosition = transform.localPosition;
        fixedPosition.y = initialPosition.y; // Mant�n la altura inicial
        transform.localPosition = fixedPosition;


        // **Recompensas por alineaci�n de la direcci�n de movimiento y rotaci�n**
        float forwardDotProduct = Vector3.Dot(forwardDirection, rBody.velocity.normalized); // Producto punto entre la direcci�n y la velocidad del agente

        // Recompensar si el agente est� avanzando en la misma direcci�n en la que est� mirando
        // Si el valor del producto punto es cercano a 1 (es decir, el agente se mueve en la misma direcci�n que est� mirando)
        float alignmentReward = Mathf.Clamp(forwardDotProduct, 0f, 1f); // Asegurarse de que est� entre 0 y 1
        AddReward(alignmentReward * 0.0001f); // Puedes ajustar la multiplicaci�n para controlar la magnitud de la recompensa

        // **Recompensa por moverse hacia el objetivo**
        Vector3 directionToTarget = (Target.localPosition - transform.localPosition).normalized; // Direcci�n hacia el objetivo
        float moveTowardsTarget = Vector3.Dot(directionToTarget, rBody.velocity.normalized); // Producto punto entre la direcci�n al objetivo y la velocidad del agente

        // Recompensar si el agente se mueve hacia el objetivo
        if (moveTowardsTarget > 0)
        {
            AddReward(0.0001f); // Recompensa peque�a por moverse hacia el objetivo
        }

        // Rewards y otras l�gicas (sin cambios)
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // Reached target
        if (distanceToTarget < 1.42f)
        {
            Debug.Log("Agent reached target");

            SetReward(1.0f);
            EndEpisode();
        }

        // Penalizaci�n si choca con un obst�culo
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                Debug.Log("Agent collision with obstacle");

                AddReward(-0.5f); // Penalizaci�n por chocar con un obst�culo
                EndEpisode();
            }
        }

        // Check if the agent is out of bounds
        if (this.transform.localPosition.x < minX || this.transform.localPosition.x > maxX ||
            this.transform.localPosition.z < minZ || this.transform.localPosition.z > maxZ)
        {
            Debug.Log("Agent out of bounds");
            Debug.Log("Agent position: " + transform.localPosition);
            SetReward(-0.5f); // Penalize for going out of bounds
            EndEpisode();
        }
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    void OnDrawGizmos()
    {
        // Aseg�rate de que solo se dibujen gizmos si el objeto tiene un transform v�lido
        if (this.transform != null)
        {
            Gizmos.color = Color.blue;
            // Dibuja una l�nea que indica hacia d�nde est� mirando el frente del coche
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 2f);

            Gizmos.color = Color.red;
            // Dibuja una l�nea que indique la direcci�n "atr�s" del coche
            Gizmos.DrawLine(transform.position, transform.position - transform.forward * 2f);
        }
    }
}
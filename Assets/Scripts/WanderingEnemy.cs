using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WanderingAgent : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform[] waypoints;
    public Transform policeAgent; // Referencia al agente policía
    public float detectionRadius = 10f; // Radio de detección del policía
    private int currentWaypointIndex;
    private int lastWaypointIndex; // Último waypoint visitado
    private bool isEscaping = false;

    void Start()
    {
        // Verificar que haya al menos dos waypoints
        if (waypoints.Length < 2)
        {
            Debug.LogError("Se necesitan al menos 2 waypoints para patrullar.");
            return;
        }

        agent = GetComponent<NavMeshAgent>();

        // Iniciar en un waypoint aleatorio
        currentWaypointIndex = Random.Range(0, waypoints.Length);
        lastWaypointIndex = currentWaypointIndex;

        // Establecer el destino inicial
        agent.SetDestination(waypoints[currentWaypointIndex].position);

        // Configurar la zona de detección
        SphereCollider detectionCollider = gameObject.AddComponent<SphereCollider>();
        detectionCollider.isTrigger = true;
        detectionCollider.radius = detectionRadius;
    }

    void Update()
    {
        if (agent.velocity == Vector3.zero)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // Registrar el último waypoint antes de actualizar
                lastWaypointIndex = currentWaypointIndex;

                // Actualizar el waypoint
                UpdateWaypoint();

                // Establecer la nueva posición de destino
                agent.SetDestination(waypoints[currentWaypointIndex].position);
            }
        }
        else
        {
            if (isEscaping) return;


            // Si el agente ha llegado al waypoint actual
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // Registrar el último waypoint antes de actualizar
                lastWaypointIndex = currentWaypointIndex;

                // Actualizar el waypoint
                UpdateWaypoint();

                // Establecer la nueva posición de destino
                agent.SetDestination(waypoints[currentWaypointIndex].position);
            }
        }
        

        
    }

    // Actualizar el índice del waypoint para avanzar al siguiente
    void UpdateWaypoint()
    {
        currentWaypointIndex++;

        // Si llegamos al final de los waypoints, reiniciar al primero
        if (currentWaypointIndex >= waypoints.Length)
        {
            currentWaypointIndex = 0;
        }
    }

    // Detectar al policía dentro del rango
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == policeAgent)
        {
            isEscaping = true;
            ReturnToLastWaypoint();
        }
    }

    // Cuando el policía sale del rango, reanudar patrullaje
    private void OnTriggerExit(Collider other)
    {
        if (other.transform == policeAgent)
        {
            isEscaping = false;
            // Reanudar patrullaje
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    // Regresar al último waypoint visitado
    private void ReturnToLastWaypoint()
    {
        Debug.Log("Policía detectado, volviendo al último waypoint.");
        agent.SetDestination(waypoints[lastWaypointIndex].position);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatrollingAgent : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform[] waypoints;
    private int currentWaypointIndex;
    private bool isStopped = false; // Indica si el agente est� detenido

    void Start()
    {
        // Aseg�rate de que haya al menos dos waypoints
        if (waypoints.Length < 2)
        {
            Debug.LogError("Se necesitan al menos 2 waypoints para patrullar.");
            return;
        }

        agent = GetComponent<NavMeshAgent>();

        // Seleccionar aleatoriamente el punto de inicio
        currentWaypointIndex = 0;

        // Establecer el destino inicial
        agent.SetDestination(waypoints[currentWaypointIndex].position);
    }

    void Update()
    {
        if (isStopped) return; // No hacer nada si el agente est� detenido

        // Si el agente ha llegado al waypoint actual
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Actualizar el waypoint para avanzar hacia adelante
            UpdateWaypoint();

            // Establecer la nueva posici�n de destino
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    // Actualizar el �ndice del waypoint para avanzar hacia adelante
    void UpdateWaypoint()
    {
        // Avanzar al siguiente waypoint
        currentWaypointIndex++;

        // Si llegamos al final de los waypoints, reiniciar al primero
        if (currentWaypointIndex >= waypoints.Length)
        {
            currentWaypointIndex = 0;
        }
    }

    // M�todo para detectar colisiones con el Trigger
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pederestian"))
        {
            Debug.Log("Pedestrian detected, stopping the agent.");
            isStopped = true;
            agent.isStopped = true; // Detener el agente
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pederestian"))
        {
            Debug.Log("Pedestrian left, resuming the agent.");
            isStopped = false;
            agent.isStopped = false; // Reanudar el agente
            agent.SetDestination(waypoints[currentWaypointIndex].position); // Asegurar que el agente contin�e
        }
    }
}

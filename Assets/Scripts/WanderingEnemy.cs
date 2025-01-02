using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WanderingAgent : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform[] waypoints;
    public Transform policeAgent; // Referencia al agente polic�a
    public float detectionRadius = 10f; // Radio de detecci�n del polic�a
    private int currentWaypointIndex;
    private int lastWaypointIndex; // �ltimo waypoint visitado
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

        // Configurar la zona de detecci�n
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
                // Registrar el �ltimo waypoint antes de actualizar
                lastWaypointIndex = currentWaypointIndex;

                // Actualizar el waypoint
                UpdateWaypoint();

                // Establecer la nueva posici�n de destino
                agent.SetDestination(waypoints[currentWaypointIndex].position);
            }
        }
        else
        {
            if (isEscaping) return;


            // Si el agente ha llegado al waypoint actual
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // Registrar el �ltimo waypoint antes de actualizar
                lastWaypointIndex = currentWaypointIndex;

                // Actualizar el waypoint
                UpdateWaypoint();

                // Establecer la nueva posici�n de destino
                agent.SetDestination(waypoints[currentWaypointIndex].position);
            }
        }
        

        
    }

    // Actualizar el �ndice del waypoint para avanzar al siguiente
    void UpdateWaypoint()
    {
        currentWaypointIndex++;

        // Si llegamos al final de los waypoints, reiniciar al primero
        if (currentWaypointIndex >= waypoints.Length)
        {
            currentWaypointIndex = 0;
        }
    }

    // Detectar al polic�a dentro del rango
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == policeAgent)
        {
            isEscaping = true;
            ReturnToLastWaypoint();
        }
    }

    // Cuando el polic�a sale del rango, reanudar patrullaje
    private void OnTriggerExit(Collider other)
    {
        if (other.transform == policeAgent)
        {
            isEscaping = false;
            // Reanudar patrullaje
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    // Regresar al �ltimo waypoint visitado
    private void ReturnToLastWaypoint()
    {
        Debug.Log("Polic�a detectado, volviendo al �ltimo waypoint.");
        agent.SetDestination(waypoints[lastWaypointIndex].position);
    }
}

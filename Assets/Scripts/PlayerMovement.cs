using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    private NavMeshAgent agent;

    void Start()
    {
        // Obtén el componente NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Detectar el clic del ratón
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            // Crear un rayo desde la cámara hacia la posición del ratón
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Verificar si el rayo golpea algo en el plano del NavMesh
            if (Physics.Raycast(ray, out hit))
            {
                // Mover el agente hacia la posición donde se hizo clic
                agent.SetDestination(hit.point);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
    private NavMeshAgent agent;

    void Start()
    {
        // Obt�n el componente NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Detectar el clic del rat�n
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            // Crear un rayo desde la c�mara hacia la posici�n del rat�n
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Verificar si el rayo golpea algo en el plano del NavMesh
            if (Physics.Raycast(ray, out hit))
            {
                // Mover el agente hacia la posici�n donde se hizo clic
                agent.SetDestination(hit.point);
            }
        }
    }
}
using UnityEngine;

public class PoliceChase : MonoBehaviour
{
    public Transform thief;  // El ladr�n que el polic�a debe perseguir
    public float chaseSpeed = 3f;  // Velocidad de persecuci�n del polic�a
    public float distanceThreshold = 2f;  // Distancia m�nima para reiniciar el episodio

    private Rigidbody rBody;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Calcular la direcci�n hacia el ladr�n
        Vector3 directionToThief = thief.position - transform.position;

        // Normalizar la direcci�n para que el polic�a se mueva a una velocidad constante
        Vector3 chaseDirection = directionToThief.normalized;

        // Mover al polic�a hacia el ladr�n
        rBody.MovePosition(transform.position + chaseDirection * chaseSpeed * Time.deltaTime);

        // Si el polic�a alcanza al ladr�n (distancia peque�a)
        if (Vector3.Distance(transform.position, thief.position) < distanceThreshold)
        {
            // Aqu� podr�as a�adir el c�digo para reiniciar el episodio si es necesario
            // Por ejemplo, si usas un sistema de ML-Agents:
            // SetReward(-1f);
            // EndEpisode();
            Debug.Log("�El polic�a ha atrapado al ladr�n!");
            // Puedes usar `SceneManager.LoadScene()` o cualquier otra l�gica para reiniciar el escenario
        }
    }
}

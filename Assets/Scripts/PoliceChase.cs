using UnityEngine;

public class PoliceChase : MonoBehaviour
{
    public Transform thief;  // El ladrón que el policía debe perseguir
    public float chaseSpeed = 3f;  // Velocidad de persecución del policía
    public float distanceThreshold = 2f;  // Distancia mínima para reiniciar el episodio

    private Rigidbody rBody;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Calcular la dirección hacia el ladrón
        Vector3 directionToThief = thief.position - transform.position;

        // Normalizar la dirección para que el policía se mueva a una velocidad constante
        Vector3 chaseDirection = directionToThief.normalized;

        // Mover al policía hacia el ladrón
        rBody.MovePosition(transform.position + chaseDirection * chaseSpeed * Time.deltaTime);

        // Si el policía alcanza al ladrón (distancia pequeña)
        if (Vector3.Distance(transform.position, thief.position) < distanceThreshold)
        {
            // Aquí podrías añadir el código para reiniciar el episodio si es necesario
            // Por ejemplo, si usas un sistema de ML-Agents:
            // SetReward(-1f);
            // EndEpisode();
            Debug.Log("¡El policía ha atrapado al ladrón!");
            // Puedes usar `SceneManager.LoadScene()` o cualquier otra lógica para reiniciar el escenario
        }
    }
}

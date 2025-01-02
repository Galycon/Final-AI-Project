using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System;
using Unity.MLAgents.Actuators;

public class ThiefAgent : Agent
{
    [Header("Velocidad")]
    [Range(0f, 5f)]
    public float _speed;

    [Header("Velocidad de giro")]
    [Range(50f, 300f)]
    public float _turnSpeed;

    public bool _training = true;

    private Rigidbody _rb;

    [Header("Spawnpoints")]
    [SerializeField]
    private Transform _thiefSpawnPoint; // Spawnpoint del ladrón
    [SerializeField]
    private List<Transform> _targetSpawnPoints; // Lista de spawnpoints del objetivo

    [SerializeField]
    private Transform _target; // Referencia al objetivo

    private Animator _anim;

    private Vector3 _previous;

    public override void Initialize()
    {
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
        _previous = transform.position;

        if (!_training) MaxStep = 0;
    }

    public override void OnEpisodeBegin()
    {
        // Reiniciar velocidad y posición del ladrón
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        MoverAlSpawnpoint(); // Mover al ladrón a su spawnpoint
        PosicionarTargetAleatoriamente(); // Colocar el objetivo en un spawnpoint aleatorio

        _previous = transform.position;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        int lForward = actionBuffers.DiscreteActions[0];  // Acción para avanzar (0 o 1)
        int lTurn = actionBuffers.DiscreteActions[1];     // Acción para girar (-1, 0, o 1)

        float forwardMovement = lForward == 1 ? 1f : 0f;  // Si es 1, avanzar; si no, detenerse.

        float turnDirection = 0f;
        if (lTurn == 1) turnDirection = 1f; // Girar a la derecha
        else if (lTurn == -1) turnDirection = -1f; // Girar a la izquierda

        _rb.MovePosition(transform.position + transform.forward * forwardMovement * _speed * Time.deltaTime);
        transform.Rotate(transform.up * turnDirection * _turnSpeed * Time.deltaTime);

        // Comprobar si el ladrón se cae del mapa
        if (transform.position.y < -5f) // Si cae por debajo de un límite
        {
            AddReward(-1f); // Penalizar
            MoverAlSpawnpoint(); // Volver al spawnpoint
            EndEpisode(); // Finalizar episodio
        }
    }

    public void Update()
    {
        float velocity = ((transform.position - _previous).magnitude) / Time.deltaTime;
        _previous = transform.position;

        if (_anim != null)
        {
            _anim.SetFloat("multiplicador", velocity);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        int lForward = 0;
        if (Input.GetKey(KeyCode.UpArrow)) lForward = 1;

        int lTurn = 0;
        if (Input.GetKey(KeyCode.LeftArrow)) lTurn = -1;
        else if (Input.GetKey(KeyCode.RightArrow)) lTurn = 1;

        discreteActions[0] = lForward;
        discreteActions[1] = lTurn;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(Vector3.Distance(_target.transform.position, transform.position));
        sensor.AddObservation((_target.transform.position - transform.position).normalized);
        sensor.AddObservation(transform.forward);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_training)
        {
            if (other.CompareTag("Target"))
            {
                AddReward(1.0f); // Recompensa por tocar el objetivo
                PosicionarTargetAleatoriamente(); // Cambiar el objetivo de posición
            }
            if (other.CompareTag("Obstacle"))
            {
                AddReward(-0.05f);
                MoverAlSpawnpoint(); // Volver al spawnpoint
                EndEpisode();
            }
        }
    }

    private void MoverAlSpawnpoint()
    {
        if (_thiefSpawnPoint != null)
        {
            transform.position = _thiefSpawnPoint.position;
            transform.rotation = _thiefSpawnPoint.rotation;
        }
        else
        {
            Debug.LogError("Spawnpoint del ladrón no asignado.");
        }
    }

    private void PosicionarTargetAleatoriamente()
    {
        if (_targetSpawnPoints.Count > 0)
        {
            int indexAleatorio = UnityEngine.Random.Range(0, _targetSpawnPoints.Count);
            Transform spawnPointSeleccionado = _targetSpawnPoints[indexAleatorio];
            _target.position = spawnPointSeleccionado.position;
            _target.rotation = spawnPointSeleccionado.rotation;
        }
        else
        {
            Debug.LogError("No hay spawnpoints asignados para el objetivo.");
        }
    }
}

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

    [SerializeField]
    private Transform _target;
    private Animator _anim;

    private Vector3 _previous;


    public override void Initialize()
    {
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
        _previous = transform.position;

        //MaxStep forma parte de la clase Agent
        if (!_training) MaxStep = 0;
    }

    public override void OnEpisodeBegin()
    {
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        MoverPosicionInicial();
        _previous = transform.position;
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Acceder a las acciones discretas
        int lForward = actionBuffers.DiscreteActions[0];  // Acción para avanzar (0 o 1)
        int lTurn = actionBuffers.DiscreteActions[1];     // Acción para girar (-1, 0, o 1)

        // Convertir la acción para avanzar
        float forwardMovement = lForward == 1 ? 1f : 0f;  // Si es 1, avanzar; si no, detenerse.

        // Convertir la acción para girar
        float turnDirection = 0f;
        if (lTurn == 1)
        {
            turnDirection = 1f; // Girar a la derecha
        }
        else if (lTurn == -1)
        {
            turnDirection = -1f; // Girar a la izquierda
        }

        // Mover el agente hacia adelante
        _rb.MovePosition(transform.position + transform.forward * forwardMovement * _speed * Time.deltaTime);

        // Rotar el agente
        transform.Rotate(transform.up * turnDirection * _turnSpeed * Time.deltaTime);
    }

    public void Update()
    {
        float velocity = ((transform.position - _previous).magnitude) / Time.deltaTime;
        _previous = transform.position;

        _anim.SetFloat("multiplicador", velocity);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Obtener las acciones de tipo discreto
        var discreteActions = actionsOut.DiscreteActions;

        // Acción para avanzar (0 = no avanzar, 1 = avanzar)
        int lForward = 0;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            lForward = 1;  // Avanzar
        }

        // Acción para girar (-1 = izquierda, 0 = no girar, 1 = derecha)
        int lTurn = 0;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            lTurn = -1;  // Girar a la izquierda
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            lTurn = 1;   // Girar a la derecha
        }

        // Asignar las acciones al ActionBuffers
        discreteActions[0] = lForward; // Acción de avance
        discreteActions[1] = lTurn;    // Acción de giro
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Distancia al target.
        //Float de 1 posicion.
        sensor.AddObservation(
        Vector3.Distance(_target.transform.position, transform.position));

        //Dirección al target.
        //Vector 3 posiciones. 
        sensor.AddObservation(
            (_target.transform.position - transform.position).normalized);

        //Vector del señor, donde mira.
        //Vector de 3 posiciones. 
        sensor.AddObservation(
            transform.forward);
    }

    private void OnTriggerStay(Collider other)
    {
        if (_training)
        {
            if (other.CompareTag("target"))
            {
                AddReward(0.5f);

            }
            if (other.CompareTag("borders"))
            {
                AddReward(-0.05f);
            }
        }
    }


    private void MoverPosicionInicial()
    {
        bool posicionEncontrada = false;
        int intentos = 100;
        Vector3 posicionPotencial = Vector3.zero;

        while (!posicionEncontrada || intentos >= 0)
        {
            intentos--;
            posicionPotencial = new Vector3(
                transform.parent.position.x + UnityEngine.Random.Range(-3f, 3f),
                0.555f,
                transform.parent.position.z + UnityEngine.Random.Range(-3f, 3f));
            //en el caso de que tengamos mas cosas en el escenario checker que no choca
            Collider[] colliders = Physics.OverlapSphere(posicionPotencial, 0.5f);
            if (colliders.Length == 0)
            {
                transform.position = posicionPotencial;
                posicionEncontrada = true;
            }
        }
    }
}
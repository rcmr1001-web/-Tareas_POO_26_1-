using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlBola : MonoBehaviour
{
    public Transform CamaraPrincipal;
    public Rigidbody rb;
    public CamaraSeguimiento camaraSeguimiento;

    //Variables para apuntar y limitar
    public float velocidadDeApuntado = 5f;
    public float limiteIzquierdo = -2f;
    public float limiteDerecho = 2f;


    public float fuerzaDeLanzamiento = 1000f;

    private bool haSidoLanzada = false;
    //if aninado, controlan otros 

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Expresion:mientras que haSidoLanzada sea falso puedes disparar
        if (haSidoLanzada == false)
        {
            Apuntar();
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Lanzar();
            }
        }

    }
    void Apuntar()
    {
        //1.Leer un input Horizontal de tipo Axis, te pernite registrar
        //entradas con las teclas A y D y Flecha izquierda y Flecha derecha
        float inputHorizontal = Input.GetAxis("Horizontal");
        //2. Mover la bola hacia los lados
        transform.Translate(Vector3.right * inputHorizontal * velocidadDeApuntado * Time.deltaTime);
        //3. Delimitar el movimiento de la bola
        Vector3 posicionActual = transform.position;
        //transform.position me perimte saber cual es la posicion actual de la escena
        posicionActual.x = Mathf.Clamp(posicionActual.x, limiteIzquierdo, limiteDerecho);
        transform.position = posicionActual;//se coloca de nuevo pq se actualiza la posicion
    }

    void Lanzar()// empieza un metodo
    {
        haSidoLanzada = true;
        rb.AddForce(Vector3.forward * fuerzaDeLanzamiento);
        if (CamaraPrincipal != null)
        {
            CamaraPrincipal.SetParent(transform);
        }


        if (camaraSeguimiento != null) 
        {
            camaraSeguimiento.IniciarSeguimiento();
        }
    }

}//Bienvenido a la entrada al infierno
using UnityEngine;

// <summary>
// C�mara de seguimiento para una bola de boliche.
// - Suave en posici�n y rotaci�n (LateUpdate).
// - Evita atravesar objetos con raycast (clipping).
// - Zoom din�mico cuando la bola va r�pido.
// - Opci�n de comenzar seguimiento autom�ticamente por velocidad o por llamada p�blica.
// </summary>
public class CamaraSeguimiento : MonoBehaviour
{
    [Header("Objetivo")]
    [Tooltip("Transform de la bola. Si est� vac�o buscar� un GameObject con tag 'Ball'.")]
    public Transform objetivo;
    [Tooltip("Rigidbody de la bola (opcional). Se usa para detectar velocidad y direcci�n real).")]
    public Rigidbody rbObjetivo;

    [Header("Offset y suavizado")]
    [Tooltip("Offset relativo: x lateral, y altura, z distancia (negativo para atr�s).")]
    public Vector3 offset = new Vector3(0f, 3f, -7f);
    [Tooltip("Tiempo de suavizado para la posici�n (Vector3.SmoothDamp).")]
    public float suavizadoPosicion = 0.12f;
    [Tooltip("Factor de suavizado para rotaci�n (mayor = m�s r�pido en Slerp).")]
    public float suavizadoRotacion = 8f;

    [Header("Inicio del seguimiento")]
    [Tooltip("Si true: la c�mara comienza a seguir s�lo cuando la bola se mueve (o cuando llamas IniciarSeguimiento).")]
    public bool seguirAlMover = true;
    [Tooltip("Velocidad m�nima para considerar que la bola fue lanzada.")]
    public float velocidadParaSeguir = 0.5f;
    private bool sigue = false;

    [Header("Zoom din�mico")]
    [Tooltip("Multiplicador m�ximo de zoom hacia atr�s durante el lanzamiento.")]
    public float zoomLanzamiento = 1.25f;
    [Tooltip("Velocidad a la que cambia el zoom.")]
    public float velocidadZoom = 2f;
    private float zoomActual = 1f;

    [Header("Evitaci�n de clipping")]
    [Tooltip("Capas que la c�mara considerar� como obst�culos para evitar atravesarlas.")]
    public LayerMask capasObstaculos = ~0; // por defecto: todas las capas
    [Tooltip("Separaci�n desde el punto de impacto para evitar pegarse al collider.")]
    public float separacionObstaculo = 0.15f;

    Vector3 velocidadSuave;

    void Start()
    {
        // autocompletar referencias si no se asignaron en Inspector
        if (objetivo == null)
        {
            var go = GameObject.FindWithTag("Ball");
            if (go != null) objetivo = go.transform;
        }

        if (rbObjetivo == null && objetivo != null)
        {
            rbObjetivo = objetivo.GetComponent<Rigidbody>();
        }
    }

    void LateUpdate()
    {
        if (objetivo == null) return;

        // decidir si empezamos a seguir
        if (!sigue)
        {
            if (!seguirAlMover) sigue = true;
            else if (rbObjetivo != null && rbObjetivo.velocity.magnitude > velocidadParaSeguir) sigue = true;
        }

        if (!sigue) return;

        // direcci�n real de movimiento (si hay rigidbody usamos su velocidad, sino forward)
        Vector3 dirAdelante = objetivo.forward;
        if (rbObjetivo != null && rbObjetivo.velocity.sqrMagnitude > 0.001f)
            dirAdelante = rbObjetivo.velocity.normalized;

        // crear offset relativo: nos colocamos detr�s de la direcci�n de movimiento
        Vector3 desiredOffset = (-dirAdelante * Mathf.Abs(offset.z))
                                + (Vector3.up * offset.y)
                                + (objetivo.right * offset.x);

        Vector3 desiredPosition = objetivo.position + desiredOffset * zoomActual;

        // evitar clipping: linecast desde un punto de foco hacia la posici�n deseada
        Vector3 focus = objetivo.position + Vector3.up * 0.5f;
        RaycastHit hit;
        if (Physics.Linecast(focus, desiredPosition, out hit, capasObstaculos))
        {
            desiredPosition = hit.point + hit.normal * separacionObstaculo;
        }

        // suavizado de posici�n
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocidadSuave, suavizadoPosicion);

        // rotaci�n suave mirando ligeramente por encima del objetivo
        Vector3 lookPoint = objetivo.position + Vector3.up * 1.0f;
        Quaternion rotDeseada = Quaternion.LookRotation(lookPoint - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotDeseada, Time.deltaTime * suavizadoRotacion);

        // zoom din�mico seg�n rapidez de la bola
        float targetZoom = 1f;
        if (rbObjetivo != null)
        {
            float speed = rbObjetivo.velocity.magnitude;
            float t = Mathf.Clamp01((speed - velocidadParaSeguir) / (velocidadParaSeguir * 4f)); // escala suave
            targetZoom = Mathf.Lerp(1f, zoomLanzamiento, t);
        }
        zoomActual = Mathf.Lerp(zoomActual, targetZoom, Time.deltaTime * velocidadZoom);
    }

    /// <summary>Forzar inicio del seguimiento (llamar desde ControlBola al lanzar).</summary>
    public void IniciarSeguimiento()
    {
        sigue = true;
    }

    /// <summary>Detener seguimiento (por ejemplo en reinicio).</summary>
    public void DetenerSeguimiento()
    {
        sigue = false;
        zoomActual = 1f;
    }

    void OnDrawGizmosSelected()
    {
        if (objetivo == null) return;
        Gizmos.color = Color.cyan;
        Vector3 dirAdelante = objetivo.forward;
        Vector3 desiredOffset = (-dirAdelante * Mathf.Abs(offset.z)) + (Vector3.up * offset.y) + (objetivo.right * offset.x);
        Gizmos.DrawLine(objetivo.position, objetivo.position + desiredOffset);
        Gizmos.DrawSphere(objetivo.position + desiredOffset, 0.08f);
    }
}

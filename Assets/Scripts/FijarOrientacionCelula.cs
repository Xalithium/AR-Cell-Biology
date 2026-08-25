using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class FijarOrientacionCelula : MonoBehaviour
{
    public Vector3 direccionReferencia = Vector3.forward;
    const float SeparacionDelPlano = 0.05f;

    readonly List<ARRaycastHit> impactosPlano = new List<ARRaycastHit>();
    ARRaycastManager raycastManager;
    int dedoMovimientoActivo = -1;

    void Awake()
    {
        if (GetComponent<CellSelectionGate>() == null)
            gameObject.AddComponent<CellSelectionGate>();
    }

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Update()
    {
        CellLabelBuilder[] celulas = FindObjectsByType<CellLabelBuilder>();

        if (celulas.Length > 0)
        {
            // Ya hay una celula creada: bloquear el Object Spawner para que no se puedan crear mas.
            foreach (var comp in GetComponents<MonoBehaviour>())
            {
                string nombreComp = comp.GetType().Name;
                if (nombreComp == "ObjectSpawner" || nombreComp == "ARInteractorSpawnTrigger")
                    comp.enabled = false;
            }
        }

        foreach (var c in celulas)
        {
            PrepararInteraccion(c);

            if (c.GetComponent<OrientacionCorregidaMarcador>() != null)
                continue;

            Transform t = c.transform;
            Vector3 normal = t.up; 
            Vector3 referenciaProyectada = Vector3.ProjectOnPlane(direccionReferencia, normal);
            if (referenciaProyectada.sqrMagnitude < 0.001f)
                referenciaProyectada = Vector3.ProjectOnPlane(Vector3.right, normal);

            t.rotation = Quaternion.LookRotation(normal, referenciaProyectada);
            t.position += normal.normalized * SeparacionDelPlano;
            c.gameObject.AddComponent<OrientacionCorregidaMarcador>();
        }

        MoverCelulaSobrePlano(celulas);
    }

    void MoverCelulaSobrePlano(CellLabelBuilder[] celulas)
    {
        if (celulas.Length == 0)
            return;

        var toques = EnhancedTouch.activeTouches;
        if (toques.Count != 1)
        {
            dedoMovimientoActivo = -1;
            return;
        }

        EnhancedTouch toque = toques[0];
        if (toque.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            dedoMovimientoActivo = TocoCelula(toque.screenPosition) ? -1 : toque.touchId;
            return;
        }

        if (toque.touchId != dedoMovimientoActivo || toque.phase != UnityEngine.InputSystem.TouchPhase.Moved)
            return;

        if (raycastManager == null)
            raycastManager = FindAnyObjectByType<ARRaycastManager>();

        if (raycastManager == null || !raycastManager.Raycast(toque.screenPosition, impactosPlano, TrackableType.PlaneWithinPolygon))
            return;

        ARRaycastHit impacto = impactosPlano[0];
        Vector3 posicionSeparada = impacto.pose.position + impacto.pose.up * SeparacionDelPlano;
        celulas[0].transform.position = posicionSeparada;
    }

    bool TocoCelula(Vector2 posicionPantalla)
    {
        Camera camara = Camera.main;
        if (camara == null)
            return false;

        Ray rayo = camara.ScreenPointToRay(posicionPantalla);
        return Physics.Raycast(rayo, out RaycastHit impacto) && impacto.collider.GetComponentInParent<CellTouchManipulator>() != null;
    }

    void PrepararInteraccion(CellLabelBuilder celula)
    {
        SpriteRenderer spriteRenderer = celula.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        BoxCollider collider = celula.GetComponent<BoxCollider>();
        if (collider == null)
            collider = celula.gameObject.AddComponent<BoxCollider>();

        collider.center = spriteRenderer.sprite.bounds.center;
        collider.size = new Vector3(spriteRenderer.sprite.bounds.size.x, spriteRenderer.sprite.bounds.size.y, 0.2f);

        if (celula.GetComponent<CellTouchManipulator>() == null)
            celula.gameObject.AddComponent<CellTouchManipulator>();
    }

    public void ReiniciarCelula()
    {
        // Elimina la celula puesta (si existe) y vuelve a activar el spawner
        // para poder elegir y colocar una nueva celula.
        CellLabelBuilder[] celulasActuales = FindObjectsByType<CellLabelBuilder>();
        foreach (var c in celulasActuales)
        {
            if (c != null)
                Destroy(c.gameObject);
        }

        foreach (var comp in GetComponents<MonoBehaviour>())
        {
            string nombreComp = comp.GetType().Name;
            if (nombreComp == "ObjectSpawner" || nombreComp == "ARInteractorSpawnTrigger")
                comp.enabled = true;
        }

        GetComponent<CellSelectionGate>()?.ExigirEleccion();
    }
}

public class OrientacionCorregidaMarcador : MonoBehaviour { }

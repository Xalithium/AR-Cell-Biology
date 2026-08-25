using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

/// <summary>
/// Genera automáticamente las etiquetas (texto + línea guía) de los orgánulos
/// sobre un sprite de célula (animal o vegetal), en dos columnas (izquierda /
/// derecha) para que ninguna línea se cruce con otra.
///
/// Uso:
///  1) Agrega este componente al mismo GameObject que tiene el SpriteRenderer
///     de la célula.
///  2) En el Inspector, clic derecho sobre el componente y elegí
///     "Cargar datos (Célula Animal)" o "Cargar datos (Célula Vegetal)"
///     para completar la lista de orgánulos automáticamente.
///  3) Clic derecho de nuevo y elegí "Generar etiquetas".
///  4) Si algo no te gusta (posición, tamaño, color), ajustalo en el
///     Inspector y volvé a generar ("Borrar etiquetas" primero si ya existían).
///
/// Requiere el paquete TextMeshPro importado (Window > TextMeshPro > Import
/// TMP Essential Resources, Unity lo pide la primera vez que se usa).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class CellLabelBuilder : MonoBehaviour
{
    [System.Serializable]
    public class OrganelleLabel
    {
        public string nombre;
        [Range(0f, 1f)] public float normX; // 0 = borde izquierdo de la imagen, 1 = borde derecho
        [Range(0f, 1f)] public float normY; // 0 = borde superior de la imagen, 1 = borde inferior
    }

    [Header("Datos de orgánulos (posición normalizada sobre la imagen)")]
    public List<OrganelleLabel> organelos = new List<OrganelleLabel>();

    [Header("Apariencia de las etiquetas")]
    [Tooltip("Separación extra entre el borde de la célula y el texto, en unidades del mundo.")]
    public float margenColumna = 1.2f;
    public float tamañoTexto = 3.0f;
    public Color colorTexto = new Color(0.1f, 0.1f, 0.1f);
    public Color colorLinea = new Color(0.25f, 0.25f, 0.25f, 0.9f);
    public float grosorLinea = 0.03f;
    public string ordenamientoCapa = "Default";
    public int ordenDibujoTexto = 11;
    public int ordenDibujoLinea = 10;

    [HideInInspector] public List<GameObject> etiquetasGeneradas = new List<GameObject>();

    static Sprite spriteFondo;

    class Callout
    {
        public Vector3 puntoOrganeloLocal;
        public Vector3 puntoEtiquetaLocal;
        public Transform etiqueta;
        public LineRenderer conector;
    }

    readonly List<Callout> callouts = new List<Callout>();
    TMP_FontAsset fuenteCallouts;

    void Awake()
    {
        RehacerCallouts();
    }

    void LateUpdate()
    {
        Camera camara = Camera.main;
        if (camara == null || callouts.Count == 0)
            return;

        Vector3 haciaCamara = (camara.transform.position - transform.position).normalized;
        foreach (Callout callout in callouts)
        {
            Vector3 puntoOrganelo = transform.TransformPoint(callout.puntoOrganeloLocal) + haciaCamara * 0.008f;
            Vector3 puntoEtiqueta = transform.TransformPoint(callout.puntoEtiquetaLocal) + haciaCamara * 0.035f;

            callout.etiqueta.position = puntoEtiqueta;
            Vector3 direccion = puntoEtiqueta - camara.transform.position;
            if (direccion.sqrMagnitude > 0.0001f)
                callout.etiqueta.rotation = Quaternion.LookRotation(direccion, camara.transform.up);

            callout.conector.SetPosition(0, puntoOrganelo);
            callout.conector.SetPosition(1, puntoEtiqueta);
        }
    }

    void RehacerCallouts()
    {
        TextMeshPro etiquetaExistente = GetComponentInChildren<TextMeshPro>(true);
        fuenteCallouts = etiquetaExistente != null ? etiquetaExistente.font : TMP_Settings.defaultFontAsset;

        var hijosAntiguos = new List<Transform>();
        foreach (Transform hijo in transform)
            if (hijo.name.StartsWith("Etiqueta_") || hijo.name.StartsWith("Linea_") || hijo.name == "Callouts")
                hijosAntiguos.Add(hijo);

        foreach (Transform hijo in hijosAntiguos)
        {
            hijo.gameObject.SetActive(false);
            Destroy(hijo.gameObject);
        }

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        GameObject contenedor = new GameObject("Callouts");
        contenedor.transform.SetParent(transform, false);

        Bounds limites = spriteRenderer.sprite.bounds;
        CrearColumnaCallouts(organelos.Where(o => o.normX < 0.5f).OrderBy(o => o.normY).ToList(), true, limites, contenedor.transform);
        CrearColumnaCallouts(organelos.Where(o => o.normX >= 0.5f).OrderBy(o => o.normY).ToList(), false, limites, contenedor.transform);
    }

    void CrearColumnaCallouts(List<OrganelleLabel> lista, bool izquierda, Bounds limites, Transform contenedor)
    {
        for (int indice = 0; indice < lista.Count; indice++)
        {
            OrganelleLabel organelo = lista[indice];
            float proporcion = lista.Count > 1 ? (float)indice / (lista.Count - 1) : 0.5f;
            float x = izquierda ? -(limites.extents.x + margenColumna) : limites.extents.x + margenColumna;
            float y = Mathf.Lerp(limites.extents.y, -limites.extents.y, proporcion);
            CrearCallout(organelo.nombre, NormalizadoALocal(organelo.normX, organelo.normY, limites.extents.x, limites.extents.y), new Vector3(x, y, 0f), contenedor);
        }
    }

    void CrearCallout(string nombre, Vector3 puntoOrganeloLocal, Vector3 puntoEtiquetaLocal, Transform contenedor)
    {
        GameObject objetoEtiqueta = new GameObject("Etiqueta_" + nombre);
        objetoEtiqueta.transform.SetParent(contenedor, false);
        TextMeshPro texto = objetoEtiqueta.AddComponent<TextMeshPro>();
        texto.font = fuenteCallouts;
        texto.text = nombre;
        texto.fontSize = Mathf.Max(tamañoTexto, 7f);
        texto.fontStyle = FontStyles.Bold;
        texto.alignment = TextAlignmentOptions.Center;
        texto.color = Color.white;
        texto.outlineWidth = 0.14f;
        texto.outlineColor = new Color(0f, 0f, 0f, 0.95f);
        texto.ForceMeshUpdate();

        MeshRenderer textoRenderer = texto.GetComponent<MeshRenderer>();
        textoRenderer.sortingLayerName = ordenamientoCapa;
        textoRenderer.sortingOrder = 30;

        GameObject fondo = new GameObject("Fondo");
        fondo.transform.SetParent(objetoEtiqueta.transform, false);
        SpriteRenderer fondoRenderer = fondo.AddComponent<SpriteRenderer>();
        fondoRenderer.sprite = ObtenerSpriteFondo();
        fondoRenderer.color = new Color(0.03f, 0.06f, 0.12f, 0.82f);
        fondoRenderer.sortingLayerName = ordenamientoCapa;
        fondoRenderer.sortingOrder = 29;
        Vector2 tamañoTextoRenderizado = texto.GetRenderedValues(false);
        fondo.transform.localScale = new Vector3(tamañoTextoRenderizado.x + 0.35f, tamañoTextoRenderizado.y + 0.22f, 1f);

        GameObject objetoLinea = new GameObject("Linea_" + nombre);
        objetoLinea.transform.SetParent(contenedor, false);
        LineRenderer linea = objetoLinea.AddComponent<LineRenderer>();
        linea.useWorldSpace = true;
        linea.positionCount = 2;
        linea.startWidth = 0.0025f;
        linea.endWidth = 0.0025f;
        linea.alignment = LineAlignment.View;
        linea.startColor = linea.endColor = new Color(0.05f, 0.1f, 0.16f, 1f);
        Shader shaderLinea = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shaderLinea == null)
            shaderLinea = Shader.Find("Sprites/Default");
        linea.material = new Material(shaderLinea);
        linea.sortingLayerName = ordenamientoCapa;
        linea.sortingOrder = 28;

        callouts.Add(new Callout {
            puntoOrganeloLocal = puntoOrganeloLocal,
            puntoEtiquetaLocal = puntoEtiquetaLocal,
            etiqueta = objetoEtiqueta.transform,
            conector = linea,
        });
    }

    void RepararPunteros()
    {
        Shader shaderLinea = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shaderLinea == null)
            shaderLinea = Shader.Find("Sprites/Default");

        foreach (LineRenderer linea in GetComponentsInChildren<LineRenderer>(true))
        {
            if (shaderLinea != null)
                linea.material = new Material(shaderLinea);

            linea.startWidth = Mathf.Max(grosorLinea, 0.05f);
            linea.endWidth = Mathf.Max(grosorLinea, 0.05f);
            linea.sortingLayerName = ordenamientoCapa;
            linea.sortingOrder = ordenDibujoLinea;
        }
    }

    void MejorarEtiquetas()
    {
        foreach (TextMeshPro etiqueta in GetComponentsInChildren<TextMeshPro>(true))
        {
            etiqueta.fontSize = Mathf.Max(etiqueta.fontSize, 8f);
            etiqueta.fontStyle = FontStyles.Bold;
            etiqueta.outlineWidth = 0.12f;
            etiqueta.outlineColor = new Color(0f, 0f, 0f, 0.9f);
            ActualizarFondo(etiqueta);
        }
    }

    void ActualizarFondo(TextMeshPro etiqueta)
    {
        Transform fondo = etiqueta.transform.Find("FondoEtiqueta");
        if (fondo == null)
        {
            GameObject objetoFondo = new GameObject("FondoEtiqueta");
            objetoFondo.transform.SetParent(etiqueta.transform, false);
            var rendererFondo = objetoFondo.AddComponent<SpriteRenderer>();
            rendererFondo.sprite = ObtenerSpriteFondo();
            rendererFondo.color = new Color(0.02f, 0.04f, 0.08f, 0.72f);
            rendererFondo.sortingLayerName = ordenamientoCapa;
            rendererFondo.sortingOrder = ordenDibujoTexto - 1;
            fondo = objetoFondo.transform;
        }

        etiqueta.ForceMeshUpdate();
        Vector2 tamaño = etiqueta.GetRenderedValues(false);
        fondo.localScale = new Vector3(tamaño.x + 0.45f, tamaño.y + 0.25f, 1f);
    }

    static Sprite ObtenerSpriteFondo()
    {
        if (spriteFondo == null)
            spriteFondo = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);

        return spriteFondo;
    }

    // ---------------------------------------------------------------
    // Generación
    // ---------------------------------------------------------------

    [ContextMenu("Generar etiquetas")]
    public void GenerarEtiquetas()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogWarning("CellLabelBuilder: falta un SpriteRenderer con un sprite asignado.");
            return;
        }

        BorrarEtiquetas();

        Bounds b = sr.sprite.bounds; // espacio local del sprite (ya considera el pivote)
        float halfW = b.extents.x;
        float halfH = b.extents.y;

        var izquierda = organelos.Where(o => o.normX < 0.5f).OrderBy(o => o.normY).ToList();
        var derecha = organelos.Where(o => o.normX >= 0.5f).OrderBy(o => o.normY).ToList();

        ColocarColumna(izquierda, true, halfW, halfH);
        ColocarColumna(derecha, false, halfW, halfH);
    }

    void ColocarColumna(List<OrganelleLabel> lista, bool esIzquierda, float halfW, float halfH)
    {
        int n = lista.Count;
        for (int i = 0; i < n; i++)
        {
            var o = lista[i];
            Vector3 objetivo = NormalizadoALocal(o.normX, o.normY, halfW, halfH);

            float t = n > 1 ? (float)i / (n - 1) : 0.5f;
            float yEtiqueta = Mathf.Lerp(halfH, -halfH, t);
            float xEtiqueta = esIzquierda ? -(halfW + margenColumna) : (halfW + margenColumna);
            Vector3 posEtiqueta = new Vector3(xEtiqueta, yEtiqueta, 0f);

            CrearLinea(posEtiqueta, objetivo, o.nombre);
            CrearTexto(o.nombre, posEtiqueta, esIzquierda);
        }

        RepararPunteros();
        MejorarEtiquetas();
    }

    Vector3 NormalizadoALocal(float nx, float ny, float halfW, float halfH)
    {
        float x = Mathf.Lerp(-halfW, halfW, nx);
        float y = Mathf.Lerp(halfH, -halfH, ny); // se invierte: la imagen crece hacia abajo, Unity hacia arriba
        return new Vector3(x, y, 0f);
    }

    void CrearLinea(Vector3 desde, Vector3 hasta, string nombre)
    {
        GameObject go = new GameObject("Linea_" + nombre);
        go.transform.SetParent(transform, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = false;
        lr.positionCount = 2;
        lr.SetPosition(0, desde);
        lr.SetPosition(1, hasta);
        lr.startWidth = lr.endWidth = grosorLinea;
        Shader shaderLinea = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shaderLinea == null)
            shaderLinea = Shader.Find("Sprites/Default"); // por si el proyecto no usa URP
        lr.material = new Material(shaderLinea);
        lr.startColor = lr.endColor = colorLinea;
        lr.sortingLayerName = ordenamientoCapa;
        lr.sortingOrder = ordenDibujoLinea;

        etiquetasGeneradas.Add(go);
    }

    void CrearTexto(string texto, Vector3 posicion, bool esIzquierda)
    {
        GameObject go = new GameObject("Etiqueta_" + texto);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = posicion;

        var tmp = go.AddComponent<TextMeshPro>();
        tmp.text = texto;
        tmp.fontSize = tamañoTexto;
        tmp.color = colorTexto;
        tmp.alignment = esIzquierda ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;
        tmp.fontStyle = FontStyles.Bold;

        var mr = go.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingLayerName = ordenamientoCapa;
            mr.sortingOrder = ordenDibujoTexto;
        }

        etiquetasGeneradas.Add(go);
    }

    [ContextMenu("Borrar etiquetas")]
    public void BorrarEtiquetas()
    {
        foreach (var g in etiquetasGeneradas)
        {
            if (g == null) continue;
            if (Application.isPlaying) Destroy(g);
            else DestroyImmediate(g);
        }
        etiquetasGeneradas.Clear();

        // Por si quedaron etiquetas de una generación anterior a la que se
        // perdió la referencia (por ejemplo, tras recargar el script).
        var hijos = new List<Transform>();
        foreach (Transform hijo in transform)
            if (hijo.name.StartsWith("Etiqueta_") || hijo.name.StartsWith("Linea_"))
                hijos.Add(hijo);
        foreach (var hijo in hijos)
        {
            if (Application.isPlaying) Destroy(hijo.gameObject);
            else DestroyImmediate(hijo.gameObject);
        }
    }

    // ---------------------------------------------------------------
    // Datos precargados (coordenadas ya identificadas sobre las imágenes
    // de brgfx / Freepik usadas en el proyecto)
    // ---------------------------------------------------------------

    [ContextMenu("Cargar datos (Célula Animal)")]
    public void CargarDatosAnimal()
    {
        organelos = new List<OrganelleLabel> {
            Dato("Membrana plasmática", 40, 1160, 2098, 2400),
            Dato("Mitocondria",          340, 1350, 2098, 2400),
            Dato("Lisosoma",             250, 950,  2098, 2400),
            Dato("Retículo endoplasmático", 760, 650, 2098, 2400),
            Dato("Centríolos",           650, 1420, 2098, 2400),
            Dato("Núcleo",               1080, 750, 2098, 2400),
            Dato("Nucléolo",             1130, 820, 2098, 2400),
            Dato("Microtúbulos",         1650, 450, 2098, 2400),
            Dato("Aparato de Golgi",     950, 1850, 2098, 2400),
        };
    }

    [ContextMenu("Cargar datos (Célula Vegetal)")]
    public void CargarDatosVegetal()
    {
        organelos = new List<OrganelleLabel> {
            Dato("Pared celular",           20, 300,   1964, 2400),
            Dato("Membrana plasmática",     60, 1950,  1964, 2400),
            Dato("Citoplasma",              700, 110,  1964, 2400),
            Dato("Aparato de Golgi",        330, 600,  1964, 2400),
            Dato("Vacuola",                 750, 1100, 1964, 2400),
            Dato("Núcleo",                  1080, 480, 1964, 2400),
            Dato("Nucléolo",                1130, 500, 1964, 2400),
            Dato("Retículo endoplasmático", 1000, 350, 1964, 2400),
            Dato("Mitocondria",             1450, 1250,1964, 2400),
            Dato("Cloroplasto",             1450, 1600,1964, 2400),
            Dato("Peroxisoma",              1130, 2000,1964, 2400),
        };
    }

    static OrganelleLabel Dato(string nombre, float px, float py, float imgW, float imgH)
    {
        return new OrganelleLabel { nombre = nombre, normX = px / imgW, normY = py / imgH };
    }
}

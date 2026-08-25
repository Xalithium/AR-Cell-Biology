using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using UnityEngine.XR.Templates.AR;

public class CellSelectionGate : MonoBehaviour
{
    ObjectSpawner spawner;
    ARTemplateMenuManager menu;
    Button botonEliminar;
    Button botonOpciones;
    GoalManager instruccionesIniciales;
    bool esperandoEleccion;
    bool tarjetasConfiguradas;

    void Start()
    {
        spawner = GetComponent<ObjectSpawner>();
        menu = FindAnyObjectByType<ARTemplateMenuManager>();
        botonEliminar = BuscarBotonEliminar();
        ConfigurarTarjetasInstrucciones();
        ExigirEleccion();
    }

    void Update()
    {
        if (!esperandoEleccion || spawner == null || spawner.spawnOptionIndex < 0)
            return;

        esperandoEleccion = false;
        HabilitarColocacion(true);
        Debug.Log("[AR Cell Biology] Tipo de célula seleccionado. Toca un plano para colocarla.");
    }

    void LateUpdate()
    {
        ConfigurarTarjetasInstrucciones();
        OcultarControlesDepuracion();

        if (botonEliminar == null)
            botonEliminar = BuscarBotonEliminar();

        if (botonEliminar == null)
            return;

        // La plantilla oculta este botón si no hay foco XR. Nuestra célula se
        // maneja por toque, así que lo dejamos visible mientras exista una.
        botonEliminar.gameObject.SetActive(FindAnyObjectByType<CellLabelBuilder>() != null);
    }

    public void ExigirEleccion()
    {
        if (spawner == null)
            spawner = GetComponent<ObjectSpawner>();

        if (menu == null)
            menu = FindAnyObjectByType<ARTemplateMenuManager>();

        esperandoEleccion = true;
        if (spawner != null)
            spawner.spawnOptionIndex = -1;

        HabilitarColocacion(false);

        if (menu != null)
        {
            menu.objectMenu.SetActive(true);
            menu.objectMenuAnimator.SetBool("Show", true);
        }
    }

    void HabilitarColocacion(bool habilitar)
    {
        foreach (MonoBehaviour componente in GetComponents<MonoBehaviour>())
        {
            string nombre = componente.GetType().Name;
            if (nombre == "ObjectSpawner" || nombre == "ARInteractorSpawnTrigger")
                componente.enabled = habilitar;
        }
    }

    Button BuscarBotonEliminar()
    {
        foreach (Button boton in Resources.FindObjectsOfTypeAll<Button>())
        {
            if (boton.name == "Delete Button" && boton.gameObject.scene.IsValid())
                return boton;
        }

        return null;
    }

    void ConfigurarTarjetasInstrucciones()
    {
        if (tarjetasConfiguradas)
            return;

        if (instruccionesIniciales == null)
            instruccionesIniciales = FindAnyObjectByType<GoalManager>();

        if (instruccionesIniciales == null)
            return;

        foreach (GoalManager.Step paso in instruccionesIniciales.stepList)
        {
            if (paso.stepObject != null && paso.stepObject.GetComponent<InstructionCardAdvance>() == null)
                paso.stepObject.AddComponent<InstructionCardAdvance>();
        }

        tarjetasConfiguradas = true;
    }

    void OcultarControlesDepuracion()
    {
        if (botonOpciones == null)
        {
            foreach (Button boton in Resources.FindObjectsOfTypeAll<Button>())
            {
                if (boton.name == "Options Button" && boton.gameObject.scene.IsValid())
                {
                    botonOpciones = boton;
                    break;
                }
            }
        }

        if (botonOpciones != null)
            botonOpciones.gameObject.SetActive(false);

        if (menu != null && menu.modalMenu != null)
            menu.modalMenu.SetActive(false);
    }
}

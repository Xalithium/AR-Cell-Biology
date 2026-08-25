# AR Cell Biology

Proyecto de realidad aumentada desarrollado para la actividad evaluativa **800TV06 / EPE1** del curso **Taller de Programación de Videojuegos**.

| Información | Detalle |
| --- | --- |
| Alumno | Gonzalo Tapia |
| Asignatura | Taller de Programación de Videojuegos |
| Plataforma | Android con realidad aumentada |
| Motor | Unity 6.5 (`6000.5.0f1`) |

## Objetivo

**AR Cell Biology** permite explorar una célula vegetal o animal en realidad aumentada desde un dispositivo móvil. El usuario elige el tipo de célula, la coloca sobre una superficie detectada y puede rotarla, moverla o cambiar su tamaño para observar sus estructuras.

La célula vegetal incluye etiquetas con punteros para identificar sus principales orgánulos: pared celular, membrana plasmática, citoplasma, vacuola, núcleo, nucléolo, retículo endoplasmático, mitocondria, cloroplasto, peroxisoma y aparato de Golgi.

## Requisitos

- Unity Hub.
- Unity 6.5, versión `6000.5.0f1`.
- Un teléfono Android compatible con **ARCore** para probar realidad aumentada.
- Cable USB y modo desarrollador activado en el teléfono para usar **Build And Run**.

> También es posible probar la escena dentro del Editor mediante la simulación XR de Unity.

## Cómo abrir y configurar el proyecto

1. Abre Unity Hub y selecciona **Open**.
2. Elige la carpeta raíz de este repositorio: `AR Cell Biology`.
3. Abre el proyecto con Unity `6000.5.0f1` y espera a que termine la importación inicial.
4. Abre la escena `Assets/Scenes/SampleScene.unity` si no se carga automáticamente.
5. Para probar en el Editor, presiona **Play**.
6. Para probar en el teléfono, selecciona Android como plataforma y usa **Build And Run** con el dispositivo conectado.

La configuración de AR Foundation y ARCore ya está incluida en el proyecto.

## Uso de la aplicación

1. Presiona **Continuar** en la pantalla de bienvenida.
2. Elige **célula vegetal** o **célula animal**.
3. Apunta la cámara a una superficie y tócala para colocar la célula.
4. Interactúa con el modelo:

| Acción | Gesto |
| --- | --- |
| Rotar la célula | Arrastrar sobre la célula con un dedo |
| Cambiar tamaño | Juntar o separar dos dedos sobre la célula |
| Mover la célula | Arrastrar con un dedo sobre un plano vacío |
| Eliminar y elegir otra célula | Tocar el botón de papelera |

Las etiquetas de la célula vegetal se mantienen visibles y orientadas hacia la cámara, facilitando la identificación de cada estructura.

## Estructura principal

```text
Assets/
├── Prefabs/       Prefabs de célula animal y vegetal
├── Scenes/        Escena principal de realidad aumentada
├── Scripts/       Colocación, gestos, etiquetas e interfaz
└── Sprites/       Ilustraciones de células e ícono de la aplicación
```

## Depuración y pruebas

Los mensajes de funcionamiento se pueden revisar en la ventana **Console** de Unity. Durante las pruebas se debe verificar la detección de superficies, colocación, rotación, escalado, movimiento, eliminación y selección de una nueva célula.

## Créditos de recursos

Este proyecto utiliza ilustraciones obtenidas desde internet y un ícono creado con inteligencia artificial. La procedencia, enlaces originales y declaraciones de edición están en [CREDITOS.md](CREDITOS.md).

## Entrega

Para entregar el proyecto, se debe comprimir la carpeta excluyendo directorios regenerables como `Library`, `Temp` y `Logs`. Deben incluirse `Assets`, `Packages`, `ProjectSettings`, este `README.md` y `CREDITOS.md`.

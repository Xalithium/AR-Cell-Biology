using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(Collider))]
public class CellTouchManipulator : MonoBehaviour
{
    [SerializeField] float minimumScale = 0.03f;
    [SerializeField] float maximumScale = 0.15f;
    [SerializeField] float rotationDegreesPerPixel = 0.25f;

    Camera arCamera;
    int activeFingerId = -1;

    void Awake()
    {
        arCamera = Camera.main;
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
        var touches = EnhancedTouch.activeTouches;
        if (touches.Count == 2)
        {
            ApplyPinchScale(touches[0], touches[1]);
            activeFingerId = -1;
            return;
        }

        if (touches.Count != 1)
        {
            activeFingerId = -1;
            return;
        }

        EnhancedTouch touch = touches[0];
        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
        {
            activeFingerId = WasTouched(touch.screenPosition) ? touch.touchId : -1;
            if (activeFingerId == touch.touchId)
                Debug.Log("[AR Cell Biology] Célula seleccionada.");
            return;
        }

        if (touch.touchId == activeFingerId && touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
        {
            transform.Rotate(transform.forward, CellGestureMath.RotationFromDrag(touch.delta.x, rotationDegreesPerPixel), Space.World);
        }

        if (touch.phase is UnityEngine.InputSystem.TouchPhase.Ended or UnityEngine.InputSystem.TouchPhase.Canceled)
            activeFingerId = -1;
    }

    bool WasTouched(Vector2 screenPosition)
    {
        if (arCamera == null)
            arCamera = Camera.main;

        if (arCamera == null)
            return false;

        Ray ray = arCamera.ScreenPointToRay(screenPosition);
        return Physics.Raycast(ray, out RaycastHit hit) && hit.collider.GetComponentInParent<CellTouchManipulator>() == this;
    }

    void ApplyPinchScale(EnhancedTouch firstTouch, EnhancedTouch secondTouch)
    {
        Vector2 previousFirstPosition = firstTouch.screenPosition - firstTouch.delta;
        Vector2 previousSecondPosition = secondTouch.screenPosition - secondTouch.delta;
        float previousDistance = Vector2.Distance(previousFirstPosition, previousSecondPosition);
        float currentDistance = Vector2.Distance(firstTouch.screenPosition, secondTouch.screenPosition);
        float nextScale = CellGestureMath.NextScale(transform.localScale.x, previousDistance, currentDistance, minimumScale, maximumScale);

        transform.localScale = Vector3.one * nextScale;
    }
}

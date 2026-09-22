using UnityEngine;
using UnityEngine.InputSystem;

namespace KingdomRuins
{
    /// <summary>
    /// Camera top-down cuon tu do cho Kingdom View (giong Super Fantasy Kingdom):
    /// keo chuot giua / WASD de di chuyen, con lan de zoom, gioi han trong vung map.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class KingdomCameraController : MonoBehaviour
    {
        [Header("Di chuyen")]
        [SerializeField] private float keyboardSpeed = 8f;
        [SerializeField] private float dragSpeed = 1f;

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float minZoom = 3f;
        [SerializeField] private float maxZoom = 14f;

        [Header("Gioi han map (world units)")]
        [SerializeField] private Vector2 boundsMin = new Vector2(-20f, -20f);
        [SerializeField] private Vector2 boundsMax = new Vector2(40f, 40f);

        private Camera cam;
        private Vector3 dragOrigin;
        private bool dragging;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (!cam.orthographic) cam.orthographic = true;
        }

        private void Update()
        {
            // Chi dieu khien khi dang o pha Kingdom
            var gm = GameManager.Instance;
            if (gm != null && gm.Phase == GamePhase.Battle) return;

            HandleKeyboard();
            HandleDrag();
            HandleZoom();
            ClampPosition();
        }

        private void HandleKeyboard()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            Vector2 dir = Vector2.zero;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) dir.y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) dir.y -= 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) dir.x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) dir.x += 1f;

            if (dir == Vector2.zero) return;

            transform.position += (Vector3)(dir.normalized * keyboardSpeed * Time.unscaledDeltaTime);
        }

        private void HandleDrag()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            // Chuot giua de keo map, tranh xung dot voi click dat cong trinh
            if (mouse.middleButton.wasPressedThisFrame)
            {
                dragOrigin = cam.ScreenToWorldPoint(mouse.position.ReadValue());
                dragging = true;
            }
            if (mouse.middleButton.wasReleasedThisFrame) dragging = false;

            if (!dragging) return;

            Vector3 current = cam.ScreenToWorldPoint(mouse.position.ReadValue());
            Vector3 delta = dragOrigin - current;
            transform.position += new Vector3(delta.x, delta.y, 0f) * dragSpeed;
        }

        private void HandleZoom()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Approximately(scroll, 0f)) return;

            cam.orthographicSize = Mathf.Clamp(
                cam.orthographicSize - Mathf.Sign(scroll) * zoomSpeed,
                minZoom, maxZoom);
        }

        private void ClampPosition()
        {
            Vector3 p = transform.position;
            p.x = Mathf.Clamp(p.x, boundsMin.x, boundsMax.x);
            p.y = Mathf.Clamp(p.y, boundsMin.y, boundsMax.y);
            transform.position = p;
        }
    }
}

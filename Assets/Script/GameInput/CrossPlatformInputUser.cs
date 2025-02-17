using UnityEngine;
using UnityEngine.InputSystem;

namespace GameInput
{
    public enum InputButton
    {
        Primary,   // Left mouse on PC, first touch on mobile
        Secondary  // Right mouse on PC, second touch on mobile
    }

    public class CrossPlatformInputUser : MonoBehaviour
    {
        private InputActions _inputActions;
        public Vector2 PointerPosition { get; private set; }
        public Vector2 PointerWorldPosition => Camera.main.ScreenToWorldPoint(PointerPosition);

        private bool _isPrimaryPressed;
        private bool _isSecondaryPressed;
        private Touch[] _touches;

        private void Awake()
        {
            _inputActions = new InputActions();
            _inputActions.Enable();  
        }

        private void OnEnable()
        {
            _inputActions = new InputActions();
            _inputActions.Enable();

            // Subscribe to mouse/touch position events
            _inputActions.Game.PointerPosition.performed += OnPointerPositionPerformed;

            // Subscribe to primary action (left click/first touch)
            _inputActions.Game.PerformAction.performed += OnPrimaryActionPerformed;
            _inputActions.Game.PerformAction.canceled += OnPrimaryActionCanceled;

            // Subscribe to secondary action (right click/second touch)
            _inputActions.Game.CancelAction.performed += OnSecondaryActionPerformed;
            _inputActions.Game.CancelAction.canceled += OnSecondaryActionCanceled;
        }

        private void OnDisable()
        {
            _inputActions.Game.PointerPosition.performed -= OnPointerPositionPerformed;
            _inputActions.Game.PerformAction.performed -= OnPrimaryActionPerformed;
            _inputActions.Game.PerformAction.canceled -= OnPrimaryActionCanceled;
            _inputActions.Game.CancelAction.performed -= OnSecondaryActionPerformed;
            _inputActions.Game.CancelAction.canceled -= OnSecondaryActionCanceled;
        }


        private void Update()
        {
            if (Application.isMobilePlatform)
            {
                _touches = Input.touches;
                HandleTouchInput();
            }
        }

        public void HandleTouchInput()
        {
            if (_touches.Length > 0)
            {
                // Update pointer position to first touch position
                PointerPosition = _touches[0].position;
                _isPrimaryPressed = true;

                // If there's a second touch, treat it as secondary input
                if (_touches.Length > 1)
                {
                    _isSecondaryPressed = true;
                }
                else
                {
                    _isSecondaryPressed = false;
                }
            }
            else
            {
                _isPrimaryPressed = false;
                _isSecondaryPressed = false;
            }
        }

        private void OnPointerPositionPerformed(InputAction.CallbackContext ctx)
        {
            if (!Application.isMobilePlatform)
            {
                PointerPosition = ctx.ReadValue<Vector2>();
            }
        }
        private void OnPrimaryActionPerformed(InputAction.CallbackContext ctx)
        {
            if (!Application.isMobilePlatform)
            {
                _isPrimaryPressed = true;
            }
        }

        private void OnPrimaryActionCanceled(InputAction.CallbackContext ctx)
        {
            if (!Application.isMobilePlatform)
            {
                _isPrimaryPressed = false;
            }
        }

        private void OnSecondaryActionPerformed(InputAction.CallbackContext ctx)
        {
            if (!Application.isMobilePlatform)
            {
                _isSecondaryPressed = true;
            }
        }

        private void OnSecondaryActionCanceled(InputAction.CallbackContext ctx)
        {
            if (!Application.isMobilePlatform)
            {
                _isSecondaryPressed = false;
            }
        }

        public bool IsInputButtonPressed(InputButton button)
        {
            return button == InputButton.Primary ? _isPrimaryPressed : _isSecondaryPressed;
        }
    }
}
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;

        public bool jump;
        public bool sprint;

        // Controle da rotação da câmera pelo teclado
        // -1 = esquerda
        //  1 = direita
        public float cameraRotate;


        [Header("Movement Settings")]
        public bool analogMovement = true;


        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;


        // ============================================================
        // MOVIMENTO
        // ============================================================

        public void OnMove(InputValue value)
        {
            move = value.Get<Vector2>();
        }


        // ============================================================
        // OLHAR
        // ============================================================

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                look = value.Get<Vector2>();
            }
        }


        // ============================================================
        // ROTAÇÃO DA CÂMERA
        // ============================================================

        public void OnCameraRotate(InputValue value)
        {
            cameraRotate = value.Get<float>();
        }


        // ============================================================
        // PULO
        // ============================================================

        public void OnJump(InputValue value)
        {
            jump = value.isPressed;
        }


        // ============================================================
        // SPRINT
        // ============================================================

        public void OnSprint(InputValue value)
        {
            sprint = value.isPressed;
        }


        // ============================================================
        // CURSOR
        // ============================================================

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }


        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState
                ? CursorLockMode.Locked
                : CursorLockMode.None;
        }
    }
}

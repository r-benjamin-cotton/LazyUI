using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace LazyUI
{
    public partial class @InputActions : IInputActionCollection2, IDisposable
    {
        private static readonly @InputActions actions = new();
        private static int activateCount = 0;

        public static @InputActions Actions => actions;

        public static void Activate()
        {
            if (activateCount++ == 0)
            {
                actions.Enable();
            }
        }
        public static void Deactivate()
        {
            if (--activateCount == 0)
            {
                actions.Disable();
            }
        }
        public static InputAction Up => actions.UI.Up;
        public static InputAction Down => actions.UI.Down;
    }
}

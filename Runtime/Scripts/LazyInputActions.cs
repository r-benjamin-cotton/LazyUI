

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LazyUI
{

    public class LazyInputActions : MonoBehaviour
    {
        [SerializeField]
        private InputActionReference pageUpAction;
        [SerializeField]
        private InputActionReference pageDownAction;
        [SerializeField]
        private InputActionReference shiftAction;
        [SerializeField]
        private bool activateOnEnable = true;

        private static readonly List<LazyInputActions> activeList = new();
        private bool registerd = false;
        private bool activated = false;

        private static LazyInputActions Active
        {
            get
            {
                return activeList.LastOrDefault();
            }
        }
        public static InputAction PageUp => Active?.pageUpAction;
        public static InputAction PageDown => Active?.pageDownAction;
        public static InputAction Shift => Active?.shiftAction;


        private static void EnableInputAction(InputActionReference inputActionReference)
        {
            if (inputActionReference == null)
            {
                return;
            }
            var action = inputActionReference.action;
            if (action == null)
            {
                return;
            }
            action.Enable();
        }
        private static void DisableInputAction(InputActionReference inputActionReference)
        {
            if (inputActionReference == null)
            {
                return;
            }
            var action = inputActionReference.action;
            if (action == null)
            {
                return;
            }
            action.Disable();
        }
        private void EnableInputActions()
        {
            if (registerd)
            {
                return;
            }
            EnableInputAction(pageUpAction);
            EnableInputAction(pageDownAction);
            EnableInputAction(shiftAction);
            registerd = true;
        }
        private void DisableInputActions()
        {
            if (!registerd)
            {
                return;
            }
            DisableInputAction(pageUpAction);
            DisableInputAction(pageDownAction);
            DisableInputAction(shiftAction);
            registerd = false;
        }
        public void Activate()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
            var active = Active;
            if (ReferenceEquals(active, this))
            {
                return;
            }
            if (active != null)
            {
                active.DisableInputActions();
            }
            if (activated)
            {
                activeList.Remove(this);
            }
            {
                activeList.Add(this);
                activated = true;
                active = this;
            }
            active.EnableInputActions();
        }
        public void Deactivate()
        {
            if (!activated)
            {
                return;
            }
            var active = Active;
            {
                activeList.Remove(this);
                activated = false;
            }
            if (ReferenceEquals(active, this))
            {
                {
                    active.DisableInputActions();
                }
                active = Active;
                if (active != null)
                {
                    active.EnableInputActions();
                }
            }
        }
        private void OnEnable()
        {
            if (activateOnEnable)
            {
                Activate();
            }
        }
        private void OnDisable()
        {
            {
                Deactivate();
            }
        }
        private void Reset()
        {
            var defaultActions = new LazyUIInputActions();
            pageUpAction = InputActionReference.Create(defaultActions.LazyUI.PageUp);
            pageDownAction = InputActionReference.Create(defaultActions.LazyUI.PageDown);
            shiftAction = InputActionReference.Create(defaultActions.LazyUI.Shift);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LazyUI
{
    /// <summary>
    /// RangeSlider用のハンドル
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PropertyRangeSliderHandle : Selectable, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler
    {
        public enum HandleType
        {
            LowerHandle,
            UpperHandle,
        }

        [SerializeField]
        private PropertyRangeSlider owner = null;

        [SerializeField]
        private RectTransform handleRect = null;

        [SerializeField]
        private HandleType handle = HandleType.LowerHandle;

        private LazyRange<float> currentValue = default;

        private bool started = false;
        private bool active = false;
        private RectTransform handleParentRect = null;

        private bool dragging = false;
        private Vector2 dragPoint = default;
        private float dragValue = 0.0f;

        public override bool IsInteractable()
        {
            if (owner == null)
            {
                return false;
            }
            return owner.IsInteractable();
        }

        private void UpdateState(bool force)
        {
            if (owner == null)
            {
                return;
            }
            if (force || (currentValue != owner.NormalizedValue))
            {
                UpdateVisuals();
            }
            var ac = IsInteractable();
            if (active != ac)
            {
                active = ac;
                OnDidApplyAnimationProperties();
            }
        }

        private bool Reverse
        {
            get
            {
                var direction = owner.Direction;
                return (direction == PropertyRangeSlider.DirectionType.RightToLeft) || (direction == PropertyRangeSlider.DirectionType.TopToBottom);
            }
        }
        private int Axis
        {
            get
            {
                var direction = owner.Direction;
                return ((direction == PropertyRangeSlider.DirectionType.BottomToTop) || (direction == PropertyRangeSlider.DirectionType.TopToBottom)) ? 1 : 0;
            }
        }

        private void AddValue(float step)
        {
            var v = owner.Value;
            var min = v.MinValue;
            var max = v.MaxValue;
            if (handle == HandleType.LowerHandle)
            {
                min += step;
            }
            else
            {
                max += step;
            }
            owner.Value = new LazyRange<float>(min, max);
            UpdateState(false);
        }
        private void UpdateInput()
        {
            if (!IsActive() || !IsInteractable() || (currentSelectionState != SelectionState.Selected))
            {
                return;
            }
            var step = owner.StepValue;
            if (InputActions.Up.WasPressedThisFrame())
            {
                AddValue(Reverse ? -step : +step);
            }
            if (InputActions.Down.WasPressedThisFrame())
            {
                AddValue(Reverse ? +step : -step);
            }
        }
        private void UpdateVisuals()
        {
            if (owner == null)
            {
                return;
            }
            currentValue = owner.NormalizedValue;
            if (handleRect != null)
            {
                var anchorMin = handleRect.anchorMin;
                var anchorMax = handleRect.anchorMax;
                var value = (handle == HandleType.LowerHandle) ? currentValue.MinValue : currentValue.MaxValue;
                var minMax = Reverse ? (1 - value) : value;
                var axis = Axis;
                anchorMin[axis] = minMax;
                anchorMax[axis] = minMax;
                handleRect.anchorMin = anchorMin;
                handleRect.anchorMax = anchorMax;
            }
        }
        private bool MayDrag(PointerEventData eventData)
        {
            return (eventData.button == PointerEventData.InputButton.Left) && IsActive() && IsInteractable();
        }
        private void UpdateDrag(PointerEventData eventData)
        {
            if (!dragging)
            {
                return;
            }
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(handleParentRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                var axis = Axis;
                var rect = handleParentRect.rect;
                var size = rect.size[axis];
                if (size > 0)
                {
                    var dx = (localPoint - dragPoint) / size;
                    var d0 = dx[axis];
                    if (Reverse)
                    {
                        d0 = 1 - d0;
                    }
                    var d1 = dx[axis ^ 1];
                    var vv = PropertySlider.CalcDragValue(d0, d1, dragValue);
                    if (dragValue != vv)
                    {
                        dragValue = vv;
                        dragPoint[axis] = localPoint[axis];
                    }
                    var nv = owner.NormalizedValue;
                    if (handle == HandleType.LowerHandle)
                    {
                        var upper = Mathf.Max(nv.MaxValue, vv);
                        owner.NormalizedValue = new LazyRange<float>(vv, upper);
                    }
                    else
                    {
                        var lower = Mathf.Min(nv.MinValue, vv);
                        owner.NormalizedValue = new LazyRange<float>(lower, vv);
                    }
                }
            }
        }
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            dragging = false;
            if (!MayDrag(eventData))
            {
                return;
            }
            if (handleParentRect != null)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(handleRect, eventData.position, eventData.pressEventCamera))
                {
                    dragging = true;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(handleParentRect, eventData.position, eventData.pressEventCamera, out dragPoint);
                }
            }
        }
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            var nv = owner.NormalizedValue;
            if (handle == HandleType.LowerHandle)
            {
                dragValue = nv.MinValue;
            }
            else
            {
                dragValue = nv.MaxValue;
            }
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
            {
                return;
            }
            UpdateDrag(eventData);
        }

        void IInitializePotentialDragHandler.OnInitializePotentialDrag(PointerEventData eventData)
        {
            eventData.useDragThreshold = false;
        }
        private void SetupCache()
        {
            handleParentRect = (handleRect == null) ? null : (handleRect.parent as RectTransform);
        }
        private void UpdateState()
        {
            UpdateInput();
            UpdateState(false);
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            InputActions.Activate();
            SetupCache();
            LazyCallbacker.RegisterCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateState);
            if (started)
            {
                UpdateState(true);
            }
        }
        protected override void Start()
        {
            base.Start();
            started = true;
            {
                UpdateState(true);
            }
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            LazyCallbacker.RemoveCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateState);
            InputActions.Deactivate();
        }
#if UNITY_EDITOR
        private void DelayedUpdate()
        {
            if ((this == null) || !isActiveAndEnabled)
            {
                return;
            }
            SetupCache();
            UpdateState(true);
        }
        protected override void OnValidate()
        {
            base.OnValidate();
            UnityEditor.EditorApplication.delayCall += () => DelayedUpdate();
        }
#endif
    }
}

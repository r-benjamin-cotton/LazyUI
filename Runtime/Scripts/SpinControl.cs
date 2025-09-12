using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LazyUI
{
    [RequireComponent(typeof(RectTransform))]
    public class SpinControl : Selectable, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        public enum DirectionType
        {
            LeftToRight,
            RightToLeft,
            BottomToTop,
            TopToBottom,
        }

        [SerializeField]
        private DirectionType direction = DirectionType.BottomToTop;

        [SerializeField]
        private bool quickButton = false;

        [SerializeField]
        private bool reverseScroll = false;

        [SerializeField]
        private float repeatDelay = 0.0f;

        [SerializeField]
        private float repeatInterval = 0.0f;

        [SerializeField]
        private float gap = 0.2f;

        [SerializeField]
        private float dragSensitivity = 2;

        [SerializeField]
        private PropertySlider targetSlider = null;

        [SerializeField]
        private Selector targetSelector = null;

        private bool started = false;
        private RectTransform rectTransform = null;
        private DirectionType currentDirection = DirectionType.BottomToTop;
        private readonly Vector2[] scrollDir = { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };

        private int clickDelta = 0;
        private bool dragging = false;
        private int buttonDown = -1;
        private float repeat = 0;
        private int clickCount = 0;
        private Vector2 downPoint = default;
        private float sliderValue = 0;
        private float scrollPos = 0;
        private float scrollDelta = 0;
        private int dragDelta = 0;

        public RectTransform RectTransform => rectTransform;

        public DirectionType Direction
        {
            get
            {
                return direction;
            }
            set
            {
                direction = value;
                UpdateDirection();
            }
        }
        public bool Reverse
        {
            get
            {
                return (currentDirection == DirectionType.RightToLeft) || (currentDirection == DirectionType.TopToBottom);
            }
        }
        public int Axis
        {
            get
            {
                return ((currentDirection == DirectionType.BottomToTop) || (currentDirection == DirectionType.TopToBottom)) ? 1 : 0;
            }
        }
        public float RepeatDelay
        {
            get { return repeatDelay; }
            set { repeatDelay = value; }
        }
        public float RepeatInterval
        {
            get { return repeatInterval; }
            set { repeatInterval = value; }
        }

        public override bool IsInteractable()
        {
            return base.IsInteractable();
        }

        public void SpinUp()
        {
            if (!IsInteractable())
            {
                return;
            }
            if (targetSlider != null)
            {
                targetSlider.StepUp();
            }
            if (targetSelector != null)
            {
                targetSelector.Up();
            }
            OnSpinUp();
        }
        public void SpinDown()
        {
            if (!IsInteractable())
            {
                return;
            }
            if (targetSlider != null)
            {
                targetSlider.StepDown();
            }
            if (targetSelector != null)
            {
                targetSelector.Down();
            }
            OnSpinDown();
        }
        private void Click(int dt)
        {
            clickCount++;
            //LazyDebug.Log($"{clickCount} {dt}");
            while (dt > 0)
            {
                dt--;
                SpinUp();
            }
            while (dt < 0)
            {
                dt++;
                SpinDown();
            }
        }
        protected Vector2 DownPoint => downPoint;
        protected virtual void OnSpinUp()
        {
        }
        protected virtual void OnSpinDown()
        {
        }
        protected virtual void OnBeginDrag(PointerEventData eventData)
        {
        }
        protected virtual void OnDrag(PointerEventData eventData)
        {
        }
        protected virtual void OnEndDrag(PointerEventData eventData)
        {
        }
        protected virtual void OnCoroutine()
        {
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (buttonDown >= 0)
            {
                return;
            }
            buttonDown = (int)eventData.button;
            //if (eventData.button == PointerEventData.InputButton.Left)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out downPoint);
                clickCount = 0;
                clickDelta = CalcClickDelta(downPoint);
                if (quickButton)
                {
                    Click(clickDelta);
                }
                repeat = (repeatDelay > 0) ? repeatDelay : repeatInterval;
                repeat += Time.unscaledDeltaTime;//cancel this frame
            }
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (buttonDown != (int)eventData.button)
            {
                return;
            }
            if (!quickButton && (clickCount == 0) && !dragging)
            {
                Click(clickDelta);
            }
            {
                buttonDown = -1;
            }
        }
        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            if (buttonDown < 0)
            {
                return;
            }
            //Click((ClickPosition)(-(int)clickPosition));
            dragging = true;
            if (targetSlider != null)
            {
                sliderValue = targetSlider.Value;
            }
            if (targetSelector != null)
            {
                scrollPos = targetSelector.ScrollPos;
                targetSelector.Open();
            }
            if ((targetSlider == null) && (targetSelector == null))
            {
                dragDelta = 0;
            }
            OnBeginDrag(eventData);
        }
        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            if (!dragging)
            {
                return;
            }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 pos);

            var delta = pos - downPoint;
            if (targetSlider != null)
            {
                var reverse = Reverse != targetSlider.Reverse;
                var size = rectTransform.rect.size[Axis];
                var dt = (delta[Axis] / size) * targetSlider.StepValue;
                var val = sliderValue + (reverse ? -dt : +dt);
                targetSlider.Value = val;
            }
            if ((targetSelector != null) && (targetSelector.Count > 1))
            {
                var reverse = Reverse != targetSelector.Reverse;
                var size = targetSelector.GetItemSize()[Axis];
                var dt = delta[Axis] / size;
                var scr = Mathf.Clamp(scrollPos + (reverse ? -dt : +dt), 0, targetSelector.Count - 1);
                targetSelector.ScrollPos = scr;
            }
            if ((targetSlider == null) && (targetSelector == null))
            {
                var rect = rectTransform.rect;
                var size = rectTransform.rect.size[Axis];
                var dt = (int)(delta[Axis] / size * dragSensitivity);
                Click(dt - dragDelta);
                dragDelta = dt;
            }
            OnDrag(eventData);
        }
        void IEndDragHandler.OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
        {
            if (!dragging)
            {
                return;
            }
            dragging = false;
            if (targetSelector != null)
            {
                targetSelector.Close();
            }
            OnEndDrag(eventData);
        }
        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
#if false
            SetClickPosition(eventData);
            Click();
#endif
        }
        public virtual void OnScroll(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
            {
                return;
            }
            var dt = Vector2.Dot(eventData.scrollDelta, scrollDir[(int)direction]);
            if (Mathf.Abs(dt) >= 120)
            {
                dt /= 120.0f;
            }
            if (reverseScroll)
            {
                dt = -dt;
            }
            scrollDelta -= dt / 3;
            var di = (int)scrollDelta;
            if (di != 0)
            {
                scrollDelta -= di;
                Click(di);
            }
        }
        private int CalcClickDelta(Vector2 point)
        {
            var rect = rectTransform.rect;
            var space = Mathf.Clamp01(gap) * 0.5f;//上下の隙間をrectの割合で指定
            var rw = rect.width * space;
            var rh = rect.height * space;
            var pos = point - rect.center;
            pos *= new Vector2(1.0f / rw, 1.0f / rh);
            //LazyDebug.Log($"{pos} {rw},{rh}  {rect}");
            var dt = Vector2.Dot(pos, scrollDir[(int)direction]);
            return Mathf.Clamp((int)dt, -1, +1);
        }
        private void UpdateInput()
        {
            if (!IsActive() || !IsInteractable())
            {
                return;
            }
            if (!dragging && (buttonDown >= 0) && (repeatInterval > 0))
            {
                repeat -= Time.unscaledDeltaTime;
                if (repeat <= 0)
                {
                    Click(clickDelta);
                    repeat = repeatInterval + repeat % repeatInterval;
                }
            }
            if (currentSelectionState == SelectionState.Selected)
            {
                if (InputActions.Up.WasPressedThisFrame())
                {
                    SpinUp();
                }
                if (InputActions.Down.WasPressedThisFrame())
                {
                    SpinDown();
                }
            }
        }
        private void UpdateDirection()
        {
            if (currentDirection != direction)
            {
                var oldAxis = Axis;
                var oldReverse = Reverse;
                currentDirection = direction;
                var newAxis = Axis;
                var newReverse = Reverse;
                if (oldAxis != newAxis)
                {
                    RectTransformUtility.FlipLayoutAxes(transform as RectTransform, true, true);
                }
                if (oldReverse != newReverse)
                {
                    RectTransformUtility.FlipLayoutOnAxis(transform as RectTransform, newAxis, true, true);
                }
            }
        }
        private void UpdateState()
        {
            UpdateInput();
            OnCoroutine();
        }
        protected override void Awake()
        {
            base.Awake();
            currentDirection = direction;
            if (Application.isPlaying)
            {
                if (repeatDelay < 0)
                {
                    repeatDelay = LazyPlayerPrefs.GetValue("RepeatDelay", 0.5f);
                }
                if (repeatInterval < 0)
                {
                    repeatInterval = LazyPlayerPrefs.GetValue("RepeatInterval", 0.1f);
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InputActions.Activate();
            rectTransform = GetComponent<RectTransform>();
            LazyCallbacker.RegisterCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateState);
            if (started)
            {
                UpdateState();
            }
        }
        protected override void Start()
        {
            base.Start();
            started = true;
            {
                UpdateState();
            }
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            rectTransform = null;
            buttonDown = -1;
            dragging = false;
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
            UpdateDirection();
        }
        protected override void OnValidate()
        {
            base.OnValidate();
            UnityEditor.EditorApplication.delayCall += () => DelayedUpdate();
        }
#endif
    }
}

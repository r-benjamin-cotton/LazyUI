using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LazyUI
{
    /// <summary>
    /// button押下で反応するボタン、リピートあり
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class QuickButton : Selectable, ISubmitHandler
    {
        [Serializable]
        public class QuickButtonEvent : UnityEvent { }

        [SerializeField]
        private float repeatDelay = 0.0f;

        [SerializeField]
        private float repeatInterval = 0.0f;

        //[SerializeField]
        public QuickButtonEvent OnClick = new();


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

        private int buttonDown = -1;
        private float repeat = 0;

        public void Submit()
        {
            if (!IsActive() || !IsInteractable())
            {
                return;
            }
            OnClick?.Invoke();
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
                Submit();
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
            {
                buttonDown = -1;
            }
        }
        void ISubmitHandler.OnSubmit(BaseEventData eventData)
        {
            Submit();
        }
        private void UpdateState()
        {
            if ((buttonDown >= 0) && (repeatInterval > 0))
            {
                repeat -= Time.unscaledDeltaTime;
                if (repeat <= 0)
                {
                    Submit();
                    repeat = repeatInterval + repeat % repeatInterval;
                }
            }
        }
        protected override void Awake()
        {
            base.Awake();
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
            LazyCallbacker.RegisterCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateState);
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            buttonDown = -1;
            LazyCallbacker.RemoveCallback(LazyCallbacker.CallbackType.YieldNull, 0, UpdateState);
        }
    }
}

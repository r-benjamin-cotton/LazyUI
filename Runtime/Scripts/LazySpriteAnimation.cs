using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LazyUI
{
    [ExecuteAlways]
    public class LazySpriteAnimation : MonoBehaviour
    {
        public enum WrapMode
        {
            Once,
            Loop,
            PingPong,
            ClampForever,
        }
        public enum StateType
        {
            Idling,
            Running,
            Finished,
        }

        [Serializable]
        public struct Cel
        {
            public Sprite sprite;
            public float duration;
        }
        [Serializable]
        public class Sheet
        {
            public WrapMode wrapMode;
            public Cel[] action;
        }

        [SerializeField]
        private Image targetImage = null;

        [SerializeField]
        private bool paused = false;

        [SerializeField]
        private float speed = 1.0f;

        [SerializeField]
        private bool resetOnActive = true;


        [SerializeField]
        private int initialSheet = 0;

        [SerializeField]
        private List<Sheet> sheets = new();

        private StateType state = StateType.Idling;
        private int index = -1;
        private int next = -1;
        private int dir = +1;
        private int pos = -1;
        private float time = 0;

        public int SheetIndex => index;

        public StateType State => state;

        public float Speed
        {
            get { return speed; }
            set { speed = value; }
        }
        public bool Paused
        {
            get
            {
                return paused;
            }
            set
            {
                paused = value;
            }
        }
        public void Play(int sheetIndex)
        {
            index = -1;
            next = sheetIndex;
            paused = false;
        }
        public void PlayNext(int sheetIndex)
        {
            next = sheetIndex;
            paused = false;
        }
        public void Stop()
        {
            index = -1;
            next = -1;
        }
        public void Pause(bool pause)
        {
            paused = pause;
        }
        private void SetState(StateType state)
        {
            if (this.state == state)
            {
                return;
            }
            this.state = state;
            if (state == StateType.Idling)
            {
                dir = +1;
                pos = -1;
                time = 0;
                if (targetImage != null)
                {
                    targetImage.overrideSprite = null;
                }
            }
        }
        private int frameCount = -1;
        private void Repaint()
        {
            if (!isActiveAndEnabled || (sheets == null))
            {
                index = -1;
                SetState(StateType.Idling);
                return;
            }
            var first = pos < 0;
            if (index < 0)
            {
                index = next;
                if (index < 0)
                {
                    return;
                }
                first = true;
            }
            if (index >= sheets.Count)
            {
                index = -1;
                SetState(StateType.Idling);
                return;
            }
            var sheet = sheets[index];
            if ((sheet == null) || (sheet.action == null) || (sheet.action.Length == 0))
            {
                index = -1;
                SetState(StateType.Idling);
                return;
            }
            var fc = Time.frameCount;
            if (!paused && (frameCount != fc))
            {
                time -= Time.deltaTime * speed;
            }
            frameCount = fc;
            pos = Mathf.Clamp(pos, 0, sheet.action.Length);
            if (first)
            {
                time = sheet.action[pos].duration;
                SetState(StateType.Running);
            }
            var p0 = pos;
            while (time <= 0)
            {
                pos += dir;
                if (pos >= sheet.action.Length)
                {
                    switch (sheet.wrapMode)
                    {
                        case WrapMode.Once:
                            index = -1;
                            SetState(StateType.Idling);
                            return;
                        case WrapMode.Loop:
                            pos = 0;
                            break;
                        case WrapMode.PingPong:
                            pos = (sheet.action.Length < 2) ? 0 : (sheet.action.Length - 2);
                            dir = -dir;
                            break;
                        default:
                        case WrapMode.ClampForever:
                            index = -1;
                            SetState(StateType.Finished);
                            return;
                    }
                }
                else if (pos < 0)
                {
                    dir = -dir;
                    pos = (sheet.action.Length < 2) ? 0 : 1;
                }
                time += sheet.action[pos].duration;
                if (p0 == pos)
                {
                    break;
                }
            }
            if (targetImage != null)
            {
                targetImage.overrideSprite = sheet.action[pos].sprite;
            }
        }
        private void LateUpdate()
        {
            Repaint();
        }
        private void ResetState()
        {
            state = StateType.Idling;
            index = initialSheet;
            next = -1;
            dir = +1;
            pos = -1;
            time = 0;
        }
        private void OnEnable()
        {
            if (resetOnActive)
            {
                ResetState();
            }
            Repaint();
        }
        private void OnDisable()
        {
            if (targetImage != null)
            {
                targetImage.overrideSprite = null;
            }
        }
#if UNITY_EDITOR
        private void Reset()
        {
            targetImage = GetComponent<Image>();
        }
        private void OnValidate()
        {
            index = initialSheet;
            SetState(StateType.Idling);
        }
#endif
    }
}

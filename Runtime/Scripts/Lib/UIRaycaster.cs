using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LazyUI
{
    /// <summary>
    /// UI�Ƃ̏Փ˔���
    /// </summary>
    public static class UIRaycaster
    {
        private static readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
        private static EventSystem eventSystemCurrent = null;
        private static PointerEventData eventDataCurrent = null;
        private static int previousFrameCount = 0;

        /// <summary>
        /// �X�N���[���ʒu��UI�I�u�W�F�N�g��Ԃ�
        /// �����t���[���œ���screenPosition�̏ꍇ�L���b�V���������e��ς����B
        /// ���X�g�͎g���܂킵�Ȃ̂Œ���
        /// </summary>
        /// <param name="screenPosition"></param>
        /// <returns></returns>
        public static ReadOnlyList<RaycastResult> RaycastAll(Vector2 screenPosition)
        {
            var eventSystem = EventSystem.current;
            if (!ReferenceEquals(eventSystemCurrent, eventSystem))
            {
                eventSystemCurrent = eventSystem;
                eventDataCurrent = null;
                raycastResults.Clear();
            }
            if (eventSystem == null)
            {
                return null;
            }
            if (eventDataCurrent == null)
            {
                eventDataCurrent = new PointerEventData(eventSystem);
            }
            var frameCount = Time.frameCount;
            if ((previousFrameCount != frameCount) || (eventDataCurrent.position != screenPosition))
            {
                previousFrameCount = frameCount;
                eventDataCurrent.position = screenPosition;
                if ((screenPosition.x < 0) || (screenPosition.x >= Screen.width) || (screenPosition.y < 0) || (screenPosition.y >= Screen.height))
                {
                    raycastResults.Clear();
                }
                else
                {
                    eventSystem.RaycastAll(eventDataCurrent, raycastResults);
                }
            }
            return raycastResults;
        }
    }
}

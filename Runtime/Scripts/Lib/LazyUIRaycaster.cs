using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LazyUI
{
    /// <summary>
    /// UIとの衝突判定
    /// </summary>
    public static class LazyUIRaycaster
    {
        private static readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
        private static EventSystem eventSystemCurrent = null;
        private static PointerEventData eventDataCurrent = null;
        private static int previousFrameCount = 0;

        /// <summary>
        /// スクリーン位置のUIオブジェクトを返す
        /// 同じフレームで同じscreenPositionの場合キャッシュした内容を変えす。
        /// リストは使いまわしなので注意
        /// </summary>
        /// <param name="screenPosition"></param>
        /// <returns></returns>
        public static LazyReadOnlyList<RaycastResult> RaycastAll(Vector2 screenPosition)
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

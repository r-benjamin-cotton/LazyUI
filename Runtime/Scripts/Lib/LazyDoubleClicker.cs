using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LazyUI
{
    /// <summary>
    /// ダブルクリックを集中管理
    /// DoubleClickDelayで前のクリックからの許容時間
    /// DoubleClickDistanceで前のクリックからの許容距離
    /// </summary>
    public static class LazyDoubleClicker
    {
        public static float DoubleClickDelay = 0.5f;
        public static float DoubleClickDistance = 4.0f;

        private static double time = 0;
        private static PointerEventData.InputButton button = 0;
        private static Vector2 position = Vector2.zero;
        public static bool OnPointerDown(PointerEventData eventData)
        {
            var doubleClick = false;
            var timeStamp = Time.unscaledTimeAsDouble;
            var dist = (position - eventData.position).magnitude;
            var delay = timeStamp - time;
            if ((button == eventData.button) && (dist < DoubleClickDistance) && (delay < DoubleClickDelay))
            {
                doubleClick = true;
            }
            position = eventData.position;
            button = eventData.button;
            time = timeStamp;
            return doubleClick;
        }
    }
}

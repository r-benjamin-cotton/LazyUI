using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LazyUI
{
    using static VectorUtil;

    /// <summary>
    /// ・anchorZero:AnchoredPositionが０になるようAnchorMinMaxを移動
    /// ・autoPivotX,Y:ピボット点をアンカー位置の比率に合わせる
    /// ・keepAspect:親のRectTransformからはみ出さないよう比率を保ってスケーリングする
    /// ・adjustPixel:左下の点をスクリーン座標系でのピクセル境界に近づける※canvasのpixelPerfectとは相性悪いっぽい
    /// pixelPerfectでうまくいかなかったのでanchorで調整してみた（謎。。
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class AnchorCoordinator : UIBehaviour
    {
        [SerializeField]
        private bool anchorZero = false;

        [SerializeField]
        private bool autoPivotX = false;

        [SerializeField]
        private bool autoPivotY = false;

        [SerializeField]
        private bool keepAspect = false;

        [SerializeField]
        private bool adjustPixel = false;

        private const LazyCallbacker.CallbackType callbackType = LazyCallbacker.CallbackType.LateUpdate;
        //private const LazyCallbacker.CallbackType callbackType = LazyCallbacker.CallbackType.WaitForEndOfFrame;

        private bool started = false;
        private float aspect = -1.0f;
        private Vector2 sizeDeltaOrg = Vector2.zero;
        private Vector2 sizeDeltaDelta = Vector2.zero;
        private Vector2 sizeDeltaPrev = Vector2.zero;
        private Vector2 anchorMinPrev = Vector2.zero;
        private Vector2 anchorMaxPrev = Vector2.zero;

        protected RectTransform rectTransform = null;
        protected RectTransform parentRectTransform = null;

        public bool AnchorZero => anchorZero;
        public bool AutoPivotX => autoPivotX;
        public bool AutoPivotY => autoPivotY;
        public bool AutoScaler => keepAspect;
        public bool AdjustPixel => adjustPixel;


        private bool CoordinateAnchorZero()
        {
            if (rectTransform.anchoredPosition == Vector2.zero)
            {
                return false;
            }
            var pr = parentRectTransform.rect;
            var dt = rectTransform.anchoredPosition * Rcp(pr.size);
            rectTransform.anchorMin += dt;
            rectTransform.anchorMax += dt;
            rectTransform.anchoredPosition = Vector2.zero;
            return true;
        }
        private bool CoordinatePivot()
        {
            var pr = parentRectTransform.rect;
            var cr = rectTransform.GetRectInParent();
            var lx = (pr.xMax - pr.xMin) - (cr.xMax - cr.xMin);
            var ly = (pr.yMax - pr.yMin) - (cr.yMax - cr.yMin);
            var tx = (lx == 0) ? 0.5f : ((cr.xMin - pr.xMin) / lx);
            var ty = (ly == 0) ? 0.5f : ((cr.yMin - pr.yMin) / ly);
            var pv = rectTransform.pivot;
            var pp = pv;
            if (autoPivotX)
            {
                pv.x = tx;
            }
            if (autoPivotY)
            {
                pv.y = ty;
            }
            if (pv == pp)
            {
                return false;
            }
            rectTransform.anchoredPosition += rectTransform.sizeDelta * (pv - pp);
            rectTransform.pivot = pv;
            return true;
        }
        private bool CoordinateAspect()
        {
            var cs = rectTransform.rect.size;
            var sd = rectTransform.sizeDelta;
            var mn = rectTransform.anchorMin;
            var mx = rectTransform.anchorMax;
            var iv = (aspect < 0) || (sd != sizeDeltaPrev) || (mn != anchorMinPrev) || (mx != anchorMaxPrev);
            var a0 = (cs.x == 0) ? 0 : (cs.y / cs.x);
            if (iv)
            {
                aspect = a0;
                sizeDeltaOrg = sd;
                sizeDeltaDelta = Vector2.zero;
                sizeDeltaPrev = sd;
                anchorMinPrev = mn;
                anchorMaxPrev = mx;
                return false;
            }
            if ((aspect == 0) || (aspect == a0))
            {
                return false;
            }
            cs -= sizeDeltaDelta;
            var ps = parentRectTransform.rect.size + sizeDeltaOrg;
            var xx = Mathf.Max(cs.x, cs.y / aspect);
            var yy = Mathf.Max(cs.y, cs.x * aspect);
            if (xx > ps.x)
            {
                var r = ps.x / xx;
                xx = xx * r;
                yy = yy * r;
            }
            if (yy > ps.y)
            {
                var r = ps.y / yy;
                xx = xx * r;
                yy = yy * r;
            }
            sizeDeltaDelta.x = xx - cs.x;
            sizeDeltaDelta.y = yy - cs.y;
            sizeDeltaPrev = sizeDeltaOrg + sizeDeltaDelta;
            rectTransform.sizeDelta = sizeDeltaPrev;
            return true;
        }
        private bool CoordinateAdjuster()
        {
            var canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return false;
            }
            var camera = canvas.worldCamera;
            rectTransform.ForceUpdateRectTransforms();
            var mat = rectTransform.localToWorldMatrix;
            var rect = rectTransform.rect;
            var op = rect.position;
            var rp = mat.MultiplyPoint3x4(op);
            var sp = RectTransformUtility.WorldToScreenPoint(camera, rp);
            var fp = Round(sp);
            var dt = Abs(fp - sp);
            var ap = sp;
            var threshold = 1.0f / 256.0f;// 誤差が残るので閾値設定
            var d = false;
            if (dt.x >= threshold)
            {
                ap.x = fp.x;
                d = true;
            }
            if (dt.y >= threshold)
            {
                ap.y = fp.y;
                d = true;
            }
            if (!d)
            {
                return false;
            }
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, ap, camera, out Vector2 np);
            var dd = np - op;
            rectTransform.anchoredPosition += dd;
            return true;
        }

        protected virtual void OnCoordinateAnchor()
        {
            if (anchorZero)
            {
                CoordinateAnchorZero();
            }
            if (autoPivotX || autoPivotY)
            {
                CoordinatePivot();
            }
            if (keepAspect)
            {
                CoordinateAspect();
            }
            if (adjustPixel)
            {
                CoordinateAdjuster();
            }
        }
        protected void CoordinateAnchor()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }
#if UNITY_EDITOR
            if (editing)
            {
                return;
            }
#endif
            {
                parentRectTransform = rectTransform.parent as RectTransform;
                OnCoordinateAnchor();
                parentRectTransform = null;
            }
        }
        protected virtual void OnSetup()
        {
        }
        private void Setup()
        {
            OnSetup();
            {
                LazyCallbacker.RegisterCallback(callbackType, 0, CoordinateAnchor);
#if UNITY_EDITOR
                onBeforeGui += CoordinateAnchor;
#endif
            }
            {
                CoordinateAnchor();
            }
        }
        protected override void OnEnable()
        {
            //base.OnEnable();
            rectTransform = transform as RectTransform;
            if (started)
            {
                Setup();
            }
        }
        protected override void Start()
        {
            //base.Start();
            {
                started = true;
                Setup();
            }
        }
        protected override void OnDisable()
        {
            //base.OnDisable();
            rectTransform = null;
            parentRectTransform = null;
            if (started)
            {
#if UNITY_EDITOR
                onBeforeGui -= CoordinateAnchor;
#endif
                LazyCallbacker.RemoveCallback(callbackType, 0, CoordinateAnchor);
            }
        }
#if false
        protected override void OnRectTransformDimensionsChange()
        {
            Debug.Log("OnRectTransformDimensionsChange");
        }
        private void OnTransformChildrenChanged()
        {
            Debug.Log("OnTransformChildrenChanged");
        }
        protected override void OnTransformParentChanged()
        {
            Debug.Log("OnTransformParentChanged");
        }
        protected override void OnBeforeTransformParentChanged()
        {
            Debug.Log("OnBeforeTransformParentChanged");
        }
#endif
#if UNITY_EDITOR
        private static bool editing = false;
        private static Action onBeforeGui = null;
        [UnityEditor.InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            UnityEditor.SceneView.beforeSceneGui += SceneView_beforeSceneGui;
        }
        private static void SceneView_beforeSceneGui(UnityEditor.SceneView obj)
        {
            if (Event.current == null)
            {
                return;
            }
            // editorでドラッグ中にアンカーの補正を抑制するための判定。怪しい。
            // もう少し何か手段がないのかな？
            var et = Event.current.type;
            if (et == EventType.MouseDown)
            {
                editing = true;
            }
            if (et == EventType.MouseUp)
            {
                editing = false;
            }
            onBeforeGui?.Invoke();
        }
#endif
    }
}

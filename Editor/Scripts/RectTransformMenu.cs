using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MySpace
{
    public class RectTransformMenu
    {
        private static Vector3 Vector3Mul(Vector3 v0, Vector3 v1)
        {
#if true
            v0.Scale(v1);
            return v0;
#else
            return new Vector3(v0.x * v1.x, v0.y * v1.y, v0.z * v1.z);
#endif
        }
        private static Vector3 Vector3Div(Vector3 v0, Vector3 v1)
        {
            return new Vector3(v0.x / v1.x, v0.y / v1.y, v0.z / v1.z);
        }
        private static Vector3 Vector3Lerp(Vector3 v0, Vector3 v1, Vector3 t)
        {
            return Vector3Mul(v1 - v0, t) + v0;
        }
        /// <summary>
        /// シーンにあるルートのオブジェクトを取得
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        private static GameObject[] GetRootGameObjects(GameObject gameObject)
        {
            return gameObject.scene.GetRootGameObjects();
        }
        /// <summary>
        /// オブジェクトの配列からRectTransformを列挙
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        private static IEnumerable<RectTransform> GetRectTransforms(object[] objects)
        {
            return objects.Select((obj) =>
            {
                var go = obj as GameObject;
                return go != null ? go.GetComponent<RectTransform>() : null;
            }).Where(obj => obj != null);
        }
        /// <summary>
        /// オブジェクトの配列からTransformを列挙
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        private static IEnumerable<Transform> GetTransforms(object[] objects)
        {
            return objects.Select((obj) =>
            {
                var go = obj as GameObject;
                return go != null ? go.GetComponent<Transform>() : null;
            }).Where(obj => obj != null);
        }
        /// <summary>
        /// 選択オブジェクトを最初と最後の要素の位置の範囲に均等に再配置
        /// </summary>
        [MenuItem("RectTransform/Arrange Position", priority = 10)]
        private static void ArrangePosition()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return;
            }
            var transforms = GetTransforms(Selection.objects).ToList();
            var count = transforms.Count;
            if (count < 3)
            {
                return;
            }
            var head = transforms[0];
            var tail = transforms[count - 1];
            var ic = 1.0f / (count - 1);
            for (int i = 1; i < count - 1; i++)
            {
                var rt = transforms[i];
                Undo.RecordObject(rt, "RectTransform/Arrange Position");
                var t = i * ic;
                rt.position = Vector3.Lerp(head.position, tail.position, t);
            }
        }
        [MenuItem("RectTransform/Arrange Position", true)]
        private static bool ValidateArrangePosition()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return false;
            }
            var rectTransforms = GetTransforms(Selection.objects);
            return rectTransforms.Count() >= 3;
        }
        /// <summary>
        /// 選択オブジェクトを最初と最後の要素の回転の範囲に均等に再設定
        /// </summary>
        [MenuItem("RectTransform/Arrange Rotation", priority = 20)]
        private static void ArrangeRotation()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return;
            }
            var transforms = GetTransforms(Selection.objects).ToList();
            var count = transforms.Count;
            if (count < 3)
            {
                return;
            }
            var head = transforms[0];
            var tail = transforms[count - 1];
            var ic = 1.0f / (count - 1);
            for (int i = 1; i < count - 1; i++)
            {
                var rt = transforms[i];
                Undo.RecordObject(rt, "RectTransform/Arrange Rotation");
                var t = i * ic;
                rt.rotation = Quaternion.Lerp(head.rotation, tail.rotation, t);
            }
        }
        [MenuItem("RectTransform/Arrange Rotation", true)]
        private static bool ValidateArrangeRotation()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return false;
            }
            var rectTransforms = GetTransforms(Selection.objects);
            return rectTransforms.Count() >= 3;
        }
        /// <summary>
        /// 選択オブジェクトを最初と最後の要素の拡大率の範囲に均等に再設定
        /// </summary>
        [MenuItem("RectTransform/Arrange Scale", priority = 30)]
        private static void ArrangeScale()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return;
            }
            var transforms = GetTransforms(Selection.objects).ToList();
            var count = transforms.Count;
            if (count < 3)
            {
                return;
            }
            var head = transforms[0];
            var tail = transforms[count - 1];
            var ic = 1.0f / (count - 1);
            for (int i = 1; i < count - 1; i++)
            {
                var rt = transforms[i];
                Undo.RecordObject(rt, "RectTransform/Arrange Scale");
                var t = i * ic;
                rt.localScale = Vector3.Lerp(head.localScale, tail.localScale, t);
            }
        }
        [MenuItem("RectTransform/Arrange Scale", true)]
        private static bool ValidateArrangeScale()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return false;
            }
            var rectTransforms = GetTransforms(Selection.objects);
            return rectTransforms.Count() >= 3;
        }
        /// <summary>
        /// 選択オブジェクトを最後の要素を原点に、最初の要素の位置から時計回りに残りのオブジェクトを再配置。
        /// </summary>
        [MenuItem("RectTransform/Align Circle (CW)", priority = 40)]
        private static void AlignCircleCW()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return;
            }
            var transforms = GetTransforms(Selection.objects).ToList();
            var count = transforms.Count;
            if (count < 3)
            {
                return;
            }
            var centerPos = transforms[count - 1].position;
            var dir = transforms[0].position - centerPos;
            var rot = Quaternion.AngleAxis(360.0f / (count - 1), Vector3.back);
            for (int i = 1; i < count - 1; i++)
            {
                var rt = transforms[i];
                Undo.RecordObject(rt, "RectTransform/Align Circle (CW)");
                dir = rot * dir;
                rt.position = dir + centerPos;
            }
        }
        [MenuItem("RectTransform/Align Circle (CW)", true)]
        private static bool ValidateAlignCircleCW()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return false;
            }
            var rectTransforms = GetTransforms(Selection.objects);
            return rectTransforms.Count() >= 3;
        }
        /// <summary>
        /// 選択オブジェクトのY軸下方向を最後の要素の原点へ向ける。
        /// </summary>
        [MenuItem("RectTransform/LookAt (Down)", priority = 50)]
        private static void LookAtDown()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return;
            }
            var transforms = GetTransforms(Selection.objects).ToList();
            var count = transforms.Count;
            if (count < 2)
            {
                return;
            }
            var targetPosition = transforms[count - 1].position;
            for (int i = 0; i < count - 1; i++)
            {
                var rt = transforms[i];
                Undo.RecordObject(rt, "RectTransform/LookAt (Down)");
                //rt.LookAt(last);
                var dir = rt.position - targetPosition;
                rt.rotation = rt.rotation * Quaternion.AngleAxis(Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg, Vector3.back);

            }
        }
        [MenuItem("RectTransform/LookAt (Down)", true)]
        private static bool ValidateLookAtDown()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return false;
            }
            var rectTransforms = GetTransforms(Selection.objects);
            return rectTransforms.Count() >= 2;
        }
        /// <summary>
        /// 選択オブジェクトを最初の要素の水平中央にそろえる
        /// </summary>
        [MenuItem("RectTransform/Align Horizontally Center", priority = 60)]
        private static void AlignCenterH()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return;
            }
            var rectTransforms = GetRectTransforms(Selection.objects).ToList();
            var count = rectTransforms.Count;
            if (count < 2)
            {
                return;
            }
            var head = rectTransforms[0];
            var org = head.position.y;
            for (int i = 1; i < count; i++)
            {
                var rt = rectTransforms[i];
                Undo.RecordObject(rt, "RectTransform/Align Horizontally Center");
                var pos = rt.position;
                pos.y = org;
                rt.position = pos;
            }
        }
        [MenuItem("RectTransform/Align Horizontally Center", true)]
        private static bool ValidateAlignCenterH()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return false;
            }
            var rectTransforms = GetRectTransforms(Selection.objects);
            return rectTransforms.Count() >= 2;
        }
        /// <summary>
        /// 選択オブジェクトを最初の要素の垂直中央にそろえる
        /// </summary>
        [MenuItem("RectTransform/Align Vertically Center", priority = 70)]
        private static void AlignCenterV()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return;
            }
            var rectTransforms = GetRectTransforms(Selection.objects).ToList();
            var count = rectTransforms.Count;
            if (count < 2)
            {
                return;
            }
            var head = rectTransforms[0];
            var org = head.position.x;
            for (int i = 1; i < count; i++)
            {
                var rt = rectTransforms[i];
                Undo.RecordObject(rt, "RectTransform/Align Vertically Center");
                var pos = rt.position;
                pos.x = org;
                rt.position = pos;
            }
        }
        [MenuItem("RectTransform/Align Vertically Center", true)]
        private static bool ValidateAlignCenterV()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return false;
            }
            var rectTransforms = GetRectTransforms(Selection.objects);
            return rectTransforms.Count() >= 2;
        }
        /// <summary>
        /// 選択オブジェクトを最初の要素の左端にそろえる
        /// </summary>
        [MenuItem("RectTransform/Align Left", priority = 80)]
        private static void AlignLeft()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return;
            }
            var rectTransforms = GetRectTransforms(Selection.objects).ToList();
            var count = rectTransforms.Count;
            if (count < 2)
            {
                return;
            }
            var head = rectTransforms[0];
            var org = head.TransformPoint(head.rect.xMin, 0.0f, 0.0f).x;
            for (int i = 1; i < count; i++)
            {
                var rt = rectTransforms[i];
                Undo.RecordObject(rt, "RectTransform/Align Left");
                var pos = rt.position;
                pos.x -= rt.TransformPoint(rt.rect.xMin, 0.0f, 0.0f).x - org;
                rt.position = pos;
            }
        }
        [MenuItem("RectTransform/Align Left", true)]
        private static bool ValidateAlignLeft()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return false;
            }
            var rectTransforms = GetRectTransforms(Selection.objects);
            return rectTransforms.Count() >= 2;
        }
        /// <summary>
        /// 選択オブジェクトを最初の要素の右端にそろえる
        /// </summary>
        [MenuItem("RectTransform/Align Right", priority = 90)]
        private static void AlignRight()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return;
            }
            var rectTransforms = GetRectTransforms(Selection.objects).ToList();
            var count = rectTransforms.Count;
            if (count < 2)
            {
                return;
            }
            var head = rectTransforms[0];
            var org = head.TransformPoint(head.rect.xMax, 0.0f, 0.0f).x;
            for (int i = 1; i < count; i++)
            {
                var rt = rectTransforms[i];
                Undo.RecordObject(rt, "RectTransform/Align Right");
                var pos = rt.position;
                pos.x -= rt.TransformPoint(rt.rect.xMax, 0.0f, 0.0f).x - org;
                rt.position = pos;
            }
        }
        [MenuItem("RectTransform/Align Right", true)]
        private static bool ValidateAlignRight()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return false;
            }
            var rectTransforms = GetRectTransforms(Selection.objects);
            return rectTransforms.Count() >= 2;
        }
        /// <summary>
        /// 選択オブジェクトを最初の要素の上端にそろえる
        /// </summary>
        [MenuItem("RectTransform/Align Top", priority = 100)]
        private static void AlignTop()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return;
            }
            var rectTransforms = GetRectTransforms(Selection.objects).ToList();
            var count = rectTransforms.Count;
            if (count < 2)
            {
                return;
            }
            var head = rectTransforms[0];
            var org = head.TransformPoint(0.0f, head.rect.yMax, 0.0f).y;
            for (int i = 1; i < count; i++)
            {
                var rt = rectTransforms[i];
                Undo.RecordObject(rt, "RectTransform/Align Top");
                var pos = rt.position;
                pos.y -= rt.TransformPoint(0.0f, rt.rect.yMax, 0.0f).y - org;
                rt.position = pos;
            }
        }
        [MenuItem("RectTransform/Align Top", true)]
        private static bool ValidateAlignTop()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return false;
            }
            var rectTransforms = GetRectTransforms(Selection.objects);
            return rectTransforms.Count() >= 2;
        }
        /// <summary>
        /// 選択オブジェクトを最初の要素の下端にそろえる
        /// </summary>
        [MenuItem("RectTransform/Align Bottom", priority = 110)]
        private static void AlignBottom()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return;
            }
            var rectTransforms = GetRectTransforms(Selection.objects).ToList();
            var count = rectTransforms.Count;
            if (count < 2)
            {
                return;
            }
            var head = rectTransforms[0];
            var org = head.TransformPoint(0.0f, head.rect.yMin, 0.0f).y;
            for (int i = 1; i < count; i++)
            {
                var rt = rectTransforms[i];
                Undo.RecordObject(rt, "RectTransform/Align Bottom");
                var pos = rt.position;
                pos.y -= rt.TransformPoint(0.0f, rt.rect.yMin, 0.0f).y - org;
                rt.position = pos;
            }
        }
        [MenuItem("RectTransform/Align Bottom", true)]
        private static bool ValidateAlignBottom()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return false;
            }
            var rectTransforms = GetRectTransforms(Selection.objects);
            return rectTransforms.Count() >= 2;
        }
        /// <summary>
        /// 選択オブジェクトのアンカーをオブジェクトの原点に集める
        /// </summary>
        [MenuItem("CONTEXT/RectTransform/BakeAnchor Gather")]
        [MenuItem("RectTransform/BakeAnchor Gather", priority = 120)]
        private static void BakeAnchorGather()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return;
            }
            var rectTransforms = GetRectTransforms(Selection.objects);
            foreach (var rectTransform in rectTransforms)
            {
                BakeAnchorGather(rectTransform);
            }
        }
        [MenuItem("RectTransform/BakeAnchor Gather", true)]
        private static bool ValidateBakeAnchorGather()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return false;
            }
            var rectTransforms = GetRectTransforms(Selection.objects);
            return rectTransforms.Count() > 0;
        }
        /// <summary>
        /// 選択オブジェクトのアンカーをオブジェクトの四隅に広げる
        /// </summary>
        [MenuItem("CONTEXT/RectTransform/BakeAnchor Spread")]
        [MenuItem("RectTransform/BakeAnchor Spread", priority = 130)]
        private static void BakeAnchorSpread()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return;
            }
            var rectTransforms = GetRectTransforms(Selection.objects);
            foreach (var rectTransform in rectTransforms)
            {
                BakeAnchorSpread(rectTransform);
            }
        }
        [MenuItem("RectTransform/BakeAnchor Spread", true)]
        private static bool ValidateBakeAnchorSpread()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return false;
            }
            var rectTransforms = GetRectTransforms(Selection.objects);
            return rectTransforms.Count() > 0;
        }

        private static RectTransform GetParentRectTransform(RectTransform rt)
        {
            if (rt.parent == null)
            {
                return null;
            }
            return rt.parent.GetComponent<RectTransform>();
        }
        /// <summary>
        /// RectTransformのアンカーをpivot点に集める。
        /// </summary>
        /// <param name="rectTransform"></param>
        private static void BakeAnchorGather(RectTransform rectTransform)
        {
            var p = GetParentRectTransform(rectTransform);
            if (p == null)
            {
                return;
            }
            var nd = rectTransform.sizeDelta / p.rect.size;
            var np = rectTransform.anchoredPosition / p.rect.size;
            var ns = rectTransform.anchorMax - rectTransform.anchorMin + nd;
            var ap = np + ((rectTransform.anchorMax - rectTransform.anchorMin) * rectTransform.pivot) + rectTransform.anchorMin;
            Undo.RecordObject(rectTransform, "RectTransform/BakeAnchorGather");
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.anchorMin = ap;
            rectTransform.anchorMax = ap;
            rectTransform.sizeDelta = ns * p.rect.size;
        }
        /// <summary>
        /// RectTransformのアンカーをUI要素の四隅に分散。
        /// </summary>
        /// <param name="rectTransform"></param>
        private static void BakeAnchorSpread(RectTransform rectTransform)
        {
            var p = GetParentRectTransform(rectTransform);
            if (p == null)
            {
                return;
            }
            var ls = new Vector2(rectTransform.localScale.x, rectTransform.localScale.y);
            var nd = rectTransform.sizeDelta / p.rect.size;
            var np = rectTransform.anchoredPosition / p.rect.size;
            var ns = rectTransform.anchorMax - rectTransform.anchorMin + nd;
            var ss = ns * ls;
            var ap = np + ((rectTransform.anchorMax - rectTransform.anchorMin) * rectTransform.pivot) + rectTransform.anchorMin;
            var tl = ap - (ss * rectTransform.pivot);
            var br = ap + (ss * (Vector2.one - rectTransform.pivot));
            Undo.RecordObject(rectTransform, "RectTransform/BakeAnchorSpread");
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.anchorMin = tl;
            rectTransform.anchorMax = br;
            rectTransform.sizeDelta = ns * (Vector2.one - ls) * p.rect.size;
        }
#if false
        [MenuItem("RectTransform/Normalize Scale", priority = 90)]
        private static void NormalizeScale()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return;
            }
            var rectTransforms = GetRectTransforms(Selection.objects);
            foreach (var rectTransform in rectTransforms)
            {
                NormalizeScale(rectTransform);
            }
        }
        [MenuItem("RectTransform/Normalize Scale", true)]
        private static bool ValidateNormalizeScale()
        {
            if ((Selection.objects == null) || (Selection.objects.Length == 0))
            {
                return false;
            }
            var rectTransforms = GetRectTransforms(Selection.objects);
            return rectTransforms.Count() > 0;
        }

        // uGUIのRectTransformのscaleを強制的に1.0fへ変更。
        // 子要素は大きさや位置が変わらないよう自動調整。
        // Textコンポーネントはフォントが表示領域のサイズに依存しないのでスケールで調整。
        // Imageコンポーネントのタイルパターンは比率が変わるので注意。
        private static void NormalizeScale(RectTransform rt)
        {
            Undo.RecordObject(rt, "NormalizeScale:" + rt);
            // 自身のスケールを正規化
            float sx = rt.localScale.x;
            float sy = rt.localScale.y;
            float sz = rt.localScale.z;
            {
                UnityEngine.UI.Text txt = rt.GetComponent<UnityEngine.UI.Text>();
                if (txt != null)
                {
                    Undo.RecordObject(txt, "NormalizeScale:" + rt);
                    sy = sx;
                    sz = sx;
                    txt.fontSize = (int)(txt.fontSize * sx);
                }
                float px = (rt.anchorMax.x - rt.anchorMin.x) * rt.pivot.x + rt.anchorMin.x;
                float py = (rt.anchorMax.y - rt.anchorMin.y) * rt.pivot.y + rt.anchorMin.y;
                rt.anchorMin = new Vector2((rt.anchorMin.x - px) * sx + px, (rt.anchorMin.y - py) * sy + py);
                rt.anchorMax = new Vector2((rt.anchorMax.x - px) * sx + px, (rt.anchorMax.y - py) * sy + py);
                rt.sizeDelta = new Vector2(rt.sizeDelta.x * sx, rt.sizeDelta.y * sy);
                rt.localScale = new Vector3(1.0f, rt.localScale.y / sy, 1.0f);
            }

            // 子要素の位置とサイズを調整。
            TransformActionRecursively(rt, (ct) =>
            {
                RectTransform crt = ct as RectTransform;
                if (crt == null)
                {
                    return;
                }
                Undo.RecordObject(crt, "NormalizeScale:" + rt);

                UnityEngine.UI.Text txt = ct.GetComponent<UnityEngine.UI.Text>();
                if (txt != null)
                {
#if true
                    // テキスト要素の時は強制的に横を１に正規化、yzは比率を保つようにスケール。
                    Undo.RecordObject(txt, "NormalizeScale:" + rt);
                    float fs = crt.transform.localScale.x * sx;
                    float isx = fs / sx;
                    float isy = fs / sy;
                    float isz = fs / sz;
                    float cpx = (crt.anchorMax.x - crt.anchorMin.x) * crt.pivot.x + crt.anchorMin.x;
                    float cpy = (crt.anchorMax.y - crt.anchorMin.y) * crt.pivot.y + crt.anchorMin.y;
                    crt.anchorMin = new Vector2((crt.anchorMin.x - cpx) * isx + cpx, (crt.anchorMin.y - cpy) * isy + cpy);
                    crt.anchorMax = new Vector2((crt.anchorMax.x - cpx) * isx + cpx, (crt.anchorMax.y - cpy) * isy + cpy);
                    crt.sizeDelta = new Vector2(crt.sizeDelta.x * fs, crt.sizeDelta.y * fs);
                    crt.anchoredPosition3D = new Vector3(crt.anchoredPosition3D.x * sx, crt.anchoredPosition3D.y * sy, crt.anchoredPosition3D.z * sz);
                    crt.transform.localScale = new Vector3(crt.transform.localScale.x / isx, crt.transform.localScale.y / isy, crt.transform.localScale.z / isz);
                    txt.fontSize = (int)(txt.fontSize * fs);
#else
                    // テキスト要素の時はスケールで調整。
                    float isx = 1.0f / sx;
                    float isy = 1.0f / sy;
                    float cpx = (crt.anchorMax.x - crt.anchorMin.x) * crt.pivot.x + crt.anchorMin.x;
                    float cpy = (crt.anchorMax.y - crt.anchorMin.y) * crt.pivot.y + crt.anchorMin.y;
                    crt.anchorMin = new Vector2((crt.anchorMin.x - cpx) * isx + cpx, (crt.anchorMin.y - cpy) * isy + cpy);
                    crt.anchorMax = new Vector2((crt.anchorMax.x - cpx) * isx + cpx, (crt.anchorMax.y - cpy) * isy + cpy);
                    //crt.sizeDelta = new Vector2(crt.sizeDelta.x * sx, crt.sizeDelta.y * sy);
                    crt.anchoredPosition3D = new Vector3(crt.anchoredPosition3D.x * sx, crt.anchoredPosition3D.y * sy, crt.anchoredPosition3D.z * sz);
                    crt.transform.localScale = new Vector3(crt.transform.localScale.x * sx, crt.transform.localScale.y * sy, crt.transform.localScale.z * sz);
#endif
                }
                else
                {
                    // テキスト以外はサイズを調整。
                    crt.sizeDelta = new Vector2(crt.sizeDelta.x * sx, crt.sizeDelta.y * sy);
                    crt.anchoredPosition3D = new Vector3(crt.anchoredPosition3D.x * sx, crt.anchoredPosition3D.y * sy, crt.anchoredPosition3D.z * sz);
                }
            });
        }
        // trasnformの子要素を再帰的に列挙
        private static void TransformActionRecursively(Transform t, System.Action<Transform> action)
        {
            for (int i = 0; i < t.childCount; i++)
            {
                Transform ct = t.GetChild(i);
                action(ct);
                TransformActionRecursively(ct, action);
            }
        }
#endif
    }
}

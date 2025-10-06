using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

namespace LazyUI
{
    /// <summary>
    /// 怠惰なテクスチャフォント
    /// 等間隔グリッド(DWidth,Dheight)でcharactersで指定した順に割り当て,半端は切り捨て
    /// metricsで個別に代替文字、基準位置、幅を上書き指定できる
    /// 
    ///  <------->width
    ///  |   *      ^
    ///  |  ***     |height
    ///  | *   *    |
    /// -+-------   ~
    ///  ^origin(x,y)
    /// 
    /// </summary>
    [CreateAssetMenu(menuName = "LazyUI/TexelFont", fileName = "LazyUITexelFont", order = 91)]
    public class LazyTexelFont : ScriptableObject
    {
        [Serializable]
        public struct Metrics
        {
            public byte tx;
            public byte ty;
            public sbyte originX;
            public sbyte originY;
            public byte width;
            public byte height;
        }

        [SerializeField]
        private Texture2D texture = null;
        [SerializeField]
        private int dwidth = 8;
        [SerializeField]
        private int dheight = 8;

        [SerializeField]
        private int originX = 0;
        [SerializeField]
        private int originY = 0;
        [SerializeField]
        private int width = 8;
        [SerializeField]
        private int height = 8;
        [SerializeField, LazyRawString, TextArea]
        private string characters = "";
        [SerializeField, LazyRawString, TextArea]
        [Tooltip("character,reference character,originx,originy,width,height\\n")]
        private string metrics = "";
        [SerializeField, Tooltip("pixel coord")]
        private Rect backgroundUV = new(0, 0, 0, 0);

        private Metrics tofu = default;

        public Texture2D Texture => texture;
        public int DWidth => dwidth;
        public int DHeight => dheight;
        public int Width => width;
        public int Height => height;
        public string Characters => characters;
        public Rect BackgroundUV => backgroundUV;
        public Metrics Tofu => tofu;

        public bool Valid
        {
            get
            {
                if (texture == null)
                {
                    return false;
                }
                if ((texture.width < dwidth) || (texture.height < dheight))
                {
                    return false;
                }
                if ((dwidth <= 0) || (dheight <= 0) || (width <= 0) || (height <= 0))
                {
                    return false;
                }
                if (string.IsNullOrEmpty(characters))
                {
                    return false;
                }
                return true;
            }
        }

        private Dictionary<char, Metrics> metricsCache = null;
        public LazyReadOnlyDictionary<char, Metrics> GetMetrics()
        {
            if (metricsCache != null)
            {
                return metricsCache;
            }
            metricsCache = new Dictionary<char, Metrics>();
            if (!Valid)
            {
                return metricsCache;
            }
            var tofuch = '\0';
            {
                var ww = Mathf.Min(byte.MaxValue + 1, texture.width / dwidth);
                var hh = Mathf.Min(byte.MaxValue + 1, texture.height / dheight);
                var max = ww * hh;
                var cnt = 0;
                foreach ((int _, char ch, bool escaped) in EnumChar(characters))
                {
                    if (!escaped && char.IsControl(ch))
                    {
                        continue;
                    }
                    if (metricsCache.ContainsKey(ch))
                    {
                        continue;
                    }
                    var m = new Metrics()
                    {
                        tx = (byte)(cnt % ww),
                        ty = (byte)(cnt / ww),
                        originX = (sbyte)originX,
                        originY = (sbyte)originY,
                        width = (byte)width,
                        height = (byte)height,
                    };
                    if ((cnt == 0) || (ch == '\0'))
                    {
                        tofu = m;
                        tofuch = ch;
                    }
                    //LazyDebug.Log($"{cnt:x2}: {(int)ch:x4}[{(char.IsControl(ch) ? ' ' : ch)}] {m.tx * dwidth},{m.ty * dheight}");
                    metricsCache[ch] = m;
                    cnt++;
                    if (cnt >= max)
                    {
                        break;
                    }
                }
            }
            if (!string.IsNullOrEmpty(metrics))
            {
                var ml = metrics.Split('\n');
                var line = 0;
                foreach (var l in ml)
                {
                    line++;
                    int index = 0;
                    char rc = '\0';
                    char ch = '\0';
                    var str = GetCSVField(l, ref index);
                    if (string.IsNullOrEmpty(str))
                    {
                        continue;
                    }
                    {
                        (_, ch, _) = GetChar(str, 0, false);
                    }
                    str = GetCSVField(l, ref index);
                    if (string.IsNullOrEmpty(str))
                    {
                        rc = ch;
                    }
                    else
                    {
                        (_, rc, _) = GetChar(str, 0, false);
                    }
                    if (!metricsCache.TryGetValue(rc, out Metrics m))
                    {
                        //LazyDebug.LogWarning($"{line}: missing character: {l}");
                        continue;
                    }
                    str = GetCSVField(l, ref index);
                    if (sbyte.TryParse(str, out sbyte ox))
                    {
                        m.originX = ox;
                    }
                    str = GetCSVField(l, ref index);
                    if (sbyte.TryParse(str, out sbyte oy))
                    {
                        m.originY = oy;
                    }
                    str = GetCSVField(l, ref index);
                    if (byte.TryParse(str, out byte ww) && (ww != 0))
                    {
                        m.width = ww;
                    }
                    str = GetCSVField(l, ref index);
                    if (byte.TryParse(str, out byte hh) && (hh != 0))
                    {
                        m.height = hh;
                    }
                    metricsCache[ch] = m;
                    if (ch == tofuch)
                    {
                        tofu = m;
                    }
                }
            }
            return metricsCache;
        }
        private void OnValidate()
        {
            dwidth = Mathf.Max(dwidth, 0);
            dheight = Mathf.Max(dheight, 0);
            originX = Mathf.Clamp(originX, sbyte.MinValue, sbyte.MaxValue);
            originY = Mathf.Clamp(originY, sbyte.MinValue, sbyte.MaxValue);
            width = Mathf.Clamp(width, byte.MinValue, byte.MaxValue);
            height = Mathf.Clamp(height, byte.MinValue, byte.MaxValue);
            metricsCache = null;
        }
        private static string GetCSVField(string line, ref int index)
        {
            var str = "";
            var quot = false;
            var end = line.Length;
            if (index >= end)
            {
                return str;
            }
            while (index < end)
            {
                var ch = line[index++];
                if (ch == ',')
                {
                    if (!quot)
                    {
                        break;
                    }
                }
                else if (ch == '\"')
                {
                    if (index >= end)
                    {
                        break;
                    }
                    else
                    {
                        ch = line[index];
                        if (ch != '\"')
                        {
                            quot = !quot;
                            continue;
                        }
                        index++;
                    }
                }
                else if (ch == '\\')
                {
                    if (index < end)
                    {
                        ch = line[index++];
                    }
                }
                str += ch;
            }
#if true
            str = str.Trim();
#endif
            return str;
        }
        public static (int index, char character, bool escaped) GetChar(string text, int index, bool raw)
        {
            var i = index;
            var ch = text[i++];
            if (raw || (ch != '\\'))
            {
                return (i, ch, false);
            }
            else
            {
                var end = text.Length;
                if (i >= end)
                {
                    return (i, '\\', false);
                }
                ch = text[i++];
                switch (ch)
                {
                    case '0':
                        ch = '\0';
                        break;
                    case 'a':
                        ch = '\a';
                        break;
                    case 'b':
                        ch = '\b';
                        break;
                    case 'e':
                        ch = '\x1b';
                        break;
                    case 'f':
                        ch = '\f';
                        break;
                    case 'n':
                        ch = '\n';
                        break;
                    case 'r':
                        ch = '\r';
                        break;
                    case 't':
                        ch = '\t';
                        break;
                    case 'u':
                    case 'x':
                        {
                            var cn = 0;
                            var cnt = 0;
                            while (i < end)
                            {
                                var c = text[i++];
                                var n = "0123456789abcdef".IndexOf(c);
                                if (n < 0)
                                {
                                    n = "ABCDEF".IndexOf(c);
                                    if (n < 0)
                                    {
                                        i--;
                                        break;
                                    }
                                    n += 10;
                                }
                                cn = (cn << 4) | n;
                                if (++cnt == 4)
                                {
                                    break;
                                }
                            }
                            ch = (char)cn;
                        }
                        break;
                    case 'v':
                        ch = '\v';
                        break;
                    case '\\':
                        ch = '\\';
                        break;
                    case '\'':
                        ch = '\'';
                        break;
                    case '\"':
                        ch = '\"';
                        break;
                    default:
                    case 'U': // unsupported
                        return (i - 1, '\\', false);
                }
                return (i, ch, true);
            }
        }
        public static IEnumerable<(int index, char character, bool escaped)> EnumChar(string text, bool raw = false)
        {
            int i = 0;
            int end = text.Length;
            while (i < end)
            {
                (int nxt, char ch, bool esc) = GetChar(text, i, raw);
                yield return (i, ch, esc);
                i = nxt;
            }
        }
    }
}

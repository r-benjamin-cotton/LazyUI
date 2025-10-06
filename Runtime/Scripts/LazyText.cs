using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LazyUI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasRenderer))]
    [ExecuteAlways]
    public class LazyText : MaskableGraphic
    {
        public enum HorizontalAlignmentType
        {
            Left,
            Center,
            Right,
        }
        public enum VerticalAlignmentType
        {
            Top,
            Middle,
            Bottom,
        }
        public enum DirectionType
        {
            LeftToRight,
            RightToLeft,
            TopToBottom,
        }

        [SerializeField]
        private LazyTexelFont texelFont = null;

        [SerializeField]
        private Color fgColor = Color.white;

        [SerializeField]
        private Color bgColor = Color.clear;

        [SerializeField]
        private int fontSize = 8;

        [SerializeField]
        private int lineGap = 3;

        [SerializeField]
        private int spacing = 0;

        [SerializeField]
        private HorizontalAlignmentType horizontalAlignment = HorizontalAlignmentType.Left;

        [SerializeField]
        private VerticalAlignmentType verticalAlignment = VerticalAlignmentType.Top;

        [SerializeField]
        private DirectionType direction = DirectionType.LeftToRight;

        [SerializeField]
        private bool wrapping = false;

        [SerializeField]
        private bool overflow = true;

        [SerializeField, TextArea, LazyRawString]
        private string text = "";

        [SerializeField]
        [Tooltip("fontsize:{12s} fontColor:{3f}")]
        private bool richText = true;

        [SerializeField]
        private bool parseEscapeCharacters = true;

        [SerializeField]
        private LazyMargin margin = new(0, 0, 0, 0);

        private struct Characteristics
        {
            public int index;
            public char character;

            public float scale;
            public Vector2 pos;
            public Vector2 siz;
            public Vector2 spc;

            public Vector2 ofs;
            public Vector2 psz;
            public Vector2 tex;
            public Color fgColor;
            public Color bgColor;
        }

        private bool dirtyText = true;
        private bool cooking = false;
        private Rect rect = default;
        private Rect boundingRect = default;
        private int skipLine = 0;
        private class LineInfo
        {
            public Rect bounds;
            public Vector2 tailPos;
            public int headIdx;
            public int tailIdx;
            public float headScl;
            public float tailScl;
            public float width;
            public float height;
            public readonly List<Characteristics> chlist = new();
            public void Clear()
            {
                bounds = default;
                tailPos = default;
                headIdx = 0;
                tailIdx = 0;
                headScl = 1.0f;
                tailScl = 1.0f;
                width = 0;
                height = 0;
                chlist.Clear();
            }
        }
        private int lineCount = 0;
        private readonly List<LineInfo> lines = new();
        private readonly List<UIVertex> vertices = new();
        private readonly List<int> indices = new();

        private readonly static Color[] colors = BuildColorTable();
        private readonly List<string> parameters = new();

        public struct Context
        {
            public int fontSize;
            //public int fontType;
            public int offsetX;
            public int offsetY;
            public Color fgColor;
            public Color bgColor;
        }
        public delegate void OverrideActionDelegate(string text, int index, char ch, ref Context context);
        public event OverrideActionDelegate OverrideAction;

        public override Texture mainTexture
        {
            get
            {
                return (texelFont == null) ? null : texelFont.Texture;
            }
        }

        public void DirtyText()
        {
            dirtyText = true;
            SetVerticesDirty();
        }

        public LazyTexelFont TexelFont
        {
            get
            {
                return texelFont;
            }
            set
            {
                if (texelFont == value)
                {
                    return;
                }
                texelFont = value;
                DirtyText();
            }
        }
        public Color ForegroundColor
        {
            get
            {
                return fgColor;
            }
            set
            {
                if (fgColor == value)
                {
                    return;
                }
                fgColor = value;
                DirtyText();
            }
        }
        public Color BackgroundColor
        {
            get
            {
                return bgColor;
            }
            set
            {
                if (bgColor == value)
                {
                    return;
                }
                bgColor = value;
                DirtyText();
            }
        }
        public int FontSize
        {
            get
            {
                return fontSize;
            }
            set
            {
                if (fontSize == value)
                {
                    return;
                }
                fontSize = value;
                DirtyText();
            }
        }
        public int LineGap
        {
            get
            {
                return lineGap;
            }
            set
            {
                if (lineGap == value)
                {
                    return;
                }
                lineGap = value;
                DirtyText();
            }
        }
        public int Spacing
        {
            get
            {
                return spacing;
            }
            set
            {
                if (spacing == value)
                {
                    return;
                }
                spacing = value;
                DirtyText();
            }
        }
        public HorizontalAlignmentType HorizontalAlignment
        {
            get
            {
                return horizontalAlignment;
            }
            set
            {
                if (horizontalAlignment == value)
                {
                    return;
                }
                horizontalAlignment = value;
                DirtyText();
            }
        }
        public VerticalAlignmentType VerticalAlignment
        {
            get
            {
                return verticalAlignment;
            }
            set
            {
                if (verticalAlignment == value)
                {
                    return;
                }
                verticalAlignment = value;
                DirtyText();
            }
        }
        public DirectionType Direction
        {
            get
            {
                return direction;
            }
            set
            {
                if (direction == value)
                {
                    return;
                }
                direction = value;
                DirtyText();
            }
        }
        public bool Wrapping
        {
            get
            {
                return wrapping;
            }
            set
            {
                if (wrapping == value)
                {
                    return;
                }
                wrapping = value;
                DirtyText();
            }
        }
        public bool Overflow
        {
            get
            {
                return overflow;
            }
            set
            {
                if (overflow == value)
                {
                    return;
                }
                overflow = value;
                DirtyText();
            }
        }
        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                if (text == value)
                {
                    return;
                }
                text = value;
                DirtyText();
            }
        }
        public bool RichText
        {
            get
            {
                return richText;
            }
            set
            {
                if (richText == value)
                {
                    return;
                }
                richText = value;
                DirtyText();
            }
        }
        public bool ParseEscapeCharacters
        {
            get
            {
                return parseEscapeCharacters;
            }
            set
            {
                if (parseEscapeCharacters == value)
                {
                    return;
                }
                parseEscapeCharacters = value;
                DirtyText();
            }
        }
        public LazyMargin Margin
        {
            get
            {
                return margin;
            }
            set
            {
                if (margin == value)
                {
                    return;
                }
                margin = value;
                DirtyText();
            }
        }

        public int SkipLine
        {
            get
            {
                return skipLine;
            }
            set
            {
                var val = Mathf.Max(value, 0);
                if (skipLine == val)
                {
                    return;
                }
                skipLine = val;
                DirtyText();
            }
        }
        public Rect BoundingRect
        {
            get
            {
                UpdateText();
                return boundingRect;
            }
        }

        private static Color[] BuildColorTable()
        {
            var colors = new Color[256];
            colors[0] = new Color32(1, 1, 1, 255);
            colors[1] = new Color32(222, 56, 43, 255);
            colors[2] = new Color32(57, 181, 74, 255);
            colors[3] = new Color32(255, 199, 6, 255);
            colors[4] = new Color32(0, 111, 184, 255);
            colors[5] = new Color32(118, 38, 113, 255);
            colors[6] = new Color32(44, 181, 233, 255);
            colors[7] = new Color32(204, 204, 204, 255);
            colors[8] = new Color32(128, 128, 128, 255);
            colors[9] = new Color32(255, 0, 0, 255);
            colors[10] = new Color32(0, 255, 0, 255);
            colors[11] = new Color32(255, 255, 0, 255);
            colors[12] = new Color32(0, 0, 255, 255);
            colors[13] = new Color32(255, 0, 255, 255);
            colors[14] = new Color32(0, 255, 255, 255);
            colors[15] = new Color32(255, 255, 255, 255);
            for (int r = 0; r < 6; r++)
            {
                for (int g = 0; g < 6; g++)
                {
                    for (int b = 0; b < 6; b++)
                    {
                        colors[r * 36 + g * 6 + b + 16] = new Color32((byte)(r * 255 / 5), (byte)(g * 255 / 5), (byte)(b * 255 / 5), 255);
                    }
                }
            }
            for (int i = 0; i < 24; i++)
            {
                var v = (byte)(i * 256 / 24);
                colors[i + 232] = new Color32(v, v, v, 255);
            }
            return colors;
        }
        private static Color ParseColor(List<string> parameters, Color color)
        {
            var count = parameters.Count;
            // #00ff00 #00ffffff 7 red 255,255,255 128,128,128,128
            if (count == 4)
            {
                if (int.TryParse(parameters[0], out int r) && (r >= 0) && (r <= 255))
                {
                    if (int.TryParse(parameters[1], out int g) && (g >= 0) && (g <= 255))
                    {
                        if (int.TryParse(parameters[2], out int b) && (b >= 0) && (b <= 255))
                        {
                            if (int.TryParse(parameters[3], out int a) && (a >= 0) && (a <= 255))
                            {
                                color = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
                            }
                        }
                    }
                }
            }
            else if (count == 3)
            {
                if (int.TryParse(parameters[0], out int r) && (r >= 0) && (r <= 255))
                {
                    if (int.TryParse(parameters[1], out int g) && (g >= 0) && (g <= 255))
                    {
                        if (int.TryParse(parameters[2], out int b) && (b >= 0) && (b <= 255))
                        {
                            color = new Color32((byte)r, (byte)g, (byte)b, 255);
                        }
                    }
                }
            }
            else if (count == 1)
            {
                var str = parameters[0];
                if ((str.Length == 7) && (str[0] == '#'))
                {
                    if (uint.TryParse(str[1..], System.Globalization.NumberStyles.HexNumber, null, out uint val))
                    {
                        var b = (val >> 0) & 0xffu;
                        var g = (val >> 8) & 0xffu;
                        var r = (val >> 16) & 0xffu;
                        color = new Color32((byte)r, (byte)g, (byte)b, 255);
                    }
                }
                else if ((str.Length == 9) && (str[0] == '#'))
                {
                    if (uint.TryParse(str[1..], System.Globalization.NumberStyles.HexNumber, null, out uint val))
                    {
                        var a = (val >> 0) & 0xffu;
                        var b = (val >> 8) & 0xffu;
                        var g = (val >> 16) & 0xffu;
                        var r = (val >> 24) & 0xffu;
                        color = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
                    }
                }
                else
                {
                    switch (str.ToLowerInvariant())
                    {
                        case "black":
                            color = Color.black;
                            break;
                        case "red":
                            color = Color.red;
                            break;
                        case "green":
                            color = Color.green;
                            break;
                        case "yellow":
                            color = Color.yellow;
                            break;
                        case "blue":
                            color = Color.blue;
                            break;
                        case "magenta":
                            color = Color.magenta;
                            break;
                        case "cyan":
                            color = Color.cyan;
                            break;
                        case "white":
                            color = Color.white;
                            break;
                        default:
                            if (int.TryParse(str, out int val) && (val >= 0) && (val <= 255))
                            {
                                color = colors[val];
                            }
                            break;
                    }
                }
            }
            return color;
        }
        private static int ParseSize(List<string> parameters, int size)
        {
            if ((parameters.Count == 1) && !string.IsNullOrEmpty(parameters[0]))
            {
                var str = parameters[0];
                if (str[0] == '+')
                {
                    if (int.TryParse(str[1..], out int val))
                    {
                        size = Mathf.Max(1, size + val);
                    }
                }
                else if (str[0] == '-')
                {
                    if (int.TryParse(str[1..], out int val))
                    {
                        size = Mathf.Max(1, size - val);
                    }
                }
                else if (str[^1] == '%')
                {
                    if (int.TryParse(str[..^1], out int val))
                    {
                        size = Mathf.Max(1, (size * val + 50) / 100);
                    }
                }
                else
                {
                    if (int.TryParse(str, out int val))
                    {
                        size = Mathf.Max(1, val);
                    }
                }
            }
            return size;
        }
        private static (int x, int y) ParseInt2(List<string> parameters, (int x, int y) val)
        {
            if (parameters.Count == 2)
            {
                if (int.TryParse(parameters[0], out int x))
                {
                    val.x = x;
                }
                if (int.TryParse(parameters[0], out int y))
                {
                    val.y = y;
                }
            }
            return val;
        }
        private void Process(ref Context context, char cmd, List<string> parameters)
        {
            switch (cmd)
            {
                case 's':
                    context.fontSize = ParseSize(parameters, fontSize);
                    break;
                case 'f':
                    context.fgColor = ParseColor(parameters, fgColor);
                    break;
                case 'o':
                    (context.offsetX, context.offsetY) = ParseInt2(parameters, (0, 0));
                    break;
                case 'b':
                    context.bgColor = ParseColor(parameters, bgColor);
                    break;
                case 'i':
                    (context.fgColor, context.bgColor) = (bgColor, fgColor);
                    break;
#if false
                case 'u':
                    UserAction?.Invoke(text, ref context, parameters);
                    break;
#endif
                default:
                    break;
            }
        }

        private bool Cook(string text, ref Context context, int idx, char ch, bool escaped, int nxt)
        {
            if (!cooking)
            {
                if (!escaped && (ch == '{'))
                {
                    for (int i = nxt, end = text.Length; i < end;)
                    {
                        (int nx, char cc, bool esc) = LazyTexelFont.GetChar(text, i, false);
                        if (!esc)
                        {
                            if (cc == '}')
                            {
                                parameters.Clear();
                                parameters.Add("");
                                cooking = true;
                                return true;
                            }
                            if (cc == '{')
                            {
                                return false;
                            }
                        }
                        i = nx;
                    }
                }
                return false;
            }
            else
            {
                if (!escaped)
                {
                    if ((ch == ';') || (ch == '}'))
                    {
                        if (parameters[^1].Length > 0)
                        {
                            var cmd = parameters[^1][^1];
                            parameters[^1] = parameters[^1][..^1];
                            Process(ref context, cmd, parameters);
                        }
                        parameters.Clear();
                        parameters.Add("");
                        if (ch == '}')
                        {
                            cooking = false;
                        }
                    }
                    else if (ch == ',')
                    {
                        parameters.Add("");
                    }
                    else
                    {
                        parameters[^1] += ch;
                    }
                }
                else
                {
                    parameters[^1] += ch;
                }
                return true;
            }
        }

        private void SetupContext(ref Context context)
        {
            context.fgColor = fgColor;
            context.bgColor = bgColor;
            context.offsetX = 0;
            context.offsetY = 0;
            //context.fontType = 0;
            context.fontSize = fontSize;
        }
        private void ClearState()
        {
            cooking = false;
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i].Clear();
            }
            boundingRect = default;
        }
        private Characteristics GetCharacteristics(ref Context context, int index, char character, LazyTexelFont.Metrics mx, float px, float py, float ww, float hh, float sx, float sy, float scale)
        {
            var ox = context.offsetX - mx.originX * scale;
            var oy = context.offsetY - mx.originY * scale;
            var dw = texelFont.DWidth;
            var dh = texelFont.DHeight;
            var pw = dw * scale;
            var ph = dh * scale;
            var tw = dw / (float)texelFont.Texture.width;
            var th = dh / (float)texelFont.Texture.height;
            var tx = mx.tx * tw;
            var ty = 1.0f - (mx.ty + 1) * th;
            //var ty = mx.ty * dh * ih;
            var m = new Characteristics
            {
                index = index,
                character = character,
                scale = scale,
                pos = new Vector2(px, py),
                siz = new Vector2(ww, hh),
                spc = new Vector2(sx, sy),
                ofs = new Vector2(ox, oy),
                psz = new Vector2(pw, ph),
                tex = new Vector2(tx, ty),
                fgColor = context.fgColor,
                bgColor = context.bgColor,
            };
            return m;
        }
        private Context Override(string text, ref Context context, int index, char ch)
        {
            var ctx = new Context()
            {
                fontSize = context.fontSize,
                offsetX = context.offsetX,
                offsetY = context.offsetY,
                fgColor = context.fgColor,
                bgColor = context.bgColor,
            };
            OverrideAction?.Invoke(text, index, ch, ref ctx);
            return ctx;
        }
        private static Rect NewRectEx(float x, float y, float w, float h, float ew, float eh)
        {
            if (w < 0)
            {
                x += w;
                w = -w;
            }
            if (h < 0)
            {
                y += h;
                h = -h;
            }
            x -= ew * 0.5f;
            y -= eh * 0.5f;
            w += ew;
            h += eh;
            return new Rect(x, y, w, h);
        }
        private void BuildHorizontal()
        {
            var text = this.text;
            var context = new Context();
            SetupContext(ref context);
            ClearState();
            var scale = fontSize / (float)texelFont.Height;
            var min = rect.min + new Vector2(margin.left, margin.bottom);
            var max = rect.max - new Vector2(margin.right, margin.top);
            var len = max - min;
            var line = Mathf.Min(0, -skipLine) - 1;
            var mt = texelFont.GetMetrics();
            var maxHeight = 0.0f;
            var pos = Vector2.zero;
            var dr = (direction == DirectionType.LeftToRight) ? +1 : -1;
            var sp = spacing * dr;
            bool newline(int idx)
            {
                if (line >= 0)
                {
                    var ll = lines[line];
                    if (!overflow && ((pos.y - maxHeight) < -len.y))
                    {
                        ll.Clear();
                        return false;
                    }
                    var dy = maxHeight;
                    for (int i = 0; i < ll.chlist.Count; i++)
                    {
                        var mm = ll.chlist[i];
                        mm.pos.y -= dy;
                        ll.chlist[i] = mm;
                    }
                    var ofs = new Vector2((dr < 0) ? texelFont.Width * scale : 0, maxHeight);
                    var p1s = pos;
                    ll.bounds = NewRectEx(0.0f, pos.y, pos.x, -maxHeight, spacing, lineGap);
                    ll.tailPos = p1s - ofs;
                    ll.tailIdx = idx;
                    ll.tailScl = scale;
                    ll.width = pos.x;
                    ll.height = maxHeight;
                    pos.y -= maxHeight + lineGap;
                }
                maxHeight = texelFont.Height;
                pos.x = 0;
                line++;
                if (line >= lines.Count)
                {
                    lines.Add(new LineInfo());
                }
                lines[line].headIdx = idx + 1;
                lines[line].headScl = scale;
                return true;
            }
            newline(-1);
            for (int idx = 0, end = text.Length, nxt; idx < end; idx = nxt)
            {
                char ch;
                bool escaped;
                (nxt, ch, escaped) = LazyTexelFont.GetChar(text, idx, !parseEscapeCharacters);
                if (richText && Cook(text, ref context, idx, ch, escaped, nxt))
                {
                    continue;
                }
                if (!escaped && char.IsControl(ch))
                {
                    if (ch == '\n')
                    {
                        if (!newline(idx))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    var ctx = Override(text, ref context, idx, ch);
                    scale = ctx.fontSize / (float)texelFont.Height;
                    if (!mt.TryGetValue(ch, out LazyTexelFont.Metrics mx))
                    {
                        mx = texelFont.Tofu;
                    }
                    var ww = mx.width * scale;
                    //var hh = mx.height * scale;
                    var hh = texelFont.Height * scale;
                    var dx = ww * dr;
                    var px0 = pos.x;
                    var px1 = px0 + ((px0 == 0) ? 0 : sp);
                    var px2 = px1 + dx;
                    if ((px0 != 0) && (px2 * dr > len.x))
                    {
                        if (wrapping)
                        {
                            if (!newline(idx))
                            {
                                break;
                            }
                            px1 = 0;
                            px2 = dx;
                        }
                        else if (!overflow)
                        {
                            continue;
                        }
                    }
                    maxHeight = Mathf.Max(maxHeight, hh);
                    pos.x = px2;
                    if (line >= 0)
                    {
                        var px = (dr > 0) ? px1 : px2;
                        var py = pos.y;
                        var m = GetCharacteristics(ref ctx, idx, ch, mx, px, py, ww, hh, spacing, lineGap, scale);
                        lines[line].chlist.Add(m);
                    }
                }
            }
            newline(text.Length);
            lineCount = line;
            if (lineCount <= 0)
            {
                return;
            }
            pos.y += lineGap;
            var dt = Vector2.zero;
            {
                switch (verticalAlignment)
                {
                    default:
                    case VerticalAlignmentType.Top:
                        dt.y = max.y;
                        break;
                    case VerticalAlignmentType.Middle:
                        dt.y = (max.y + min.y - pos.y) * 0.5f;
                        break;
                    case VerticalAlignmentType.Bottom:
                        dt.y = min.y - pos.y;
                        break;
                }
            }
            for (int i = 0; i < lineCount; i++)
            {
                var ll = lines[i];
                switch (horizontalAlignment)
                {
                    default:
                    case HorizontalAlignmentType.Left:
                        dt.x = (dr > 0) ? min.x : (min.x - ll.width);
                        break;
                    case HorizontalAlignmentType.Center:
                        dt.x = (max.x + min.x - ll.width) * 0.5f;
                        break;
                    case HorizontalAlignmentType.Right:
                        dt.x = (dr > 0) ? (max.x - ll.width) : max.x;
                        break;
                }
                for (int j = 0; j < ll.chlist.Count; j++)
                {
                    var mm = ll.chlist[j];
                    mm.pos += dt;
                    var ps = mm.pos - mm.spc * 0.5f;
                    var pz = mm.siz + mm.spc;
                    boundingRect = Enlarge(boundingRect, new Rect(ps, pz));
                    ll.chlist[j] = mm;
                }
                ll.bounds = new Rect(ll.bounds.position + dt, ll.bounds.size);
                ll.tailPos += dt;
            }
        }
        private void BuildVertical()
        {
            var context = new Context();
            SetupContext(ref context);
            ClearState();
            var scale = fontSize / (float)texelFont.Width;
            var min = rect.min + new Vector2(margin.left, margin.bottom);
            var max = rect.max - new Vector2(margin.right, margin.top);
            var len = max - min;
            var line = Mathf.Min(0, -skipLine) - 1;
            var mt = texelFont.GetMetrics();
            var maxWidth = 0.0f;
            var pos = Vector2.zero;
            var dr = -1;
            var sp = spacing * dr;
            bool newline(int idx)
            {
                if (line >= 0)
                {
                    var ll = lines[line];
                    if (!overflow && ((pos.x - maxWidth) < -len.x))
                    {
                        ll.Clear();
                        return false;
                    }
                    var dx = maxWidth * 0.5f;
                    for (int i = 0; i < ll.chlist.Count; i++)
                    {
                        var mm = ll.chlist[i];
                        mm.pos.x -= dx;
                        ll.chlist[i] = mm;
                    }
                    var ofs = new Vector2(dx + texelFont.Width * scale * 0.5f, texelFont.Height * scale);
                    var p1s = pos;
                    ll.bounds = NewRectEx(pos.x, 0.0f, -maxWidth, pos.y, lineGap, spacing);
                    ll.tailPos = p1s - ofs;
                    ll.tailIdx = idx;
                    ll.tailScl = scale;
                    ll.width = maxWidth;
                    ll.height = pos.y;
                    pos.x -= maxWidth + lineGap;
                }
                maxWidth = texelFont.Width;
                pos.y = 0;
                line++;
                if (line >= lines.Count)
                {
                    lines.Add(new LineInfo());
                }
                lines[line].headIdx = idx + 1;
                lines[line].headScl = scale;
                return true;
            }
            newline(-1);
            for (int idx = 0, end = text.Length, nxt; idx < end; idx = nxt)
            {
                char ch;
                bool escaped;
                (nxt, ch, escaped) = LazyTexelFont.GetChar(text, idx, !parseEscapeCharacters);
                if (richText && Cook(text, ref context, idx, ch, escaped, nxt))
                {
                    continue;
                }
                if (!escaped && char.IsControl(ch))
                {
                    if (ch == '\n')
                    {
                        if (!newline(idx))
                        {
                            break;
                        }
                    }
                }
                else
                {
                    var ctx = Override(text, ref context, idx, ch);
                    scale = ctx.fontSize / (float)texelFont.Width;
                    if (!mt.TryGetValue(ch, out LazyTexelFont.Metrics mx))
                    {
                        mx = texelFont.Tofu;
                    }
                    //var ww = mx.width * scale;
                    var ww = texelFont.Width * scale;
                    var hh = mx.height * scale;
                    var dy = hh * dr;
                    var py0 = pos.y;
                    var py1 = py0 + ((py0 == 0) ? 0 : sp);
                    var py2 = py1 + dy;
                    if ((py0 != 0) && (py2 * dr > len.y))
                    {
                        if (wrapping)
                        {
                            if (!newline(idx))
                            {
                                break;
                            }
                            py1 = 0;
                            py2 = dy;
                        }
                        else if (!overflow)
                        {
                            continue;
                        }
                    }
                    maxWidth = Mathf.Max(maxWidth, ww);
                    pos.y = py2;
                    if (line >= 0)
                    {
                        var px = pos.x;
                        var py = py2;
                        if (texelFont.Width > mx.width)
                        {
                            px = px - ww * 0.5f + (texelFont.Width - mx.width) * scale;
                        }
                        else
                        {
                            px = px - ww * 0.5f;
                        }
                        var m = GetCharacteristics(ref ctx, idx, ch, mx, px, py, ww, hh, lineGap, spacing, scale);
                        lines[line].chlist.Add(m);
                    }
                }
            }
            newline(text.Length);
            lineCount = line;
            if (lineCount <= 0)
            {
                return;
            }
            pos.x += lineGap;
            var dt = new Vector2();
            {
                switch (horizontalAlignment)
                {
                    default:
                    case HorizontalAlignmentType.Right:
                        dt.x = max.x;
                        break;
                    case HorizontalAlignmentType.Center:
                        dt.x = (max.x + min.x - pos.x) * 0.5f;
                        break;
                    case HorizontalAlignmentType.Left:
                        dt.x = min.x - pos.x;
                        break;
                }
            }
            for (int i = 0; i < lineCount; i++)
            {
                var ll = lines[i];
                switch (verticalAlignment)
                {
                    default:
                    case VerticalAlignmentType.Bottom:
                        dt.y = min.y - ll.height;
                        break;
                    case VerticalAlignmentType.Middle:
                        dt.y = (max.y + min.y - ll.height) * 0.5f;
                        break;
                    case VerticalAlignmentType.Top:
                        dt.y = max.y;
                        break;
                }
                for (int j = 0; j < ll.chlist.Count; j++)
                {
                    var mm = ll.chlist[j];
                    mm.pos += dt;
                    var ps = mm.pos - mm.spc * 0.5f;
                    var pz = mm.siz + mm.spc;
                    boundingRect = Enlarge(boundingRect, new Rect(ps, pz));
                    ll.chlist[j] = mm;
                }
                ll.bounds = new Rect(ll.bounds.position + dt, ll.bounds.size);
                ll.tailPos += dt;
            }
        }
        private Rect Enlarge(Rect r0, Rect r1)
        {
            if (r0 == default)
            {
                return r0;
            }
            var min = r0.min;
            var max = r0.max;
            min = Vector2.Min(min, r1.min);
            max = Vector2.Max(max, r1.max);
            return new Rect(min, max - min);
        }
        public struct HitInfo
        {
            public int index;
            public char character;
            public Rect rect;
        }
        public HitInfo HitTest(Vector2 localPoint)
        {
            UpdateText();
            foreach (var ll in lines)
            {
                foreach (var mm in ll.chlist)
                {
#if true
                    var rect = new Rect(mm.pos - mm.spc * 0.5f, mm.siz + mm.spc);
#else
                    var rect = new Rect(mm.pos, mm.siz);
#endif
                    if (rect.Contains(localPoint))
                    {
                        var r = new HitInfo()
                        {
                            index = mm.index,
                            character = mm.character,
                            rect = rect,
                        };
                        return r;
                    }
                }
            }
            {
                var r = new HitInfo()
                {
                    index = -1
                };
                return r;
            }
        }
        public int GetStopPosition(int index, bool forward)
        {
            if ((texelFont == null) || !texelFont.Valid)
            {
                return index;
            }
            UpdateText();
            if ((index < 0) || (index >= text.Length))
            {
                return index;
            }
            for (int i = 0; i < lineCount; i++)
            {
                var ll = lines[i];
                if (index <= ll.tailIdx)
                {
                    for (int j = 0; j < ll.chlist.Count; j++)
                    {
                        var mm = ll.chlist[j];
                        if (mm.index == index)
                        {
                            return index;
                        }
                        if (mm.index > index)
                        {
                            if (forward)
                            {
                                return mm.index;
                            }
                            else
                            {
                                if (j == 0)
                                {
                                    if (i == 0)
                                    {
                                        return 0;
                                    }
                                    return lines[i - 1].tailIdx;
                                }
                                return ll.chlist[j - 1].index;
                            }
                        }
                    }
                    break;
                }
            }
            return index;
        }
        public int GetCursorPosition(Vector2 localPoint)
        {
            UpdateText();
            var sph = spacing * 0.5f;
            switch (direction)
            {
                case DirectionType.LeftToRight:
                    for (int i = 0, end = lineCount; i < end; i++)
                    {
                        var ll = lines[i];
                        if (localPoint.y <= ll.bounds.yMin)
                        {
                            continue;
                        }
                        if (localPoint.y > ll.bounds.yMax)
                        {
                            break;
                        }
                        if (localPoint.x <= ll.bounds.xMin)
                        {
                            return ll.headIdx;
                        }
                        if (localPoint.x >= ll.bounds.xMax)
                        {
                            return ll.tailIdx;
                        }
                        for (int j = 0, jend = ll.chlist.Count; j < jend; j++)
                        {
                            var mm = ll.chlist[j];
                            if (localPoint.x >= (mm.pos.x + mm.siz.x + sph))
                            {
                                continue;
                            }
                            if (localPoint.x < (mm.pos.x + mm.siz.x * 0.5f))
                            {
                                return mm.index;
                            }
                            else
                            {
                                var jn = j + 1;
                                if (jn >= jend)
                                {
                                    return ll.tailIdx;
                                }
                                return ll.chlist[jn].index;
                            }
                        }
                    }
                    break;
                case DirectionType.RightToLeft:
                    for (int i = 0, end = lineCount; i < end; i++)
                    {
                        var ll = lines[i];
                        if (localPoint.y <= ll.bounds.yMin)
                        {
                            continue;
                        }
                        if (localPoint.y > ll.bounds.yMax)
                        {
                            break;
                        }
                        if (localPoint.x <= ll.bounds.xMin)
                        {
                            return ll.tailIdx;
                        }
                        if (localPoint.x >= ll.bounds.xMax)
                        {
                            return ll.headIdx;
                        }
                        for (int j = 0, jend = ll.chlist.Count; j < jend; j++)
                        {
                            var mm = ll.chlist[j];
                            if (localPoint.x < (mm.pos.x - sph))
                            {
                                continue;
                            }
                            if (localPoint.x >= (mm.pos.x + mm.siz.x * 0.5f))
                            {
                                return mm.index;
                            }
                            else
                            {
                                var jn = j + 1;
                                if (jn >= jend)
                                {
                                    return ll.tailIdx;
                                }
                                return ll.chlist[jn].index;
                            }
                        }
                    }
                    break;
                case DirectionType.TopToBottom:
                    {
                        for (int i = 0, end = lineCount; i < end; i++)
                        {
                            var ll = lines[i];
                            if (localPoint.x > ll.bounds.xMax)
                            {
                                continue;
                            }
                            if (localPoint.x <= ll.bounds.xMin)
                            {
                                continue;
                            }
                            if (localPoint.y >= ll.bounds.yMax)
                            {
                                return ll.headIdx;
                            }
                            if (localPoint.y <= ll.bounds.yMin)
                            {
                                return ll.tailIdx;
                            }
                            for (int j = 0, jend = ll.chlist.Count; j < jend; j++)
                            {
                                var mm = ll.chlist[j];
                                if (localPoint.y < (mm.pos.y - sph))
                                {
                                    continue;
                                }
                                if (localPoint.y >= (mm.pos.y + mm.siz.y * 0.5f))
                                {
                                    return mm.index;
                                }
                                else
                                {
                                    var jn = j + 1;
                                    if (jn >= jend)
                                    {
                                        return ll.tailIdx;
                                    }
                                    return ll.chlist[jn].index;
                                }
                            }
                        }
                        break;
                    }
            }
            return -1;
        }
        public Rect GetRect(int index)
        {
            if ((texelFont == null) || !texelFont.Valid)
            {
                return default;
            }
            UpdateText();
            if ((index < 0) || (index >= text.Length))
            {
                return default;
            }
            for (int i = 0; i < lineCount; i++)
            {
                var ll = lines[i];
                if (index > ll.tailIdx)
                {
                    continue;
                }
                for (int j = 0; j < ll.chlist.Count; j++)
                {
                    var mm = ll.chlist[j];
                    if (mm.index == index)
                    {
                        return new Rect(mm.pos, mm.psz);
                    }
                }
                break;
            }
            return default;
        }
        public Rect GetCursorRect(int index)
        {
            if ((texelFont == null) || !texelFont.Valid)
            {
                return default;
            }
            UpdateText();
            if (lineCount <= 0)
            {
                return default;
            }
            index = Mathf.Clamp(index, 0, text.Length);
            for (int i = 0; i < lineCount; i++)
            {
                var ll = lines[i];
                if (index > ll.tailIdx)
                {
                    continue;
                }
                for (int j = 0; j < ll.chlist.Count; j++)
                {
                    var mm = ll.chlist[j];
                    if (mm.index >= index)
                    {
                        return new Rect(mm.pos, new Vector2(texelFont.Width, texelFont.Height) * mm.scale);
                    }
                }
                return new Rect(ll.tailPos, new Vector2(texelFont.Width, texelFont.Height) * ll.tailScl);
            }
            return default;
        }
        private void UpdateText()
        {
            if ((fontSize <= 0) || (texelFont == null) || !texelFont.Valid)
            {
                boundingRect = default;
                return;
            }
            var rect = rectTransform.rect;
            if (this.rect != rect)
            {
                this.rect = rect;
                dirtyText = true;
            }
            if (!dirtyText)
            {
                return;
            }
            dirtyText = false;
            if (text == null)
            {
                text = "";
            }
            if (direction == DirectionType.TopToBottom)
            {
                BuildVertical();
            }
            else
            {
                BuildHorizontal();
            }
        }
        private void ClearMesh()
        {
            vertices.Clear();
            indices.Clear();
        }
        private void AddVertex(float x, float y, float u, float v, Color color)
        {
            var vert = UIVertex.simpleVert;
            vert.position = new Vector3(x, y, 0.0f);
            vert.color = color;
            vert.uv0 = new Vector2(u, v);
            vertices.Add(vert);
        }
        private void AddQuad(float x, float y, float w, float h, float u, float v, float uw, float vh, Color color)
        {
            var idx = vertices.Count;
            AddVertex(x, y, u, v, color);
            AddVertex(x, y + h, u, v + vh, color);
            AddVertex(x + w, y, u + uw, v, color);
            AddVertex(x + w, y + h, u + uw, v + vh, color);
            indices.Add(idx + 0);
            indices.Add(idx + 1);
            indices.Add(idx + 2);
            indices.Add(idx + 2);
            indices.Add(idx + 1);
            indices.Add(idx + 3);
        }
        private void BuildMesh()
        {
            if (color.a == 0)
            {
                return;
            }
            if ((texelFont == null) || !texelFont.Valid)
            {
                return;
            }
            UpdateText();
            var iw = 1.0f / texelFont.Texture.width;
            var ih = 1.0f / texelFont.Texture.height;
            {
                var tex = new Vector2(texelFont.BackgroundUV.xMin * iw, 1.0f - texelFont.BackgroundUV.yMax * ih);
                var tsz = new Vector2(texelFont.BackgroundUV.width * iw, texelFont.BackgroundUV.height * ih);
                for (int i = 0; i < lines.Count; i++)
                {
                    var ll = lines[i];
                    for (int j = 0; j < ll.chlist.Count; j++)
                    {
                        var mm = ll.chlist[j];
                        if (mm.bgColor.a != 0)
                        {
                            var pos = mm.pos - mm.spc * 0.5f;
                            var psz = mm.siz + mm.spc;
                            AddQuad(pos.x, pos.y, psz.x, psz.y, tex.x, tex.y, tsz.x, tsz.y, mm.bgColor * color);
                        }
                    }
                }
            }
            {
                var tsz = new Vector2(texelFont.DWidth * iw, texelFont.DHeight * ih);
                for (int i = 0; i < lines.Count; i++)
                {
                    var ll = lines[i];
                    for (int j = 0; j < ll.chlist.Count; j++)
                    {
                        var mm = ll.chlist[j];
                        if (mm.fgColor.a != 0)
                        {
                            var pos = mm.pos + mm.ofs;
                            AddQuad(pos.x, pos.y, mm.psz.x, mm.psz.y, mm.tex.x, mm.tex.y, tsz.x, tsz.y, mm.fgColor * color);
                        }
                    }
                }
            }
        }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            ClearMesh();
            if (!string.IsNullOrEmpty(text) && isActiveAndEnabled)
            {
                BuildMesh();
            }
            vh.AddUIVertexStream(vertices, indices);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            DirtyText();
        }
#endif
    }
}

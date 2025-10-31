using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;


namespace LazyUI
{
    /// <summary>
    /// LazyText用の簡易なinputfield
    /// tmp_inputfieldを参考に似た感じに仕上げてみたけれど違いもあるので注意。
    /// モバイルは未検証
    /// </summary>
    public class LazyInputField : Selectable,
        IUpdateSelectedHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler,
        IPointerClickHandler,
        ISubmitHandler,
        ICancelHandler
    {
        public delegate char OnValidateInputDelegate(string text, int charIndex, char addedChar);

        [Serializable]
        public class SubmitEvent : UnityEvent<string> { }

        [Serializable]
        public class ChangedEvent : UnityEvent<string> { }

        [Serializable]
        public class SelectionEvent : UnityEvent<string> { }

        [Serializable]
        public class TouchScreenKeyboardEvent : UnityEvent<TouchScreenKeyboard.Status> { }

        public enum CharacterValidation
        {
            None,
            Digit,
            Integer,
            Decimal,
            Alphanumeric,
            EmailAddress,
        }
        public enum LineModeType
        {
            SingleLine,
            MultiLineNewline,
            MultiLineSubmit,
        }

        [SerializeField]
        private LazyText textComponent = null;

        [SerializeField, TextArea, LazyRawString]
        private string text = "";

        [SerializeField]
        private bool readOnly = false;

        [SerializeField]
        private LineModeType lineMode = LineModeType.MultiLineSubmit;

        [SerializeField]
        private CharacterValidation validation = CharacterValidation.None;

        [SerializeField]
        private string convertFrom = "";

        [SerializeField]
        private string convertTo = "";

        [SerializeField]
        private string validChars = "";

        [SerializeField]
        private int characterLimit = -1;

        [SerializeField]
        private bool richText = true;

        [SerializeField]
        private bool masking = false;

        [SerializeField]
        private char mask = '*';

        [SerializeField]
        private TouchScreenKeyboardType keyboardType = TouchScreenKeyboardType.Default;

        [SerializeField]
        private RectTransform caret = null;

        [SerializeField, Tooltip("scaling disabled for values below 0")]
        private float caretScale = 1.0f;

        [SerializeField]
        private bool adaptiveCaret = true;

        [SerializeField]
        private bool hideSoftKeyboard = false;

        [SerializeField]
        private bool hideMobileInput = false;

        [SerializeField]
        private Color editingFgColor = new(0, 0, 0, 1.0f);

        [SerializeField]
        private Color editingBgColor = new(0, 0, 0, 0);

        [SerializeField]
        private Color selectionFgColor = new(0, 0, 0, 1);

        [SerializeField]
        private Color selectionBgColor = new(168f / 255f, 206f / 255f, 255f / 255f, 192f / 255f);

        [SerializeField]
        private bool onFocusSelectAll = true;

        [SerializeField]
        private bool restoreTextOnCancel = true;


        //[SerializeField]
        private SubmitEvent m_onEndEdit = new();

        [SerializeField]
        private SubmitEvent m_onSubmit = new();

        //[SerializeField]
        private SelectionEvent m_onSelect = new();

        //[SerializeField]
        private SelectionEvent m_onDeselect = new();

        [SerializeField]
        private ChangedEvent m_onValueChanged = new();

        //[SerializeField]
        private TouchScreenKeyboardEvent m_onTouchScreenKeyboardStatusChanged = new();


        private string originalText = "";
        private bool richTextBak = false;

        private bool hasFocus = false;
        private bool dragging = false;
        private int cursorPos = 0;
        private int cursorJmp = -1;
        private int selectionTop = -1;
        private int selectionEnd = -1;

        private bool dirtyVisuals = true;
        private bool activateInputField = false;
        private bool compositing = false;
        private bool selectionStateSelected = false;

        private string compositionString = "";
        private TouchScreenKeyboard softKeyboard = null;

        private int undoPos = 0;
        private readonly List<string> undoBuf = new();
        private readonly StringBuilder sb = new();

        private readonly Event currentEvent = new();

        public LazyText TextComponent
        {
            get
            {
                return textComponent;
            }
            set
            {
                if (textComponent == value)
                {
                    return;
                }
                if (isActiveAndEnabled && (textComponent != null))
                {
                    textComponent.OverrideAction -= OnOverrideAction;
                }
                textComponent = value;
                if (isActiveAndEnabled && (textComponent != null))
                {
                    textComponent.OverrideAction += OnOverrideAction;
                }
                DirtyVisuals();
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
                SetText(value);
            }
        }
        public bool ReadOnly
        {
            get
            {
                return readOnly;
            }
            set
            {
                if (readOnly == value)
                {
                    return;
                }
                readOnly = value;
                DeactivateInputField();
            }
        }
        public LineModeType LineMode
        {
            get
            {
                return lineMode;
            }
            set
            {
                if (lineMode == value)
                {
                    return;
                }
                lineMode = value;
                DeactivateInputField();
            }
        }
        public CharacterValidation Validation
        {
            get
            {
                return validation;
            }
            set
            {
                if (validation == value)
                {
                    return;
                }
                validation = value;
                DeactivateInputField();
            }
        }
        public string ConvertFrom
        {
            get
            {
                return convertFrom;
            }
            set
            {
                if (convertFrom == value)
                {
                    return;
                }
                convertFrom = value;
                DeactivateInputField();
            }
        }
        public string ConvertTo
        {
            get
            {
                return convertTo;
            }
            set
            {
                if (convertTo == value)
                {
                    return;
                }
                convertTo = value;
                DeactivateInputField();
            }
        }
        public string ValidChars
        {
            get
            {
                return validChars;
            }
            set
            {
                if (validChars == value)
                {
                    return;
                }
                validChars = value;
                DeactivateInputField();
            }
        }
        public int CharacterLimit
        {
            get
            {
                return characterLimit;
            }
            set
            {
                value = Math.Max(value, 0);
                if (characterLimit == value)
                {
                    return;
                }
                if (softKeyboard != null)
                {
                    softKeyboard.characterLimit = value;
                }
                DirtyVisuals();
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
                richText = value;
                DirtyVisuals();
            }
        }
        public bool Masking
        {
            get
            {
                return masking;
            }
            set
            {
                if (masking == value)
                {
                    return;
                }
                masking = value;
                DirtyVisuals();
            }
        }
        public char Mask
        {
            get
            {
                return mask;
            }
            set
            {
                if (mask == value)
                {
                    return;
                }
                mask = value;
                DirtyVisuals();
            }
        }
        public TouchScreenKeyboardType KeyboardType
        {
            get
            {
                return keyboardType;
            }
            set
            {
                if (keyboardType != value)
                {
                    keyboardType = value;
                }
            }
        }
        public RectTransform Caret
        {
            get
            {
                return caret;
            }
            set
            {
                if (caret == value)
                {
                    return;
                }
                caret = value;
                DirtyVisuals();
            }
        }
        public float CaretScale
        {
            get
            {
                return caretScale;
            }
            set
            {
                if (caretScale == value)
                {
                    return;
                }
                caretScale = value;
                DirtyVisuals();
            }
        }
        public bool AdaptiveCaret
        {
            get
            {
                return adaptiveCaret;
            }
            set
            {
                if (adaptiveCaret == value)
                {
                    return;
                }
                adaptiveCaret = value;
                DirtyVisuals();
            }
        }


        public bool HideSoftKeyboard
        {
            get
            {
                return hideSoftKeyboard;
            }
            set
            {
                if (hideSoftKeyboard == value)
                {
                    return;
                }
                hideSoftKeyboard = value;
                if (hideSoftKeyboard && (softKeyboard != null))
                {
                    softKeyboard.active = false;
                    softKeyboard = null;
                }
            }
        }
        public bool HideMobileInput
        {
            get
            {
                return hideMobileInput;
            }
            set
            {
                if (hideMobileInput == value)
                {
                    return;
                }
                hideMobileInput = value;
                if (softKeyboard != null)
                {
                    TouchScreenKeyboard.hideInput = hideMobileInput;
                }
            }
        }
        public Color EditingFgColor
        {
            get
            {
                return editingFgColor;
            }
            set
            {
                if (editingFgColor == value)
                {
                    return;
                }
                editingFgColor = value;
                DirtyVisuals();
            }
        }
        public Color EditingBgColor
        {
            get
            {
                return editingBgColor;
            }
            set
            {
                if (editingBgColor == value)
                {
                    return;
                }
                editingBgColor = value;
                DirtyVisuals();
            }
        }
        public Color SelectionFgColor
        {
            get
            {
                return selectionFgColor;
            }
            set
            {
                if (selectionFgColor == value)
                {
                    return;
                }
                selectionFgColor = value;
                DirtyVisuals();
            }
        }
        public Color SelectionBgColor
        {
            get
            {
                return selectionBgColor;
            }
            set
            {
                if (selectionBgColor == value)
                {
                    return;
                }
                selectionBgColor = value;
                DirtyVisuals();
            }
        }

        public bool OnFocusSelectAll
        {
            get { return onFocusSelectAll; }
            set { onFocusSelectAll = value; }
        }

        public bool RestoreTextOnEscape
        {
            get { return restoreTextOnCancel; }
            set { restoreTextOnCancel = value; }
        }

        public SubmitEvent onEndEdit => m_onEndEdit;

        public SubmitEvent onSubmit => m_onSubmit;

        public SelectionEvent onSelect => m_onSelect;

        public SelectionEvent onDeselect => m_onDeselect;

        public ChangedEvent onValueChanged => m_onValueChanged;

        public TouchScreenKeyboardEvent onTouchScreenKeyboardStatusChanged => m_onTouchScreenKeyboardStatusChanged;

        public OnValidateInputDelegate OnValidateInput
        {
            get;
            set;
        }

        public int CursorPosition
        {
            get
            {
                return (cursorPos >= 0) ? cursorPos : selectionTop;
            }
            set
            {
                cursorPos = Mathf.Clamp(value, 0, text.Length);
                cursorJmp = -1;
                selectionTop = -1;
                selectionEnd = -1;
                DirtyVisuals();
            }
        }
        public bool HasFocus
        {
            get
            {
                return hasFocus;
            }
        }

        private BaseInput InputSystem
        {
            get
            {
                if ((EventSystem.current == null) || (EventSystem.current.currentInputModule == null))
                {
                    return null;
                }
                return EventSystem.current.currentInputModule.input;
            }
        }

        private bool HasSelection
        {
            get
            {
                return (selectionTop >= 0) && (selectionEnd >= selectionTop);
            }
        }

        private static string Clipboard
        {
            get
            {
                return GUIUtility.systemCopyBuffer;
            }
            set
            {
                GUIUtility.systemCopyBuffer = value;
            }
        }

        private LazyText.DirectionType Direction
        {
            get
            {
                return (textComponent != null) ? textComponent.Direction : LazyText.DirectionType.LeftToRight;
            }
        }

        private bool InPlaceEditing
        {
            get
            {
                if (TouchScreenKeyboard.isSupported)
                {
                    if (!TouchScreenKeyboard.isInPlaceEditingAllowed)
                    {
                        return false;
                    }
                    if (!HideSoftKeyboard && !HideMobileInput)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        private void DirtyVisuals()
        {
            LazyDebug.Assert(HasSelection || (cursorPos >= 0));
            dirtyVisuals = true;
        }

        private char pvch = '\0';
        private char Validate(string text, int pos, char ch)
        {
            var pvch = this.pvch;
            this.pvch = ch;
            if (!string.IsNullOrEmpty(convertFrom) && !string.IsNullOrEmpty(convertTo))
            {
                var idx = convertFrom.IndexOf(ch);
                if ((idx >= 0) && (idx < convertTo.Length))
                {
                    ch = convertTo[idx];
                }
            }
            if ((ch == '\n') && (pvch == '\r'))
            {
                return '\0';
            }
            if (ch == '\r')
            {
                ch = '\n';
            }
            if ((ch != '\n') && !string.IsNullOrEmpty(validChars))
            {
                if (0 > validChars.IndexOf(ch))
                {
                    return '\0';
                }
            }
            if (OnValidateInput != null)
            {
                ch = OnValidateInput.Invoke(text, pos, ch);
                if (ch == '\0')
                {
                    return '\0';
                }
            }
            if ((ch == '\n') && (lineMode == LineModeType.SingleLine))
            {
                return '\0';
            }
            switch (validation)
            {
                case CharacterValidation.None:
                    return ch;
                case CharacterValidation.Digit:
                    if ((ch >= '0') && (ch <= '9'))
                    {
                        return ch;
                    }
                    return '\0';
                case CharacterValidation.Integer:
                    if ((ch == '+') || (ch == '-'))
                    {
                        var cc = (text.Length == 0) ? '\0' : text[0];
                        if ((pos != 0) || (cc == '+') || (cc == '-'))
                        {
                            return '\0';
                        }
                        return ch;
                    }
                    if ((ch >= '0') && (ch <= '9'))
                    {
                        var cc = (text.Length == 0) ? '\0' : text[0];
                        if ((pos == 0) && ((cc == '+') || (cc == '-')))
                        {
                            return '\0';
                        }
                        return ch;
                    }
                    return '\0';
                case CharacterValidation.Decimal:
                    if ((ch == '+') || (ch == '-'))
                    {
                        var cc = (text.Length == 0) ? '\0' : text[0];
                        if ((pos != 0) || (cc == '+') || (cc == '-'))
                        {
                            return '\0';
                        }
                        return ch;
                    }
                    if ((ch >= '0') && (ch <= '9'))
                    {
                        var cc = (text.Length == 0) ? '\0' : text[0];
                        if ((pos == 0) && ((cc == '+') || (cc == '-')))
                        {
                            return '\0';
                        }
                        return ch;
                    }
                    if (Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator.IndexOf(ch) >= 0)
                    {
                        return ch;
                    }
                    return '\0';
                case CharacterValidation.Alphanumeric:
                    if ((ch >= 'a') && (ch <= 'z'))
                    {
                        return ch;
                    }
                    if ((ch >= 'A') && (ch <= 'Z'))
                    {
                        return ch;
                    }
                    if ((ch >= '0') && (ch <= '9'))
                    {
                        return ch;
                    }
                    return '\0';
                case CharacterValidation.EmailAddress:
                    {
                        var pch = (pos <= 0) ? '\0' : text[pos - 1];
                        static bool ld(char ch)
                        {
                            if ((ch >= 'a') && (ch <= 'z'))
                            {
                                return true;
                            }
                            if ((ch >= 'A') && (ch <= 'Z'))
                            {
                                return true;
                            }
                            if ((ch >= '0') && (ch <= '9'))
                            {
                                return true;
                            }
                            return false;
                        }
                        static bool sp(char ch)
                        {
                            if ("!#$%&'*+-/=?^_`{|}~".IndexOf(ch) < 0)
                            {
                                return false;
                            }
                            return true;
                        }
                        if (ch == '@')
                        {
                            if ((pos == 0) || (pch == '.'))
                            {
                                return '\0';
                            }
                            if (text.Contains('@'))
                            {
                                return '\0';
                            }
                        }
                        else if (ch == '.')
                        {
                            if (text.Contains('@'))
                            {
                                if ((pch == '@') || (pch == '-') || (pch == '.'))
                                {
                                    return '\0';
                                }
                            }
                            else
                            {
                                if ((pos == 0) || (pch == '-') || (pch == '.'))
                                {
                                    return '\0';
                                }
                            }
                        }
                        else if (ch == '-')
                        {
                            if (text.Contains('@'))
                            {
                                if ((pch == '@') || (pch == '-') || (pch == '.'))
                                {
                                    return '\0';
                                }
                            }
                        }
                        else if (ld(ch))
                        {
                            ch = char.ToLower(ch);
                        }
                        else if (sp(ch))
                        {
                            if (text.Contains('@'))
                            {
                                return '\0';
                            }
                        }
                        else
                        {
                            return '\0';
                        }
                        return ch;
                    }
                default:
                    return ch;
            }
        }
        private string FilterText(string str)
        {
            if (!masking)
            {
                return str;
            }
            for (int i = 0, end = str.Length; i < end; i++)
            {
                var ch = str[i];
                if (ch != '\n')
                {
                    ch = mask;
                }
                sb.Append(ch);
            }
            str = sb.ToString();
            sb.Clear();
            return str;
        }
        private void Cancel()
        {
            ReleaseSelection();
            if (restoreTextOnCancel)
            {
                ApplyText(originalText, true, false);
                DirtyVisuals();
            }
        }
        private void ResetUndo()
        {
            undoBuf.Clear();
            undoBuf.Add(text);
            undoPos = 0;
        }
        private void Undo()
        {
            if (undoPos <= 0)
            {
                return;
            }
            undoPos--;
            ApplyText(undoBuf[undoPos], true, false);
        }
        private void Redo()
        {
            var up = undoPos + 1;
            if (up >= undoBuf.Count)
            {
                return;
            }
            undoPos = up;
            ApplyText(undoBuf[undoPos], true, false);
        }
        private void ApplyText(string value, bool notify = true, bool register = true)
        {
            if (value == null)
            {
                value = "";
            }
            if (HasSelection)
            {
                if (cursorPos < 0)
                {
                    cursorPos = selectionTop;
                }
                selectionTop = -1;
                selectionEnd = -1;
                DirtyVisuals();
            }
            cursorJmp = -1;
            if (text.Equals(value, StringComparison.Ordinal))
            {
                return;
            }
            text = value;
            if (register)
            {
                if (undoPos < undoBuf.Count)
                {
                    undoBuf.RemoveRange(undoPos, undoBuf.Count - undoPos - 1);
                }
                undoBuf.Add(text);
                undoPos++;
            }
            DirtyVisuals();
            if (notify)
            {
                m_onValueChanged?.Invoke(text);
            }
        }
        private string AppendText(string txt, char ch)
        {
            if ((characterLimit >= 0) && (txt.Length >= characterLimit))
            {
                return txt;
            }
            ch = Validate(txt, cursorPos, ch);
            if (ch != '\0')
            {
                txt = txt[..cursorPos] + ch + txt[cursorPos..];
                cursorPos++;
            }
            return txt;
        }
        public void Append(char ch)
        {
            if (readOnly)
            {
                return;
            }
            var txt = text;
            if (HasSelection)
            {
                if (cursorPos < 0)
                {
                    cursorPos = selectionTop;
                }
                txt = txt[..selectionTop] + txt[selectionEnd..];
                selectionTop = -1;
                selectionEnd = -1;
            }
            txt = AppendText(txt, ch);
            ApplyText(txt);
        }
        public void Append(string str)
        {
            if (readOnly)
            {
                return;
            }
            var txt = text;
            if (HasSelection)
            {
                if (cursorPos < 0)
                {
                    cursorPos = selectionTop;
                }
                txt = txt[..selectionTop] + txt[selectionEnd..];
                selectionTop = -1;
                selectionEnd = -1;
            }
            if (str != null)
            {
                for (int i = 0, end = str.Length; i < end; i++)
                {
                    var ch = str[i];
                    txt = AppendText(txt, ch);
                }
            }
            ApplyText(txt);
        }

        public void SetText(string value)
        {
            SelectAll();
            Append(value);
            ResetUndo();
        }
        public void SetTextWithoutNotify(string value)
        {
            var bk = m_onValueChanged;
            m_onValueChanged = null;
            SelectAll();
            Append(value);
            ResetUndo();
            m_onValueChanged = bk;
        }
        public void SelectAll()
        {
            cursorPos = -1;
            cursorJmp = -1;
            selectionTop = 0;
            selectionEnd = text.Length;
            DirtyVisuals();
        }
        private enum WordType
        {
            Word,
            Symbol,
            Control,
            WhiteSpace,
        }
        private static WordType GetWordType(char ch)
        {
            if (char.IsControl(ch))
            {
                return WordType.Control;
            }
            if (char.IsWhiteSpace(ch))
            {
                return WordType.WhiteSpace;
            }
            if (char.IsSymbol(ch))
            {
                return WordType.Symbol;
            }
            return WordType.Word;
        }
        public void SelectWord(int index)
        {
            if ((index < 0) || (index >= text.Length))
            {
                SelectAll();
            }
            else
            {
                selectionTop = index;
                selectionEnd = index + 1;
                var ch = text[index];
                var wt = GetWordType(ch);
                if (wt != WordType.Control)
                {
                    for (int i = index - 1; i >= 0; i--)
                    {
                        var cc = text[i];
                        var ww = GetWordType(cc);
                        if (ww != wt)
                        {
                            break;
                        }
                        selectionTop = i;
                    }
                    for (int i = index + 1; i < text.Length; i++)
                    {
                        var cc = text[i];
                        var ww = GetWordType(cc);
                        if (ww != wt)
                        {
                            break;
                        }
                        selectionEnd = i + 1;
                    }
                }
                cursorPos = -1;
                cursorJmp = -1;
                DirtyVisuals();
            }
        }
        public void ReleaseSelection()
        {
            if (cursorPos < 0)
            {
                cursorPos = selectionTop;
            }
            cursorJmp = -1;
            selectionTop = -1;
            selectionEnd = -1;
            DirtyVisuals();
        }
        private void MoveTo(int index, bool shift, bool clear, bool forward)
        {
            if (clear)
            {
                cursorJmp = -1;
            }
            index = Mathf.Clamp(index, 0, text.Length);
            if (textComponent != null)
            {
                index = textComponent.GetStopPosition(index, forward);
            }
            if (!shift)
            {
                selectionTop = -1;
                selectionEnd = -1;
            }
            else
            {
                if (cursorPos < 0)
                {
                    LazyDebug.Assert(HasSelection);
                    var cx = (selectionTop + selectionEnd) / 2;
                    if (index <= cx)
                    {
                        selectionTop = index;
                    }
                    else
                    {
                        selectionEnd = index;
                    }
                }
                else
                {
                    if (selectionTop == cursorPos)
                    {
                        selectionTop = index;
                    }
                    else if (selectionEnd == cursorPos)
                    {
                        selectionEnd = index;
                    }
                    else if (index < cursorPos)
                    {
                        selectionTop = index;
                        selectionEnd = cursorPos;
                    }
                    else if (index > cursorPos)
                    {
                        selectionTop = cursorPos;
                        selectionEnd = index;
                    }
                }
                if (selectionTop > selectionEnd)
                {
                    (selectionTop, selectionEnd) = (selectionEnd, selectionTop);
                }
                if (selectionTop == selectionEnd)
                {
                    selectionTop = -1;
                    selectionEnd = -1;
                }
            }
            cursorPos = index;
            DirtyVisuals();
        }
        public void MoveTextStart(bool shift)
        {
            MoveTo(0, shift, true, false);
        }
        public void MoveTextEnd(bool shift)
        {
            MoveTo(text.Length, shift, true, true);
        }
        public void MoveToStartOfLine(bool shift)
        {
            var p = (cursorPos < 0) ? selectionTop : cursorPos;
            if (p > 0)
            {
                for (int i = p - 1; i >= 0; p = i, i--)
                {
                    var ch = text[i];
                    if (ch == '\n')
                    {
                        break;
                    }
                }
            }
            MoveTo(p, shift, true, false);
        }
        public void MoveToEndOfLine(bool shift)
        {
            var p = (cursorPos < 0) ? selectionEnd : cursorPos;
            var end = text.Length;
            if (p < end)
            {
                for (int i = p; i < end; i++, p = i)
                {
                    var ch = text[i];
                    if (ch == '\n')
                    {
                        break;
                    }
                }
            }
            MoveTo(p, shift, true, true);
        }
        private void MoveBackward(bool shift, bool ctrl)
        {
            var p = (cursorPos < 0) ? selectionTop : cursorPos;
            if (p > 0)
            {
                if (ctrl)
                {
                    p--;
                    var ch = text[p];
                    var wt = GetWordType(ch);
                    if (wt != WordType.Control)
                    {
                        for (int i = p - 1; i >= 0; p = i, i--)
                        {
                            var cc = text[i];
                            var ww = GetWordType(cc);
                            if (ww != wt)
                            {
                                break;
                            }
                        }
                    }
                }
                else if (HasSelection && !shift)
                {
                    p = selectionTop;
                }
                else
                {
                    p--;
                }
            }
            MoveTo(p, shift, true, false);
        }
        private void MoveForward(bool shift, bool ctrl)
        {
            var p = (cursorPos < 0) ? selectionEnd : cursorPos;
            var end = text.Length;
            if (p < end)
            {
                if (ctrl)
                {
                    var ch = text[p];
                    var wt = GetWordType(ch);
                    if (wt != WordType.Control)
                    {
                        for (int i = p + 1; i < end; p = i, i++)
                        {
                            var cc = text[i];
                            var ww = GetWordType(cc);
                            if (ww != wt)
                            {
                                break;
                            }
                        }
                    }
                    p++;
                }
                else if (HasSelection && !shift)
                {
                    p = selectionEnd;
                }
                else
                {
                    p++;
                }
            }
            MoveTo(p, shift, true, true);
        }
        private void MoveLower(bool shift)
        {
            var p = (cursorPos < 0) ? selectionTop : cursorPos;
            if (p > 0)
            {
                var t = p - 1;
                for (; t >= 0; t--)
                {
                    var ch = text[t];
                    if (ch == '\n')
                    {
                        break;
                    }
                }
                if (cursorJmp < 0)
                {
                    cursorJmp = p - t - 1;
                }
                if (t == 0)
                {
                    p = 0;
                }
                else if (t > 0)
                {
                    var h = t - 1;
                    for (; h >= 0; h--)
                    {
                        var ch = text[h];
                        if (ch == '\n')
                        {
                            break;
                        }
                    }
                    p = Mathf.Min(h + 1 + cursorJmp, t);
                }
            }
            MoveTo(p, shift, false, false);
        }
        private void MoveUpper(bool shift)
        {
            var p = (cursorPos < 0) ? selectionEnd : cursorPos;
            var end = text.Length;
            if (p < end)
            {
                if (cursorJmp < 0)
                {
                    var t = p - 1;
                    for (; t >= 0; t--)
                    {
                        var ch = text[t];
                        if (ch == '\n')
                        {
                            break;
                        }
                    }
                    cursorJmp = p - t - 1;
                }
                var n = p;
                for (; n < end; n++)
                {
                    var ch = text[n];
                    if (ch == '\n')
                    {
                        break;
                    }
                }
                if (n < end)
                {
                    var l = n + 1;
                    var le = Mathf.Min(l + cursorJmp, end);
                    for (; l < le; l++)
                    {
                        var ch = text[l];
                        if (ch == '\n')
                        {
                            break;
                        }
                    }
                    p = l;
                }
            }
            MoveTo(p, shift, false, true);
        }
        public void MoveLeft(bool shift, bool ctrl)
        {
            switch (Direction)
            {
                default:
                case LazyText.DirectionType.LeftToRight:
                    MoveBackward(shift, ctrl);
                    break;
                case LazyText.DirectionType.RightToLeft:
                    MoveForward(shift, ctrl);
                    break;
                case LazyText.DirectionType.TopToBottom:
                    MoveUpper(shift);
                    break;
            }
        }
        public void MoveRight(bool shift, bool ctrl)
        {
            switch (Direction)
            {
                default:
                case LazyText.DirectionType.LeftToRight:
                    MoveForward(shift, ctrl);
                    break;
                case LazyText.DirectionType.RightToLeft:
                    MoveBackward(shift, ctrl);
                    break;
                case LazyText.DirectionType.TopToBottom:
                    MoveLower(shift);
                    break;
            }
        }
        public void MoveUp(bool shift, bool ctrl)
        {
            switch (Direction)
            {
                default:
                case LazyText.DirectionType.LeftToRight:
                    MoveLower(shift);
                    break;
                case LazyText.DirectionType.RightToLeft:
                    MoveLower(shift);
                    break;
                case LazyText.DirectionType.TopToBottom:
                    MoveBackward(shift, ctrl);
                    break;
            }
        }
        public void MoveDown(bool shift, bool ctrl)
        {
            switch (Direction)
            {
                default:
                case LazyText.DirectionType.LeftToRight:
                    MoveUpper(shift);
                    break;
                case LazyText.DirectionType.RightToLeft:
                    MoveUpper(shift);
                    break;
                case LazyText.DirectionType.TopToBottom:
                    MoveForward(shift, ctrl);
                    break;
            }
        }
#if false
        public void MovePageUp(bool shift)
        {
            // not implemented
        }
        public void MovePageDown(bool shift)
        {
            // not implemented
        }
#endif
        public void Backspace()
        {
            if (readOnly)
            {
                ReleaseSelection();
                return;
            }
            var begin = selectionTop;
            var end = selectionEnd;
            if (begin < 0)
            {
                begin = cursorPos - 1;
                end = cursorPos;
                if (begin < 0)
                {
                    return;
                }
            }
            cursorPos = begin;
            cursorJmp = -1;
            selectionTop = -1;
            selectionEnd = -1;
            var txt = text[..begin] + text[end..];
            ApplyText(txt);
        }
        public void Delete()
        {
            if (readOnly)
            {
                ReleaseSelection();
                return;
            }
            var begin = selectionTop;
            var end = selectionEnd;
            if (begin < 0)
            {
                begin = cursorPos;
                end = cursorPos + 1;
            }
            cursorPos = begin;
            cursorJmp = -1;
            selectionTop = -1;
            selectionEnd = -1;
            var txt = text[..begin] + text[end..];
            ApplyText(txt);
        }
        public void Copy()
        {
            if (masking)
            {
                Clipboard = "";
            }
            else
            {
                Clipboard = GetSelectedString();
            }
        }
        public void Cut()
        {
            Copy();
            if (readOnly)
            {
                ReleaseSelection();
                return;
            }
            var begin = selectionTop;
            var end = selectionEnd + 1;
            if (begin < 0)
            {
                begin = 0;
                end = text.Length;
            }
            cursorPos = begin;
            cursorJmp = -1;
            selectionTop = -1;
            selectionEnd = -1;
            var txt = text[..begin] + text[end..];
            ApplyText(txt);
        }
        public void Paste()
        {
            if (readOnly)
            {
                ReleaseSelection();
                return;
            }
            Append(Clipboard);
        }
        public void Enter()
        {
            if (lineMode != LineModeType.MultiLineNewline)
            {
                Submit();
                DeactivateInputField();
            }
        }
        public void Escape()
        {
            Cancel();
            DeactivateInputField();
        }

        public string GetSelectedString()
        {
            if (HasSelection)
            {
                return text[selectionTop..selectionEnd];
            }
            else
            {
                return text;
            }
        }

        protected void ProcessKeyDownEvent(Event evt)
        {
            //LazyDebug.Log($"{evt}");
            var modifiers = evt.modifiers & (EventModifiers.Control | EventModifiers.Shift | EventModifiers.Alt | EventModifiers.Command);
            var ctrl = (modifiers & EventModifiers.Control) != 0;
            var shift = (modifiers & EventModifiers.Shift) != 0;
            var alt = (modifiers & EventModifiers.Alt) != 0;
            var cmd = (modifiers & EventModifiers.Command) != 0;
            if (alt || cmd)
            {
                return;
            }
            switch (evt.keyCode)
            {
                case KeyCode.A:
                    if (modifiers == EventModifiers.Control)
                    {
                        SelectAll();
                    }
                    break;
                case KeyCode.C:
                    if (modifiers == EventModifiers.Control)
                    {
                        Copy();
                    }
                    break;
                case KeyCode.V:
                    if (modifiers == EventModifiers.Control)
                    {
                        Paste();
                    }
                    break;
                case KeyCode.X:
                    if (modifiers == EventModifiers.Control)
                    {
                        Cut();
                    }
                    break;
                case KeyCode.Y:
                    if (modifiers == EventModifiers.Control)
                    {
                        Redo();
                    }
                    break;
                case KeyCode.Z:
                    if (modifiers == EventModifiers.Control)
                    {
                        Undo();
                    }
                    break;
                case KeyCode.Backspace:
                    if (modifiers == EventModifiers.None)
                    {
                        Backspace();
                    }
                    break;
                case KeyCode.Delete:
                    if (modifiers == EventModifiers.None)
                    {
                        Delete();
                    }
                    break;
                case KeyCode.Home:
                    if (ctrl)
                    {
                        MoveTextStart(shift);
                    }
                    else
                    {
                        MoveToStartOfLine(shift);
                    }
                    break;
                case KeyCode.End:
                    if (ctrl)
                    {
                        MoveTextEnd(shift);
                    }
                    else
                    {
                        MoveToEndOfLine(shift);
                    }
                    break;
                case KeyCode.LeftArrow:
                    {
                        MoveLeft(shift, ctrl);
                    }
                    break;
                case KeyCode.RightArrow:
                    {
                        MoveRight(shift, ctrl);
                    }
                    break;
                case KeyCode.UpArrow:
                    {
                        MoveUp(shift, ctrl);
                    }
                    break;
                case KeyCode.DownArrow:
                    {
                        MoveDown(shift, ctrl);
                    }
                    break;
#if false
                case KeyCode.PageUp:
                    {
                        MovePageUp(shift);
                    }
                    break;
                case KeyCode.PageDown:
                    {
                        MovePageDown(shift);
                    }
                    break;
#endif
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if (modifiers == EventModifiers.None)
                    {
                        Enter();
                    }
                    break;
                case KeyCode.Escape:
                    if (modifiers == EventModifiers.None)
                    {
                        Escape();
                    }
                    break;
                case KeyCode.None:
                    if (!ctrl)
                    {
                        var ch = evt.character;
                        if (ch != 0)
                        {
                            Append(ch);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void SetFocus()
        {
            if (hasFocus)
            {
                return;
            }
            hasFocus = true;
            if (onFocusSelectAll)
            {
                SelectAll();
            }
        }
        private void LostFocus()
        {
            if (!hasFocus)
            {
                return;
            }
            hasFocus = false;
        }
        private void ActivateInputField()
        {
            if (hasFocus)
            {
                return;
            }
            if (EventSystem.current == null)
            {
                return;
            }
            if (EventSystem.current.currentSelectedGameObject != gameObject)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
            if (textComponent != null)
            {
                richTextBak = textComponent.RichText;
                if (!richText)
                {
                    textComponent.RichText = false;
                }
            }
            originalText = text;
            {
                var cp = cursorPos;
                var st = selectionTop;
                var se = selectionEnd;
                SetTextWithoutNotify(text);
                var tl = text.Length;
                cursorPos = Mathf.Min(cp, tl);
                selectionTop = Mathf.Min(st, tl);
                selectionEnd = Mathf.Min(se, tl);
            }
            if (TouchScreenKeyboard.isSupported)
            {
                if (!HideSoftKeyboard)
                {
                    if ((InputSystem != null) && InputSystem.touchSupported)
                    {
                        TouchScreenKeyboard.hideInput = HideMobileInput;
                    }
                    var multiLine = LineMode != LineModeType.SingleLine;
                    softKeyboard = TouchScreenKeyboard.Open(text, keyboardType, false, multiLine, masking, false, "", Mathf.Max(0, characterLimit));

                    SetFocus();
                    if (softKeyboard != null)
                    {
                        onTouchScreenKeyboardStatusChanged?.Invoke(softKeyboard.status);
                        if (HasSelection)
                        {
                            softKeyboard.selection = new RangeInt(selectionTop, selectionEnd - selectionTop);
                        }
                        else
                        {
                            softKeyboard.selection = new RangeInt(cursorPos, 0);
                        }
                    }
                }
            }
            else
            {
                if (!readOnly && (InputSystem != null))
                {
                    InputSystem.imeCompositionMode = IMECompositionMode.On;
                }
                SetFocus();
            }
            DirtyVisuals();
        }

        private void DeactivateInputField()
        {
            selectionStateSelected = false;
            activateInputField = false;
            if (!hasFocus)
            {
                return;
            }
            if (textComponent != null)
            {
                textComponent.Text = FilterText(text);
                textComponent.RichText = richTextBak;
            }
            ResetUndo();
            LostFocus();
            DirtyVisuals();
            UpdateVisuals();
            if (softKeyboard != null)
            {
                softKeyboard.active = false;
                softKeyboard = null;
            }
            if (InputSystem != null)
            {
                InputSystem.imeCompositionMode = IMECompositionMode.Auto;
            }
            onEndEdit?.Invoke(text);
        }


        private bool MayDrag(PointerEventData eventData)
        {
            return IsActive() && IsInteractable()
                   && (eventData.button == PointerEventData.InputButton.Left)
                   && (textComponent != null)
                   && InPlaceEditing;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
            {
                return;
            }
            dragging = true;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
            {
                return;
            }
            if (!dragging)
            {
                return;
            }
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(textComponent.rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 pos))
            {
                var index = textComponent.GetCursorPosition(pos);
                if (index >= 0)
                {
                    var shift = true;
                    MoveTo(index, shift, true, false);
                }
            }
            eventData.Use();
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
#if false
            if (!MayDrag(eventData))
            {
                return;
            }
#endif
            dragging = false;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!MayDrag(eventData))
            {
                return;
            }
            EventSystem.current.SetSelectedGameObject(gameObject, eventData);
            base.OnPointerDown(eventData);

            var doubleClick = LazyDoubleClicker.OnPointerDown(eventData);
            if (!hasFocus)
            {
                return;
            }
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(textComponent.rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 pos))
            {
                var index = textComponent.GetCursorPosition(pos);
                if (index >= 0)
                {
#if ENABLE_INPUT_SYSTEM
                    bool shift = (currentEvent.modifiers & EventModifiers.Shift) != 0;
#else
                    bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
#endif
                    if (doubleClick && !shift)
                    {
                        SelectWord(index);
                    }
                    else
                    {
                        MoveTo(index, shift, true, false);
                    }
                }
            }
            eventData.Use();
        }
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            activateInputField = true;
        }
        private void Submit()
        {
            if (!HasFocus)
            {
                activateInputField = true;
            }
            onSubmit?.Invoke(text);
        }
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            onSelect?.Invoke(text);
            activateInputField = true;
        }
        public override void OnDeselect(BaseEventData eventData)
        {
            DeactivateInputField();
            base.OnDeselect(eventData);
            onDeselect?.Invoke(text);
        }
        public virtual void OnSubmit(BaseEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
            {
                return;
            }
            Submit();
        }
        public virtual void OnCancel(BaseEventData eventData)
        {
            Cancel();
            DeactivateInputField();
        }
        public override void OnMove(AxisEventData eventData)
        {
            //LazyDebug.Log("OnMove");
            if (HasFocus)
            {
                return;
            }
            base.OnMove(eventData);
        }


        public void OnUpdateSelected(BaseEventData eventData)
        {
            while (HasFocus)
            {
                if (!Event.PopEvent(currentEvent))
                {
                    break;
                }
                switch (currentEvent.rawType)
                {
                    case EventType.KeyUp:
                        break;
                    case EventType.KeyDown:
                        ProcessKeyDownEvent(currentEvent);
                        break;
                }
                eventData.Use();
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            if (selectionStateSelected)
            {
                state = SelectionState.Selected;
            }
            else if (state == SelectionState.Pressed)
            {
                selectionStateSelected = true;
            }
            base.DoStateTransition(state, instant);
        }

        private void UpdateCompositionString()
        {
            var inputSystem = InputSystem;
            var cmp = (inputSystem != null) ? inputSystem.compositionString : Input.compositionString;
            if (cmp == null)
            {
                cmp = "";
            }
            if (!compositionString.Equals(cmp, StringComparison.Ordinal))
            {
                compositionString = cmp;
                dirtyVisuals = true;
            }
        }
        protected void UpdateSoftKeyboard()
        {
            if (softKeyboard == null)
            {
                return;
            }
            if (!readOnly)
            {
                if (!text.Equals(softKeyboard.text, StringComparison.Ordinal))
                {
                    DirtyVisuals();
                }
            }
            {
                int cp;
                int sh;
                int st;
                if (softKeyboard.canGetSelection)
                {
                    var sel = softKeyboard.selection;
                    if (sel.length > 0)
                    {
                        cp = -1;
                        sh = sel.start;
                        st = sel.start + sel.length;
                    }
                    else
                    {
                        cp = Mathf.Clamp(sel.start, 0, text.Length);
                        sh = -1;
                        st = -1;
                    }
                }
                else
                {
                    cp = text.Length;
                    sh = -1;
                    st = -1;
                }
                if ((cursorPos != cp) || (selectionTop != sh) || (selectionEnd != st))
                {
                    cursorPos = cp;
                    cursorJmp = -1;
                    selectionTop = sh;
                    selectionEnd = st;
                    DirtyVisuals();
                }
            }
            if (softKeyboard.status != TouchScreenKeyboard.Status.Visible)
            {
                SetText(softKeyboard.text);
                onTouchScreenKeyboardStatusChanged?.Invoke(softKeyboard.status);
                if (softKeyboard.status == TouchScreenKeyboard.Status.Done)
                {
                    Submit();
                }
                else if (softKeyboard.status == TouchScreenKeyboard.Status.Canceled)
                {
                    Cancel();
                }
                DeactivateInputField();
            }
        }
        private void UpdateCaret()
        {
            if (caret == null)
            {
                return;
            }
            caret.gameObject.SetActive(false);
            if (!HasFocus || (cursorPos < 0))
            {
                return;
            }
            var cp = cursorPos;
            if (compositing)
            {
                cp += compositionString.Length;
            }
            var cr = textComponent.GetCursorRect(cp);
            if ((cr.width == 0) || (cr.height == 0))
            {
                caret.gameObject.SetActive(false);
                return;
            }
            var cpr = caret.parent as RectTransform;
            cr = textComponent.rectTransform.Transform(cr, cpr);
            var px = cpr.rect.size * (cpr.pivot - new Vector2(0.5f, 0.5f));
            var sd = cr.size * ((caretScale <= 0) ? 1 : caretScale);
            var ps = cr.position + px;
            if (adaptiveCaret)
            {
                switch (Direction)
                {
                    default:
                    case LazyText.DirectionType.LeftToRight:
                        caret.localScale = new Vector3(1, 1, 1);
                        caret.localRotation = Quaternion.identity;
                        break;
                    case LazyText.DirectionType.RightToLeft:
                        caret.localScale = new Vector3(-1, 1, 1);
                        caret.localRotation = Quaternion.identity;
                        ps.x += sd.x * (1.0f - caret.pivot.x * 2.0f);
                        break;
                    case LazyText.DirectionType.TopToBottom:
                        caret.localScale = new Vector3(1, 1, 1);
                        caret.localRotation = Quaternion.Euler(0, 0, -90.0f);
                        (sd.x, sd.y) = (sd.y, sd.x);
                        ps.y += sd.y * (1.0f - caret.pivot.x * 2.0f);
                        break;
                }
            }
            caret.anchoredPosition = ps;
            caret.sizeDelta = sd;
            caret.gameObject.SetActive(true);
        }

        private void OnOverrideAction(string text, int index, char ch, ref LazyText.Context context)
        {
            if (!HasFocus)
            {
                return;
            }
            if (compositing)
            {
                var pos = HasSelection ? selectionTop : cursorPos;
                if ((index >= pos) && (index < (pos + compositionString.Length)))
                {
                    context.fgColor = selectionFgColor;
                    context.bgColor = selectionBgColor;
                }
                else
                {
                    context.fgColor = editingFgColor;
                    context.bgColor = editingBgColor;
                }
            }
            else
            {
                if ((index >= selectionTop) && (index < selectionEnd))
                {
                    context.fgColor = selectionFgColor;
                    context.bgColor = selectionBgColor;
                }
                else
                {
                    context.fgColor = editingFgColor;
                    context.bgColor = editingBgColor;
                }
            }
        }
        private void UpdateVisuals()
        {
            if (!dirtyVisuals)
            {
                return;
            }
            dirtyVisuals = false;

            if (textComponent != null)
            {
                compositing = false;
                var txt = text;
                if (!readOnly)
                {
                    if (softKeyboard != null)
                    {
                        txt = softKeyboard.text;
                    }
                    else if (!string.IsNullOrEmpty(compositionString))
                    {
                        compositing = true;
                        if (HasSelection)
                        {
                            txt = txt[..selectionTop] + compositionString + txt[selectionEnd..];
                        }
                        else
                        {
                            txt = txt[..cursorPos] + compositionString + txt[cursorPos..];
                        }
                    }
                }
                textComponent.Text = FilterText(txt);
                textComponent.DirtyText();
            }
            UpdateCaret();
        }
        protected virtual void LateUpdate()
        {
            if (activateInputField)
            {
                activateInputField = false;
                if (!hasFocus)
                {
                    ActivateInputField();
                    return;
                }
            }
            UpdateCompositionString();
            UpdateSoftKeyboard();
            UpdateVisuals();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            activateInputField = false;
            DirtyVisuals();
            if (textComponent != null)
            {
                textComponent.OverrideAction += OnOverrideAction;
            }
        }
        protected override void OnDisable()
        {
            if (textComponent != null)
            {
                textComponent.OverrideAction -= OnOverrideAction;
            }
            DeactivateInputField();
            base.OnDisable();
        }
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            DirtyVisuals();
        }
#endif
    }
}



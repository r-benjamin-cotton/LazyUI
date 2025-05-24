using LazyUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// LazyUI�̃T���v��
/// �v���p�e�B�����J����LazyUI���瑀�삵�܂��B
/// </summary>
public class LazyUISampleTarget : MonoBehaviour
{
    /// <summary>
    /// �l�̕ύX���R���\�[���֕\��
    /// </summary>
    public bool Verbose = false;


    public enum TestEnumType
    {
        TestA,
        TestB,
        TestC,
        TestD,
    }
    private TestEnumType testEnum = TestEnumType.TestB;
    /// <summary>
    /// �񋓌^�̃f��
    /// </summary>
    public TestEnumType TestEnum
    {
        get
        {
            return testEnum;
        }
        set
        {
            if (testEnum == value)
            {
                return;
            }
            if (Verbose)
            {
                LazyDebug.Log("TestEnum:" + testEnum + " -> " + value);
            }
            testEnum = value;
        }
    }
    private bool testBool = true;
    public bool TestBool
    {
        get
        {
            return testBool;
        }
        set
        {
            if (testBool == value)
            {
                return;
            }
            if (Verbose)
            {
                LazyDebug.Log("TestBool:" + testBool + " -> " + value);
            }
            testBool = value;
        }
    }
    private int testInt = 64;
    public int TestInt
    {
        get
        {
            return testInt;
        }
        set
        {
            if (testInt == value)
            {
                return;
            }
            if (Verbose)
            {
                LazyDebug.Log("TestInt:" + testInt + " -> " + value);
            }
            testInt = value;
        }
    }
    private Range<int> testIntRange = new Range<int>(16, 112);
    public Range<int> TestIntRange
    {
        get { return testIntRange; }
        set
        {
            if (testIntRange == value)
            {
                return;
            }
            if (Verbose)
            {
                LazyDebug.Log("TestIntRange:" + testIntRange + " -> " + value);
            }
            testIntRange = value;
        }
    }

    public Range<int> TestIntRangeRange => new Range<int>(0, 127);

    private float testFloat = 1.23f;
    /// <summary>
    /// float�^�̃f��
    /// float��int���ł�"�v���p�e�B��+Range"��range�^��Ԃ��Ɠ��I�ɒl�͈̔͂��w�肷�邱�Ƃ��ł���B
    /// </summary>
    public float TestFloat
    {
        get
        {
            return testFloat;
        }
        set
        {
            if (testFloat == value)
            {
                return;
            }
            if (Verbose)
            {
                LazyDebug.Log("TestFloat:" + testFloat + " -> " + value);
            }
            testFloat = value;
        }
    }
    private Range<float> testFloatRange = new Range<float>(-2, +2);
    /// <summary>
    /// TestFloat�͈̔͂𓮓I�ɕύX����f��
    /// </summary>
    public Range<float> TestFloatRange
    {
        get
        {
            return testFloatRange;
        }
        set
        {
            if (testFloatRange == value)
            {
                return;
            }
            if (Verbose)
            {
                LazyDebug.Log("TestFloatRange:" + testFloatRange + " -> " + value);
            }
            testFloatRange = value;
        }
    }
#if true
    /// <summary>
    /// TestFloatRange�͈͎̔w��A�����̏ꍇ��UI�Ŏw�肵��minValue/maxValue���g����
    /// </summary>
    public Range<float> TestFloatRangeRange
    {
        get
        {
            return new Range<float>(-3, +3);
        }
    }
#endif

    private string testString = "test";
    /// <summary>
    /// ������^
    /// </summary>
    public string TestString
    {
        get
        {
            return testString;
        }
        set
        {
            testString = value;
        }
    }

    public void OnClick()
    {
        //if (Verbose)
        {
            LazyDebug.Log("OnClick");
        }
    }
    public void Shortcut()
    {
        //if (Verbose)
        {
            LazyDebug.Log("Shortcut");
        }
    }
}

using LazyUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// LazyUIのサンプル
/// プロパティを公開してLazyUIから操作します。
/// </summary>
public class LazyUISampleTarget : MonoBehaviour
{
    /// <summary>
    /// 値の変更をコンソールへ表示
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
    /// 列挙型のデモ
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
    private LazyRange<int> testIntRange = new LazyRange<int>(16, 112);
    public LazyRange<int> TestIntRange
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

    public LazyRange<int> TestIntRangeRange => new LazyRange<int>(0, 127);

    private float testFloat = 1.23f;
    /// <summary>
    /// float型のデモ
    /// floatやint等では"プロパティ名+Range"でrange型を返すと動的に値の範囲を指定することができる。
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
    private LazyRange<float> testFloatRange = new LazyRange<float>(-2, +2);
    /// <summary>
    /// TestFloatの範囲を動的に変更するデモ
    /// </summary>
    public LazyRange<float> TestFloatRange
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
    /// TestFloatRangeの範囲指定、無効の場合はUIで指定したminValue/maxValueが使われる
    /// </summary>
    public LazyRange<float> TestFloatRangeRange
    {
        get
        {
            return new LazyRange<float>(-3, +3);
        }
    }
#endif

    private string testString = "test";
    /// <summary>
    /// 文字列型
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
    public void OnRelease()
    {
        //if (Verbose)
        {
            LazyDebug.Log("OnRelease");
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

using System;

namespace LazyUI
{
    [Serializable]
    public struct LazyMargin
    {
        public float left;
        public float right;
        public float top;
        public float bottom;
        public LazyMargin(float left, float right, float top, float bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }
        public readonly bool Equals(LazyMargin other)
        {
            return (left == other.left) && (right == other.right) && (top == other.top) && (bottom == other.bottom);
        }
        public readonly override bool Equals(object obj)
        {
            if (obj is not LazyMargin other)
            {
                return false;
            }
            return Equals(other);
        }
        public readonly override int GetHashCode()
        {
            return HashCode.Combine(left, right, top, bottom);
        }
        public static bool operator ==(LazyMargin v0, LazyMargin v1)
        {
            return v0.Equals(v1);
        }
        public static bool operator !=(LazyMargin v0, LazyMargin v1)
        {
            return !v0.Equals(v1);
        }
        public readonly override string ToString()
        {
            return $"<{left},{right},{top},{bottom}>";
        }
    }
}

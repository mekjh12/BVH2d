using OpenGL;
using System;

namespace BVH2d
{
    public class AABB
    {
        Vertex2f _lower;
        Vertex2f _upper;

        public Vertex2f Center => (_lower + _upper) * 0.5f;

        public Vertex2f LowerBound
        {
            get => _lower;
            set => _lower = value;
        }

        public Vertex2f UpperBound
        {
            get => _upper;
            set => _upper = value;
        }

        public float Area
        {
            get
            {
                Vertex2f delta = _upper - _lower;
                return 2.0f * (Math.Abs(delta.x) + Math.Abs(delta.y));
            }
        }

        public AABB(float x1, float y1, float x2, float y2)
        {
            _lower = new Vertex2f(x1, y1);
            _upper = new Vertex2f(x2, y2);
        }

        public AABB(Vertex2f lower, Vertex2f upper)
        {
            _lower = lower;
            _upper = upper;
        }

        public static AABB operator +(AABB a, AABB b)
        {
            return new AABB(min(a.LowerBound, b.LowerBound), max(a.UpperBound, b.UpperBound));
            Vertex2f min(Vertex2f v1, Vertex2f v2) => new Vertex2f(Math.Min(v1.x, v2.x), Math.Min(v1.y, v2.y));
            Vertex2f max(Vertex2f v1, Vertex2f v2) => new Vertex2f(Math.Max(v1.x, v2.x), Math.Max(v1.y, v2.y));
        }
    }
}

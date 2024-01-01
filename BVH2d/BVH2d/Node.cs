using OpenGL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace BVH2d
{
    public class Node
    {
        public static int GUID = -1;
        public static Font Font9String = new Font("consalas", 9.0f);
        public static Stack<int> GuidStack = new Stack<int>(); // 제거한 노드의 guid를 수거하여 다음 삽입때 재사용한다.

        Node _parent;
        Node _child1;
        Node _child2;

        uint _depth;
        AABB _box;
        AABB _enhanceBox;
        float _inheritedCost;
        int _guid;
        Brush _color;

        public uint Depth
        {
            get => _depth; 
            set => _depth = value;
        }

        public int Guid => _guid;

        public string Value => $"{_box.LowerBound.ToString()} {_box.UpperBound.ToString()}";

        public int Child1_GUID => (_child1 == null) ? -1 : _child1.Guid;

        public int Child2_GUID => (_child2 == null) ? -1 : _child2.Guid;

        /// <summary>
        /// 루트노드에서 현재 노드까지의 ∑{△SA(P_i)}이다.
        /// </summary>
        public float InheritedCost
        {
            get => _inheritedCost;
            set => _inheritedCost = value;
        }

        public AABB Box => _box;

        public bool IsRoot => (_parent == null);

        public bool IsLeaf => (_child1 == null && _child2 == null);

        public bool HasChild => (_child1 != null || _child2 != null);


        public bool HasChild1 => (_child1 != null);

        public bool HasChild2 => (_child2 != null);

        public Node Child1
        {
            get => _child1;
            set => _child1 = value;
        }

        public Node Child2
        {
            get => _child2;
            set => _child2 = value;
        }

        public Node Parent
        {
            get => _parent;
            set => _parent = value;
        }

        public Node GrandParent
        {
            get
            {
                if (_parent == null) return null;
                if (_parent.Parent == null) return null;
                return _parent.Parent;
            }
        }

        public Node Brother
        {
            get
            {
                if (_parent == null) return null;
                return (_parent.Child1 == this) ? _parent.Child2 : _parent.Child1;
            }
        }

        public Node Uncle
        {
            get
            {
                if (_parent == null) return null;
                if (_parent.Parent == null) return null;
                return _parent.Brother;
            }
        }


        public Node(AABB box)
        {
            if (GuidStack.Count > 0) 
            {
                _guid = GuidStack.Pop();
            }
            else
            {
                Node.GUID++;
                _guid = Node.GUID;
            }
            _depth = 0;
            _color = RndBrush.GetRandomBrush;
            _box = box;
            box.Node = this;

            if (box.UseEnhanceBox)
            {
                _enhanceBox = new AABB(useEnhanceBox: true);
                RefitEnhanceBox();
            }
        }

        /// <summary>
        /// AABB Box를 감싸고 있는 EnhanceBox에 밖으로 이동하였는지 유무를 반환한다.
        /// </summary>
        /// <param name="enhanceBox"></param>
        /// <returns></returns>
        public bool IsOutBoundToEnhanceBox()
        {
            if (_box.LowerBound.x < _enhanceBox.LowerBound.x) return true;
            if (_box.UpperBound.x > _enhanceBox.UpperBound.x) return true;
            if (_box.LowerBound.y < _enhanceBox.LowerBound.y) return true;
            if (_box.UpperBound.y > _enhanceBox.UpperBound.y) return true;
            return false;
        }

        /// <summary>
        /// 같은 중심에서 AABB Box의 크기보다 margin만큼 더 큰 EnhanceBox를 조정해준다.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="margin"></param>
        public void RefitEnhanceBox(float margin = 5.0f)
        {
            Vertex2f marginVector = new Vertex2f(margin, margin);
            _enhanceBox.LowerBound = _box.LowerBound - marginVector;
            _enhanceBox.UpperBound = _box.UpperBound + marginVector;
        }

        /// <summary>
        /// 노드의 box와 insertBox의 AABB박스의 LowerBound와 UpperBound를 조정한다. 
        /// </summary>
        /// <param name="insertBox"></param>
        public void Refit(AABB insertBox)
        {
            Vertex2f lower = Vertex2f.Min(new Vertex2f[] { _box.LowerBound, insertBox.LowerBound });
            Vertex2f upper = Vertex2f.Max(new Vertex2f[] { _box.UpperBound, insertBox.UpperBound });
            _box.LowerBound = lower;
            _box.UpperBound = upper;
        }

        /// <summary>
        /// 제거할 자식의 자리(왼쪽 또는 오른쪽)에 새롭게 붙일 자식을 그 자리에 붙인다.
        /// </summary>
        /// <param name="removeChild"></param>
        /// <param name="attachNode"></param>
        /// <returns></returns>
        public bool ReplaceChild(Node removeChild, Node attachNode)
        {
            if (_child1 == removeChild)
            {
                _child1 = attachNode;
                attachNode.Parent = this;
                return true;
            }

            if (_child2 == removeChild)
            {
                _child2 = attachNode;
                attachNode.Parent = this;
                return true;
            }
            return false;
        }

        [Obsolete("2d graphics를 위한 기능으로 3d확장에서는 필요가 없습니다.")]
        public void Draw(Graphics g, float rootLineWidth, float stepLineWidth = 0.0f)
        {
            float width = _box.Size.x;
            float height = _box.Size.y;

            if (IsLeaf)
            {
                g.FillRectangle(_color, _box.LowerBound.x, _box.LowerBound.y, width, height);
                g.DrawString(_guid.ToString(), Node.Font9String, _color, _box.LowerBound.x, _box.LowerBound.y - 13);


                if (_box.UseEnhanceBox)
                {
                    g.DrawRectangle(new Pen(_color, rootLineWidth),
                        _enhanceBox.LowerBound.x, _enhanceBox.LowerBound.y, _enhanceBox.Size.x, _enhanceBox.Size.y);
                }

            }
            else
            {
                _child1?.Draw(g, rootLineWidth + stepLineWidth, stepLineWidth);
                _child2?.Draw(g, rootLineWidth + stepLineWidth, stepLineWidth);
                g.DrawRectangle(new Pen(_color, rootLineWidth), _box.LowerBound.x, _box.LowerBound.y, width, height);
                g.DrawString($"({_guid})", Node.Font9String, _color, _box.LowerBound.x, _box.UpperBound.y - 13);
            }

        }

        [Obsolete("2d graphics를 위한 기능으로 3d확장에서는 필요가 없습니다.")]
        public void DrawTree(Graphics g, Pen pen, float parentPosX, float parentPosY, bool isLeftChild, int maxDepth)
        {
            float stringWidth = maxDepth==0 ? maxDepth : 100 / maxDepth;
            stringWidth = Math.Max(5, stringWidth);
            int inverseDepth = (int)(maxDepth - _depth);

            float width = 10.0f * (float)(Math.Pow(2, inverseDepth));
            width = Math.Min(64, width);

            float mePosX = parentPosX + (isLeftChild ? -width : width);
            if (IsRoot) mePosX = 800;

            float depthVariable = 23 * 0.01f * (100-maxDepth);
            float mePosY = parentPosY + depthVariable;
            if (IsLeaf)
            {
                if (!IsRoot) g.DrawLine(pen, parentPosX, parentPosY + 15, mePosX, mePosY + (isLeftChild ? 0 : 20));
                g.DrawString($"{_guid}", Font9String, _color, mePosX, mePosY + (isLeftChild ? 0 : 20));
            }
            else
            {
                if (!IsRoot) g.DrawLine(pen, parentPosX, parentPosY + 15, mePosX, mePosY);
                g.DrawString($"{_guid}", Font9String, Brushes.Yellow, mePosX, mePosY);
            }
            if (HasChild1) this.Child1.DrawTree(g, pen, mePosX, parentPosY + depthVariable, true, maxDepth);
            if (HasChild2) this.Child2.DrawTree(g, pen, mePosX, parentPosY + depthVariable, false, maxDepth);
        }

        public void Print(string txt = "")
        {
            if (IsLeaf)
            {
                Console.WriteLine($"{txt}[{Guid}]");
            }
            else
            {
                _child1?.Print($"{txt}({Guid})↗");
                _child2?.Print($"{txt}({Guid})↘");
            }
        }
    }
}

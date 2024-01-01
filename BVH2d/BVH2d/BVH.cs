using OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;

namespace BVH2d
{
    /// <summary>
    /// Bounding Volume Hierarchy 트리 구조를 이용하여 Node를 구성한다.
    /// </summary>
    public class BVH
    {
        public enum INSERT_ALGORITHM_METHOD
        {
            GLOBAL_SEARCH,
            BRANCH_AND_BOUND
        }

        private const string STRING_NODE_ISNOT_LEAF = "현재 선택한 노드 ({node.Guid})는 leaf가 아닙니다.";

        Node _root;
        uint _countLeaf;
        Node _recentNode;

        public Node RecentNode => _recentNode;

        public uint CountLeaf => (_countLeaf + 1);

        public uint CountTotalNode => 2 * CountLeaf - 1;

        public Node Root => _root;

        public bool IsEmpty => (_root == null);

        /// <summary>
        /// * 노드의 전체 갯수를 반환한다.<br/>
        /// * N이 leaf의 갯수이면, 노드의 전체수는 2N-1이다. <br/>
        /// </summary>
        public uint NodeCount
        {
            get
            {
                if (IsEmpty) return 0;
                uint num = 0;
                Queue<Node> queue = new Queue<Node>();
                queue.Enqueue(_root);
                while (queue.Count > 0)
                {
                    Node tourNode = queue.Dequeue();
                    num++;
                    if (tourNode.Child1 != null) queue.Enqueue(tourNode.Child1);
                    if (tourNode.Child2 != null) queue.Enqueue(tourNode.Child2);
                }
                return num;
            }
        }

        /// <summary>
        /// 트리의 최대 깊이를 반환한다.
        /// </summary>
        public int Depth
        {
            get
            {
                if (IsEmpty) return 0;
                Queue<Node> queue = new Queue<Node>();
                queue.Enqueue(_root);
                int depth = 0;
                while (queue.Count > 0)
                {
                    Node tourNode = queue.Dequeue();
                    tourNode.Depth = (tourNode.IsRoot)? 0: tourNode.Parent.Depth + 1;
                    depth = (int)Math.Max(depth, tourNode.Depth);
                    if (tourNode.Child1 != null) queue.Enqueue(tourNode.Child1);
                    if (tourNode.Child2 != null) queue.Enqueue(tourNode.Child2);
                }
                return depth;
            }
        }

        public BVH() { }

        /// <summary>
        /// * 삽입할 자리에 드는 전체 비용을 반환한다.<br/>
        /// * 비용 = sum{부모의 변화량} + cos(T ∪ L)<br/>
        /// * L = 삽입하는 노드<br/>
        /// * T = 삽입할 자리의 노드<br/>
        /// </summary>
        /// <param name="targetNode"></param>
        /// <param name="insertBox"></param>
        /// <returns></returns>
        private float Cost(Node targetNode, Node insertBox)
        {
            float cost = (targetNode.Box + insertBox.Box).Area;

            if (targetNode.Parent != null)
            {
                Node tourNode = targetNode.Parent;
                while (tourNode != null) // targetNode로부터 부모를 따라 루트까지 이동한다. target-->...-->root까지 모두 실행되는 루프이다.
                {
                    float deltaSA = (tourNode.Box + insertBox.Box).Area - tourNode.Box.Area;
                    cost += deltaSA;

                    tourNode = tourNode.Parent;
                }
            }

            return cost;
        }

        /// <summary>
        /// * Best Global Search 알고리즘을 사용하여 최적의 삽입할 자리의 노드를 찾아 반환한다.<br/>
        /// * Branch and Bound 알고리즘을 적용하기 전에 사용하지 않는 알고리즘으로 속도가 느리다.<br/>
        /// * 총 N개의 leaf에서 internal node를 포함한 검색 노드의 갯수는 2N-1이다.<br/>
        /// </summary>
        /// <param name="insertNode"></param>
        /// <returns></returns>
        private Node PickBestGlobalSearch(Node insertNode)
        {
            Node bestSibling = _root;
            float cost = float.MaxValue;

            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(_root);
            while (queue.Count > 0)
            {
                Node tourNode = queue.Dequeue();

                float costNode = Cost(tourNode, insertNode);
                if (costNode < cost)
                {
                    cost = costNode;
                    bestSibling = tourNode;
                }

                if (tourNode.Child1 != null) queue.Enqueue(tourNode.Child1);
                if (tourNode.Child2 != null) queue.Enqueue(tourNode.Child2);
            }

            return bestSibling;
        }

        /// <summary>
        /// Branch And Bound알고리즘을 사용하여 최적의 삽입할 자리의 노드를 찾아 반환한다.
        /// </summary>
        /// <param name="insertNode"></param>
        /// <returns></returns>
        private Node PickBestBranchAndBound(Node insertNode)
        {
            Node L = insertNode;
            Queue<Node> queue = new Queue<Node>();

            // Branch And Bound를 위한 초기화
            Node bestSibling = _root;
            float bestCost = (bestSibling.Box + L.Box).Area + 1.0f;
            queue.Enqueue(_root);

            // C_low < C_best 인 경우에 하위 트리를 탐색해 나간다.
            while (queue.Count > 0)
            {
                Node currentNode = queue.Dequeue();

                float directedCost = (currentNode.Box + L.Box).Area;
                float prevInheritedCost = (currentNode.Parent == null) ? 0.0f : currentNode.Parent.InheritedCost;
                float currentDeltaSA = (directedCost - currentNode.Box.Area);
                currentNode.InheritedCost = prevInheritedCost + currentDeltaSA;
                float costLow = directedCost + prevInheritedCost;

                if (costLow < bestCost)
                {
                    bestSibling = currentNode;
                    bestCost = costLow;

                    if (currentNode.Child1 != null) queue.Enqueue(currentNode.Child1);
                    if (currentNode.Child2 != null) queue.Enqueue(currentNode.Child2);
                }
            }

            return bestSibling;
        }

        /// <summary>
        ///  두 박스의 합의 박스를 리턴한다.
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        private static Node Union(Node A, Node B)
        {
            Node C = new Node(A.Box + B.Box);
            C.Child1 = A;
            C.Child2 = B;
            A.Parent = C;
            B.Parent = C;
            return C;
        }

        /// <summary>
        /// * 두 노드의 AABB박스의 합의 비용을 반환한다.<br/>
        /// * AABB박스로 직접 생성하지 않고 float로 반환하는 이유는 노드를 링크시 엉킬지 않도록 하기 위함이다. <br/>
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        private static float UnionArea(Node A, Node B)
        {
            Node C = new Node(A.Box + B.Box);
            return C.Box.Area;
        }

        /// <summary>
        /// * SAH cost를 줄이기 위한 re-arrangeing a tree.<br/>
        /// * node를 기준으로 자식과 형제를 교환가능한지 여부를 판단한다.<br/>
        /// * 트리를 거꾸로 올라가면서 refit할 때, leaf를 제외한 바로 윗 부모부터 순회를 시작하기 때문이다.<br/>
        /// </summary>
        /// <param name="node"></param>
        private void RotateNode(Node node)
        {
            // 부모 노드가 없으면 RotateNode가 없다.
            if (node.Parent == null) return;

            // 자식 노드가 없으면 RotateNode가 없다.
            if (node.IsLeaf) return;

            // 부모가 있으면 brother가 반드시 존재한다.
            Node brother = node.Brother;

            // 검색할 4개의 노드를 선정하여 리스트에 담는다.
            List<Node> nodes = new List<Node>();
            nodes.Add(node.Child1);
            nodes.Add(node.Child2);
            if (brother.HasChild1) nodes.Add(brother.Child1);
            if (brother.HasChild2) nodes.Add(brother.Child2);

            // 검색한 4개의 노드를 순회하면서 RotateNode가 필요하면 교체한다.
            foreach (Node me in nodes)
            {
                Node parent = me.Parent;
                Node grandParent = me.GrandParent;
                Node anotherChild = me.Brother;
                Node uncle = parent.Brother;

                float candinateCost = BVH.UnionArea(anotherChild, uncle);
                if (candinateCost < node.Box.Area) // Rotate이 필요하다.
                {
                    // Rotate을 위한 링크를 수정한다.
                    grandParent.ReplaceChild(uncle, me);
                    uncle.Parent = parent;

                    if (parent.Child1 == anotherChild)
                    {
                        parent.Child2 = uncle;
                    }
                    else
                    {
                        parent.Child1 = uncle;
                    }
                }
            }
        }

        public Node ReInsert(Node node, INSERT_ALGORITHM_METHOD mode = INSERT_ALGORITHM_METHOD.BRANCH_AND_BOUND)
        {
            AABB box = node.Box;
            RemoveLeaf(node);
            return InsertLeaf(box, mode);
        }

        /// <summary>
        /// * 삽입 알고리즘에 따라서AABB박스를 트리에 삽입한다.<br/>
        /// * 기본 알고리즘은 PickBestBranchAndBound이다.<br/>
        /// </summary>
        /// <param name="box"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public Node InsertLeaf(AABB box, INSERT_ALGORITHM_METHOD mode = INSERT_ALGORITHM_METHOD.BRANCH_AND_BOUND)
        {
            box.UseEnhanceBox = true;
            Node insertNode = new Node(box);

            // stage 0: if tree is null, then inserted node is root.
            if (IsEmpty)
            {
                _root = insertNode;
                return insertNode;
            }

            // stage 1: find the best sibling for the new leaf
            Node bestSibling;
            if (mode == INSERT_ALGORITHM_METHOD.GLOBAL_SEARCH)
            {
                bestSibling = this.PickBestGlobalSearch(insertNode);
            }
            else
            {
                bestSibling = this.PickBestBranchAndBound(insertNode);
            }

            // stage 2: create a new parent
            Node newParent;
            if (bestSibling.IsRoot) // bestSilbling이 루트이다.
            {
                // [변경 전] root-->b
                // [변경 후] root-->n--(b+i)
                newParent = BVH.Union(bestSibling, insertNode);
                _root = newParent;
            }
            else // bestSilbling의 이전부모를 조부모로 만들고 새로운 부모로 이어븥인다.
            {
                // [변경 전] o-->b
                // [변경 후] o-->n--(b+i)
                Node oldParent = bestSibling.Parent;
                newParent = BVH.Union(bestSibling, insertNode);
                oldParent.ReplaceChild(bestSibling, newParent);
            }

            // stage 3: walk back up the tree refitting aabb and applying rotations
            Node tourNode = newParent;
            while (tourNode != null) // newParent부터 root까지 역상승한다.
            {
                // targetNode부터 루트까지 모두 상위로 순회한다.
                tourNode.Refit(insertNode.Box);
                RotateNode(tourNode);
                tourNode = tourNode.Parent;
            };

            // 정상적으로 삽입하면 leaf의 갯수를 증가시킨다.
            if (insertNode != null) _countLeaf++;
            _recentNode = insertNode;

            return insertNode;
        }

        /// <summary>
        /// * 잎노드를 제거한다.<br/>
        /// * 내부노드를 제거할 수 없다.<br/>
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public bool RemoveLeaf(Node node)
        {
            if (node == null) return false;
            if (!node.IsLeaf)
            {
                Console.WriteLine("{node.Guid} " + STRING_NODE_ISNOT_LEAF);
                return false;
            }

            if (node.IsRoot) // root이면 
            {
                if (node.HasChild) //  leaf가 아니므로
                {
                    Console.WriteLine("{node.Guid} " + STRING_NODE_ISNOT_LEAF);
                    return false;
                }
                else // 삭제한다.
                {
                    Node.GuidStack.Push(_root.Guid);
                    _root = null;
                }
            }
            else // root 아니면
            {
                Node parent = node.Parent;
                Node restNode = (parent.Child1 == node) ? parent.Child2 : parent.Child1;

                Node.GuidStack.Push(parent.Guid);
                Node.GuidStack.Push(node.Guid);

                if (parent.IsRoot)
                {
                    _root = restNode;
                    restNode.Parent = null;
                }
                else
                {
                    Node grandParent = parent.Parent;
                    if (grandParent.Child1 == parent)
                    {
                        grandParent.Child1 = restNode;
                    }

                    if (grandParent.Child2 == parent)
                    {
                        grandParent.Child2 = restNode;
                    }
                    restNode.Parent = grandParent;
                }
            }

            _countLeaf = Math.Max(_countLeaf--, 0);
            return true;
        }

        /// <summary>
        /// 노드의 guid를 이용하여 노드를 찾는다.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Node FindNode(uint guid)
        {
            // 검색알고리즘은 트리의 모든 노드를 탐색하여 찾는 노드를 반환한다.
            if (IsEmpty) return null;
            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(_root);
            while (queue.Count > 0)
            {
                Node tourNode = queue.Dequeue();

                if (tourNode.Guid == guid) return tourNode;

                if (tourNode.Child1 != null) queue.Enqueue(tourNode.Child1);
                if (tourNode.Child2 != null) queue.Enqueue(tourNode.Child2);
            }
            return null;
        }

        public void Optimize(Node node)
        {
            if (node == null) return;
            Console.WriteLine("* Optimize start!");
            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(node);
            while (queue.Count > 0)
            {
                Node tourNode = queue.Dequeue();

                RotateNode(node);
                if (tourNode.Child1 != null) queue.Enqueue(tourNode.Child1);
                if (tourNode.Child2 != null) queue.Enqueue(tourNode.Child2);
            }
            Console.WriteLine("* Optimize finished!");
        }

        /// <summary>
        /// * 트리의 전체 노드를 삭제한다.<br/>
        /// * 링크를 해제한 것이므로 GC실행시 메모리를 정리한다.<br/>
        /// </summary>
        public void Clear()
        {
            if (IsEmpty) return;
            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(_root);
            while (queue.Count > 0)
            {
                Node tourNode = queue.Dequeue();
                if (tourNode.Child1 != null) queue.Enqueue(tourNode.Child1);
                if (tourNode.Child2 != null) queue.Enqueue(tourNode.Child2);
                tourNode = null;
            }
            _root = null;
            _countLeaf = 0;
            Node.GUID = -1;
        }
    }
}
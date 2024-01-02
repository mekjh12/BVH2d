using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BVH2d
{
    public partial class Form1 : Form
    {
        BVH bvh;
        BVH bvh2;
        bool isDraged = false;
        Point start;
        Point end;
        uint sorted_insert_px = 10;

        public Form1()
        {
            InitializeComponent();
        }

        private void SetUpControl(int width, int height)
        {
            int titleHeight = this.ClientRectangle.Height - this.Height;
            int padding = 3;
            this.StartPosition = FormStartPosition.Manual;
            this.Left = 0;
            this.Top = 0;
            this.Width = width;
            this.Height = height;
            this.pictureBox1.Visible = false;
            this.pictureBox1.BackColor = Color.Transparent;
            int topMargin = titleHeight;
            int leftMargin = this.button1.Right + padding;
            int widthDivide2 = (width - leftMargin) / 2;
            int pcbHeight = height - topMargin - padding - 0;
            this.pcb1.Bounds = new Rectangle(leftMargin, topMargin, widthDivide2, pcbHeight);
            this.pcb2.Bounds = new Rectangle(leftMargin + widthDivide2 + padding, topMargin, widthDivide2, pcbHeight);
            this.pcb3.Bounds = new Rectangle(pcb1.Left, pcb1.Bottom + padding, 2 * widthDivide2, 0);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetUpControl(Screen.AllScreens[0].WorkingArea.Width, Screen.AllScreens[0].WorkingArea.Height);
            bvh = new BVH();
            bvh2 = new BVH();
        }

        private void DrawTree()
        {
            int depth = bvh.Depth;
            Graphics g = Graphics.FromHwnd(this.pcb2.Handle);
            g.Clear(Color.Black);
            //bvh.Root?.DrawTree(g, new Pen(Color.Red, 2.0f), this.Width, -10 , true, depth);
            bvh.Root?.DrawTree2(g, new Pen(Color.Red, 2.0f), this.pcb2.Width/2, this.pcb2.Height / 2, 90, 180);
            g.DrawString("depth=" + depth, Node.Font9String, Brushes.Yellow, new PointF(10, 10));

            string rep = $"nodeNum={bvh.CountTotalNode}, leafNum={bvh.CountLeaf}";
            g.DrawString(rep, Node.Font9String, Brushes.Yellow, new PointF(70, 10));
        }

        private void Draw(IntPtr hwnd, BVH bvh, string title)
        {
            Graphics g = Graphics.FromHwnd(hwnd);
            g.Clear(Color.Black);
            bvh.Root?.Draw(g, 1);
            g.FillRectangle(Brushes.Black, 10, 10, 300, 20);
            g.DrawString(title, Node.Font9String, Brushes.Yellow, new PointF(10, 10));
           
        }

        private async void Insert(IntPtr hwnd, BVH bvh, BVH.INSERT_ALGORITHM_METHOD mode, int num, string title)
        {
            await Task.Run(() =>
            {
                Random rnd = new Random();
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Reset();
                stopwatch.Start();
                for (int i = 0; i < num; i++)
                {
                    float x = rnd.Next(0, this.pcb1.Width);
                    float y = rnd.Next(0, this.pcb1.Height);
                    float w = rnd.Next(10, 50);
                    float h = rnd.Next(10, 50);
                    bvh.InsertLeaf(new AABB(x, y, x + w, y + h), mode);
                }
                stopwatch.Stop();
                Draw(hwnd, bvh, title + $",n={bvh.NodeCount}" + ",t=" + stopwatch.ElapsedMilliseconds);
            });
        }

        private void pcb1_MouseDown(object sender, MouseEventArgs e)
        {
            isDraged = true;
            start = new Point(e.X, e.Y);
            pictureBox1.Visible = true;
        }

        private void pcb1_MouseMove(object sender, MouseEventArgs e)
        {
            Point mid = e.Location;
            if (mid.X < start.X || mid.Y < start.Y)
            {
                isDraged = false;
            }

            if (isDraged)
            {
                Draw(pcb1.Handle, bvh, "BRANCH_AND_BOUND");
                Draw(pcb2.Handle, bvh2, "GLOBAL_SEARCH");
                pictureBox1.Bounds = new Rectangle(pcb1.Left + start.X, pcb1.Top + start.Y, mid.X - start.X, mid.Y - start.Y);
            }
        }

        private void pcb1_MouseUp(object sender, MouseEventArgs e)
        {
            isDraged = false;
            end = new Point(e.X, e.Y);
            pictureBox1.Visible = false;

            end.X = (end.X - start.X < 5) ? start.X + 20 : end.X;
            end.Y = (end.Y - start.Y < 5) ? start.Y + 20 : end.Y;

            bvh.InsertLeaf(new AABB(start.X, start.Y, end.X, end.Y), BVH.INSERT_ALGORITHM_METHOD.BRANCH_AND_BOUND);
            //bvh2.InsertLeaf(new AABB(start.X, start.Y, end.X, end.Y), BVH.INSERT_ALGORITHM_METHOD.GLOBAL_SEARCH);

            Timer t = new Timer()
            {
                Interval = 100,
                Enabled = true,
            };
            t.Tick += (o, ee) =>
            {
                Draw(pcb1.Handle, bvh, "BRANCH_AND_BOUND");
                DrawTree();
                //Draw(pcb2.Handle, bvh2, "GLOBAL_SEARCH");
                ((Timer)o).Stop();
            };
            t.Start();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Draw(pcb1.Handle, bvh, "BRANCH_AND_BOUND");
            Draw(pcb2.Handle, bvh2, "GLOBAL_SEARCH");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bvh.Clear();
            bvh2.Clear();
            Draw(pcb1.Handle, bvh, "BRANCH_AND_BOUND");
            Draw(pcb2.Handle, bvh2, "GLOBAL_SEARCH");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            bvh.Root?.Print();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            bvh.RemoveLeaf(bvh.FindNode(uint.Parse(this.textBox1.Text)));
            Draw(pcb1.Handle, bvh, "BRANCH_AND_BOUND");
            Draw(pcb2.Handle, bvh2, "GLOBAL_SEARCH");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            bvh.InsertLeaf(new AABB(200, 200, 230, 240), BVH.INSERT_ALGORITHM_METHOD.BRANCH_AND_BOUND);
            bvh.InsertLeaf(new AABB(500, 500, 570, 540), BVH.INSERT_ALGORITHM_METHOD.BRANCH_AND_BOUND);
            bvh2.InsertLeaf(new AABB(200, 200, 230, 240), BVH.INSERT_ALGORITHM_METHOD.GLOBAL_SEARCH);
            bvh2.InsertLeaf(new AABB(500, 500, 570, 540), BVH.INSERT_ALGORITHM_METHOD.GLOBAL_SEARCH);
            Draw(pcb1.Handle, bvh, "BRANCH_AND_BOUND");
            Draw(pcb2.Handle, bvh2, "GLOBAL_SEARCH");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            bvh.InsertLeaf(new AABB(sorted_insert_px, 200, sorted_insert_px + 20, 230), BVH.INSERT_ALGORITHM_METHOD.BRANCH_AND_BOUND);
            //bvh2.InsertLeaf(new AABB(px, 200, px + 20, 230), BVH.INSERT_ALGORITHM_METHOD.GLOBAL_SEARCH);
            Draw(pcb1.Handle, bvh, "BRANCH_AND_BOUND");
            Draw(pcb2.Handle, bvh2, "GLOBAL_SEARCH");
            sorted_insert_px += 25;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Insert(pcb1.Handle, bvh, BVH.INSERT_ALGORITHM_METHOD.BRANCH_AND_BOUND, 10, "BRANCH_AND_BOUND");
            Insert(pcb2.Handle, bvh2, BVH.INSERT_ALGORITHM_METHOD.GLOBAL_SEARCH, 10, "GLOBAL_SEARCH");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Insert(pcb1.Handle, bvh, BVH.INSERT_ALGORITHM_METHOD.BRANCH_AND_BOUND, 100, "BRANCH_AND_BOUND");
            Insert(pcb2.Handle, bvh2, BVH.INSERT_ALGORITHM_METHOD.GLOBAL_SEARCH, 100, "GLOBAL_SEARCH");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Insert(pcb1.Handle, bvh, BVH.INSERT_ALGORITHM_METHOD.BRANCH_AND_BOUND, 1000, "BRANCH_AND_BOUND");
            Insert(pcb2.Handle, bvh2, BVH.INSERT_ALGORITHM_METHOD.GLOBAL_SEARCH, 1000, "GLOBAL_SEARCH");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Insert(pcb1.Handle, bvh, BVH.INSERT_ALGORITHM_METHOD.BRANCH_AND_BOUND, 10000, "BRANCH_AND_BOUND");
            Insert(pcb2.Handle, bvh2, BVH.INSERT_ALGORITHM_METHOD.GLOBAL_SEARCH, 10000, "GLOBAL_SEARCH");
        }

        private void button12_Click(object sender, EventArgs e)
        {
            Insert(pcb1.Handle, bvh, BVH.INSERT_ALGORITHM_METHOD.BRANCH_AND_BOUND, 1, "BRANCH_AND_BOUND");
            Timer t = new Timer()
            {
                Interval = 100,
                Enabled = true,
            };
            t.Tick += (o, ee) =>
            {
                DrawTree();
                //Draw(pcb2.Handle, bvh2, "GLOBAL_SEARCH");
                ((Timer)o).Stop();
            };
            t.Start();
           
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            DrawTree();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            bvh.Optimize(bvh.Root);
        }

        private void pcb3_Click(object sender, EventArgs e)
        {
            DrawTree();
        }

        private void button14_Click(object sender, EventArgs e)
        {
           
        }

        private void button15_Click(object sender, EventArgs e)
        {
            AABB box = bvh.RecentNode.Box;
            box.LowerBound += new OpenGL.Vertex2f(1, 0);
            box.UpperBound += new OpenGL.Vertex2f(1, 0);
            if (bvh.RecentNode.IsOutBoundToEnhanceBox())
            {
                Node newNode = bvh.ReInsert(bvh.RecentNode);
                newNode.RefitEnhanceBox();
                DrawTree();
            }
            Draw(pcb1.Handle, bvh, "BRANCH_AND_BOUND");
        }

        private void button14_MouseDown(object sender, MouseEventArgs e)
        {
            AABB box = bvh.RecentNode.Box;
            box.LowerBound -= new OpenGL.Vertex2f(1, 0);
            box.UpperBound -= new OpenGL.Vertex2f(1, 0);
            if (bvh.RecentNode.IsOutBoundToEnhanceBox())
            {
                Node newNode = bvh.ReInsert(bvh.RecentNode);
                newNode.RefitEnhanceBox();
                DrawTree();
            }
            Draw(pcb1.Handle, bvh, "BRANCH_AND_BOUND");
        }
    }
}

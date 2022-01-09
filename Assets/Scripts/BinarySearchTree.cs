using System;

public interface IBinarySearchTree<TNode> where TNode : class, IComparable
{
    /// <summary>
    /// Add a node to the tree
    /// </summary>
    /// <param name="node">The node to add</param>
    void Add(TNode node);

    /// <summary>
    /// Delete a node from the tree
    /// </summary>
    /// <param name="node">The node to delete</param>
    void Delete(TNode node);

    /// <summary>
    /// Find the minimum node in the tree
    /// </summary>
    /// <returns>The minimum node</returns>
    TNode FindMin();
}

/// <summary>
/// Red-Black Tree implementation of the BinarySearchTree interface
/// </summary>
/// <remarks>
/// Implemented using the following structure:
/// https://algorithmtutor.com/Data-Structures/Tree/Red-Black-Trees/
/// </remarks>
public class RedBlackTree<TNode> : IBinarySearchTree<TNode> where TNode : class, IComparable
{
    private enum Color
    {
        Red,
        Black
    }

    private class TreeNode
    {
        public TNode Data;
        public Color Color;
        public TreeNode Left;
        public TreeNode Right;
        public TreeNode Parent;

        public TreeNode(TNode data, Color Color)
        {
            Data = data;
            this.Color = Color;
        }
    }

    private TreeNode root;
    private TreeNode TNull;

    public RedBlackTree()
    {
        TNull = new TreeNode(null, Color.Black);
        root = TNull;
    }

    public void Add(TNode data)
    {
        var node = new TreeNode(data, Color.Red);
        node.Left = TNull; 
        node.Right = TNull;

        TreeNode y = null;
        TreeNode x = root;

        while (x != TNull)
        {
            y = x;
            if (node.Data.CompareTo(x.Data) < 0)
            {
                x = x.Left;
            }
            else
            {
                x = x.Right;
            }
        }

        // y is Parent of x
        node.Parent = y;
        if (y == null)
        {
            root = node;
        }
        else if (node.Data.CompareTo(y.Data) < 0)
        {
            y.Left = node;
        }
        else
        {
            y.Right = node;
        }

        // if new node is a root node, simply return
        if (node.Parent == null)
        {
            node.Color = Color.Black;
            return;
        }

        // if the grandParent is null, simply return
        if (node.Parent.Parent == null)
        {
            return;
        }

        // Fix the tree
        RebalanceAfterInsert(node);
    }

    public void Delete(TNode data)
    {
        if (root == TNull || root.Data.Equals(data) && root.Left == TNull && root.Right == TNull)
        {
            root = TNull;
            return;
        }

        TreeNode node = root;

        TreeNode z = TNull;
        TreeNode x, y;

        while (node != TNull)
        {
            if (node.Data.Equals(data))
            {
                z = node;
            }

            if (node.Data.CompareTo(data) < 0)
            {
                node = node.Right;
            }
            else
            {
                node = node.Left;
            }
        }

        if (z == TNull)
        {
            Console.WriteLine("The item to delete was not found");
            return;
        }

        //if (z.Left == TNull && z.Right == TNull)
        //{
        //    if (z.Parent.Left == z)
        //    {
        //        z.Parent.Left = null;
        //    }
        //    else
        //    {
        //        z.Parent.Right = null;
        //    }
        //    return;
        //}

        y = z;
        Color yOriginalColor = y.Color;
        if (z.Left == TNull)
        {
            x = z.Right;
            RedBlackTransplant(z, z.Right);
        }
        else if (z.Right == TNull)
        {
            x = z.Left;
            RedBlackTransplant(z, z.Left);
        }
        else
        {
            y = FindMinFromNode(z.Right);
            yOriginalColor = y.Color;
            x = y.Right;
            if (y.Parent == z)
            {
                x.Parent = y;
            }
            else
            {
                RedBlackTransplant(y, y.Right);
                y.Right = z.Right;
                y.Right.Parent = y;
            }

            RedBlackTransplant(z, y);
            y.Left = z.Left;
            y.Left.Parent = y;
            y.Color = z.Color;
        }

        if (yOriginalColor == Color.Black)
        {
            RebalanceAfterDelete(x);
        }
    }

    public TNode FindMin()
    {
        if (root == TNull)
        {
            return null;
        }

        return FindMinFromNode(root).Data;
    }

    private TreeNode FindMinFromNode(TreeNode node)
    {
        while (node.Left != TNull)
        {
            node = node.Left;
        }

        return node;
    }

    /// <summary>
    /// this function performs Left rotation
    /// </summary>
    /// <param name="x"></param>
    private void RotateLeft(TreeNode x)
    {
        TreeNode y = x.Right;
        x.Right = y.Left;
        if (y.Left != TNull)
        {
            y.Left.Parent = x;
        }
        y.Parent = x.Parent;
        if (x.Parent == null)
        {
            root = y;
        }
        else if (x == x.Parent.Left)
        {
            x.Parent.Left = y;
        }
        else
        {
            x.Parent.Right = y;
        }
        y.Left = x;
        x.Parent = y;
    }

    /// <summary>
    /// this function performs Right rotation
    /// </summary>
    /// <param name="x"></param>
    private void RotateRight(TreeNode x)
    {
        TreeNode y = x.Left;
        x.Left = y.Right;
        if (y.Right != TNull)
        {
            y.Right.Parent = x;
        }
        y.Parent = x.Parent;
        if (x.Parent == null)
        {
            root = y;
        }
        else if (x == x.Parent.Right)
        {
            x.Parent.Right = y;
        }
        else
        {
            x.Parent.Left = y;
        }
        y.Right = x;
        x.Parent = y;
    }

    private void RebalanceAfterInsert(TreeNode newNode)
    {
        TreeNode uncle;
        while (newNode.Parent.Color == Color.Red)
        {
            if (newNode.Parent == newNode.Parent.Parent.Right)
            {
                uncle = newNode.Parent.Parent.Left;

                // TODO check of weg kan
                if (uncle == null)
                {
                    break;
                }

                if (uncle.Color == Color.Red)
                {
                    uncle.Color = Color.Black;
                    newNode.Parent.Color = Color.Black;
                    newNode.Parent.Parent.Color = Color.Red;
                    newNode = newNode.Parent.Parent;
                }
                else
                {
                    if (newNode == newNode.Parent.Left)
                    {
                        newNode = newNode.Parent;
                        RotateRight(newNode);
                    }
                    newNode.Parent.Color = Color.Black;
                    newNode.Parent.Parent.Color = Color.Red;
                    RotateLeft(newNode.Parent.Parent);
                }
            }
            else
            {
                uncle = newNode.Parent.Parent.Right;

                // TODO item
                if (uncle == null)
                {
                    break;
                }

                if (uncle.Color == Color.Red)
                {
                    uncle.Color = Color.Black;
                    newNode.Parent.Color = Color.Black;
                    newNode.Parent.Parent.Color = Color.Red;
                    newNode = newNode.Parent.Parent;
                }
                else
                {
                    if (newNode == newNode.Parent.Right)
                    {
                        newNode = newNode.Parent;
                        RotateLeft(newNode);
                    }
                    newNode.Parent.Color = Color.Black;
                    newNode.Parent.Parent.Color = Color.Red;
                    RotateRight(newNode.Parent.Parent);
                }
            }
            if (newNode == root)
            {
                break;
            }
        }
        root.Color = Color.Black;
    }

    private void RebalanceAfterDelete(TreeNode x)
    {
        TreeNode s;
        while (x != root && x.Color == Color.Black)
        {
            if (x == x.Parent.Left)
            {
                s = x.Parent.Right;
                if (s.Color == Color.Red)
                {
                    s.Color = Color.Black;
                    x.Parent.Color = Color.Red;
                    RotateLeft(x.Parent);
                    s = x.Parent.Right;
                }

                if (s.Left.Color == Color.Black && s.Right.Color == Color.Black)
                {
                    s.Color = Color.Red;
                    x = x.Parent;
                }
                else
                {
                    if (s.Right.Color == Color.Black)
                    {
                        s.Left.Color = Color.Black;
                        s.Color = Color.Red;
                        RotateRight(s);
                        s = x.Parent.Right;
                    }

                    s.Color = x.Parent.Color;
                    x.Parent.Color = Color.Black;
                    s.Right.Color = Color.Black;
                    RotateLeft(x.Parent);
                    x = root;
                }
            }
            else
            {
                s = x.Parent.Left;
                if (s.Color == Color.Red)
                {
                    s.Color = Color.Black;
                    x.Parent.Color = Color.Red;
                    RotateRight(x.Parent);
                    s = x.Parent.Left;
                }

                if (s.Right.Color == Color.Black && s.Right.Color == Color.Black)
                {
                    s.Color = Color.Red;
                    x = x.Parent;
                }
                else
                {
                    if (s.Left.Color == Color.Black)
                    {
                        s.Right.Color = Color.Black;
                        s.Color = Color.Red;
                        RotateLeft(s);
                        s = x.Parent.Left;
                    }

                    s.Color = x.Parent.Color;
                    x.Parent.Color = Color.Black;
                    s.Left.Color = Color.Black;
                    RotateRight(x.Parent);
                    x = root;
                }
            }
        }
        x.Color = Color.Black;
    }

    private void RedBlackTransplant(TreeNode u, TreeNode v)
    {
        if (u.Parent == null)
        {
            root = v;
        }
        else if (u == u.Parent.Left)
        {
            u.Parent.Left = v;
        }
        else
        {
            u.Parent.Right = v;
        }
        v.Parent = u.Parent;
    }
}
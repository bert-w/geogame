using UnityEngine;

class Node
{
    public Node LeftNode { get; set; }
    public Node RightNode { get; set; }
    public Edge Data { get; set; }
}

class LeftEdgeBinaryTree
{
    public Node Root { get; set; }

    public float currentY;

    // Compare the new node to the given node, returning -1 if it should be the left node, +1 for right node,
    // and 0 for the same.
    private int Comparator(Edge edge, Edge newEdge) {
        if (edge.IsEqual(newEdge)) {
			return 0;
        }

		float aX = edge.CalculateXIntersection(currentY);
		float bX = newEdge.CalculateXIntersection(currentY);

        return aX < bX ? -1 : 1;
    }
 
    public bool Add(Edge value)
    {
        Node before = null, after = this.Root;
 
        while (after != null)
        {
            before = after;
            int compare = Comparator(after.Data, value);
            if (compare == -1) {
                  after = after.LeftNode; 
            } else if (compare == 1) {
                after = after.RightNode;
            } else {
                // Same value exists.
                return false;
            }
        }
 
        Node newNode = new Node();
        newNode.Data = value;
 
        if (this.Root == null)
            this.Root = newNode;
        else
        {
            int compare = Comparator(before.Data, value);
            if (compare == -1)
                before.LeftNode = newNode;
            else
                before.RightNode = newNode;
        }
 
        return true;
    }
 
    public Node Find(Edge value)
    {
        return this.Find(value, this.Root);            
    }
 
    public void Remove(Edge value)
    {
        this.Root = Remove(this.Root, value);
    }
 
    private Node Remove(Node parent, Edge key)
    {
        if (parent == null) return parent;
 
        int compare = Comparator(parent.Data, key);
        if (compare == -1) parent.LeftNode = Remove(parent.LeftNode, key); else if (compare == 1) {
            parent.RightNode = Remove(parent.RightNode, key);
        }
 
        // if value is same as parent's value, then this is the node to be deleted  
        else
        {
            // node with only one child or no child  
            if (parent.LeftNode == null)
                return parent.RightNode;
            else if (parent.RightNode == null)
                return parent.LeftNode;
 
            // node with two children: Get the inorder successor (smallest in the right subtree)  
            parent.Data = MinValue(parent.RightNode);
 
            // Delete the inorder successor  
            parent.RightNode = Remove(parent.RightNode, parent.Data);
        }
 
        return parent;
    }
 
    private Edge MinValue(Node node)
    {
        Edge minv = node.Data;
 
        while (node.LeftNode != null)
        {
            minv = node.LeftNode.Data;
            node = node.LeftNode;
        }
 
        return minv;
    }
 
    private Node Find(Edge value, Node parent)
    {
        if (parent != null)
        {
            int compare = Comparator(parent.Data, value);
            if (compare == 0) return parent;
            if (compare == -1)
                return Find(value, parent.LeftNode);
            else
                return Find(value, parent.RightNode);
        }
 
        return null;
    }
 
    public int GetTreeDepth()
    {
        return this.GetTreeDepth(this.Root);
    }
 
    private int GetTreeDepth(Node parent)
    {
        return parent == null ? 0 : Mathf.Max(GetTreeDepth(parent.LeftNode), GetTreeDepth(parent.RightNode)) + 1;
    }
 
    public void TraversePreOrder(Node parent)
    {
        if (parent != null)
        {
            Debug.Log(parent.Data + " ");
            TraversePreOrder(parent.LeftNode);
            TraversePreOrder(parent.RightNode);
        }
    }
 
    public void TraverseInOrder(Node parent)
    {
        if (parent != null)
        {
            TraverseInOrder(parent.LeftNode);
            Debug.Log(parent.Data + " ");
            TraverseInOrder(parent.RightNode);
        }
    }
 
    public void TraversePostOrder(Node parent)
    {
        if (parent != null)
        {
            TraversePostOrder(parent.LeftNode);
            TraversePostOrder(parent.RightNode);
            Debug.Log(parent.Data + " ");
        }
    }
}
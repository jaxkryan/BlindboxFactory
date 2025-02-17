using System.Collections.Generic;

public class Node<TNode> where TNode : Node<TNode> {
    public virtual TNode Parent { get; }
    public virtual IList<TNode>  Children { get; }
}
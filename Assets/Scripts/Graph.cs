﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class GraphNode<T> : Node<T>
{
    private List<int> costs;

    public GraphNode() : base() { }
    public GraphNode(T value) : base(value) { }
    public GraphNode(T value, NodeList<T> neighbors) : base(value, neighbors) { }

    new public NodeList<T> Neighbors
    {
        get
        {
            if (base.Neighbors == null)
                base.Neighbors = new NodeList<T>();

            return base.Neighbors;
        }
    }

    public List<int> Costs
    {
        get
        {
            if (costs == null)
                costs = new List<int>();

            return costs;
        }
    }
}

public class Node<T>
{
    private T data;
    private NodeList<T> neighbors = null;

    public Node() { }
    public Node(T data) : this(data, null) { }
    public Node(T data, NodeList<T> neighbors)
    {
        this.data = data;
        this.neighbors = neighbors;
    }

    public T Value { get; set; }

    protected NodeList<T> Neighbors { get; set; }

}

public class NodeList<T> : Collection<Node<T>>
{
    public NodeList() : base() { }

    public NodeList(int initialSize)
    {
        for (int i = 0; i < initialSize; i++)
        {
            base.Items.Add(default(Node<T>));
        }
    }

    public Node<T> FindByValue(T value)
    {
        foreach (Node<T> node in Items)
        {
            if (node.Value.Equals(value))
            {
                return node;
            }
        }

        return null;
    }
}
public class Graph<T> : IEnumerable<Node<T>>
{
    private NodeList<T> nodeSet;

    public NodeList<T> Nodes { get; }

    public Graph() : this(null) { }
    public Graph(NodeList<T> nodeSet)
    {
        if (nodeSet == null)
        {
            this.nodeSet = new NodeList<T>();
        }
        else
        {
            this.nodeSet = nodeSet;
        }
    }

    public void AddNode(GraphNode<T> node)
    {
        nodeSet.Add(node);
    }

    public void AddNode(T value)
    {
        nodeSet.Add(new GraphNode<T>(value));
    }

    public void AddDirectedEdge(GraphNode<T> from, GraphNode<T> to, int cost)
    {
        from.Neighbors.Add(to);
        from.Costs.Add(cost);
    }

    public void AddUndirectedEdge(GraphNode<T> from, GraphNode<T> to, int cost)
    {
        AddDirectedEdge(from, to, cost); //This was duplicated so just call the existing value

        to.Neighbors.Add(to);
        to.Costs.Add(cost);
    }

    public bool contains(T value)
    {
        return nodeSet.FindByValue(value) != null;
    }

    public bool Remove(T value)
    {
        // Remove node from nodeset
        GraphNode<T> nodeToRemove = (GraphNode<T>)nodeSet.FindByValue(value);
        if (nodeToRemove == null)
        {
            // wasnt found
            return false;
        }

        // was found
        nodeSet.Remove(nodeToRemove);

        // enumerate through each node in nodeSet, removing edges to this node
        foreach (GraphNode<T> gnode in nodeSet)
        {
            int index = gnode.Neighbors.IndexOf(nodeToRemove);
            if (index != -1)
            {
                gnode.Neighbors.RemoveAt(index);
                gnode.Costs.RemoveAt(index);
            }
        }

        return true;
    }

    public int Count
    {
        get { return nodeSet.Count; }
    }

    public IEnumerator<Node<T>> GetEnumerator()
    {
        return nodeSet.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
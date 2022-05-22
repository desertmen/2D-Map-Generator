using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ConnectionsFinder
{
    Vector2Int[] indexEdges;
    float[] weights;
    Vector2[] roomCenters;
    public ConnectionsFinder(Vector2[][] edges, Vector2[] roomCenters)
    {
        this.roomCenters = roomCenters;
        indexEdges = new Vector2Int[edges.Length];
        weights = new float[edges.Length];
        for (int i = 0; i < edges.Length; i++)
        {
            indexEdges[i] = new Vector2Int(Array.IndexOf(roomCenters, edges[i][0]), Array.IndexOf(roomCenters, edges[i][1]));
            weights[i] = (edges[i][0] - edges[i][1]).magnitude;
        }
        Array.Sort(weights, indexEdges);
    }

    public int[][] getMST(bool drawMST = false)
    {
        List<SubTree> subTrees = new List<SubTree>();
        List<int> indexesInMST = new List<int>();
        SubTree firstTree = new SubTree();
        firstTree.add(indexEdges[0]);
        subTrees.Add(firstTree);
        foreach(Vector2Int edge in indexEdges)
        {
            if(indexesInMST.Count == roomCenters.Length - 1)
            {
                break;
            }
            if(edge == indexEdges[0])
            {
                continue;
            }
            bool edgeAdded = false;
            bool isLooping = false;
            SubTree addedTree = null;
            foreach(SubTree subtree in subTrees)
            {
                if(edgeAdded == true)
                {
                    if(subtree.containsIndex(edge.x) || subtree.containsIndex(edge.y))
                    {
                        SubTree mergedTree = subtree.merge(addedTree);
                        subTrees.Add(mergedTree);
                        subTrees.Remove(addedTree);
                        subTrees.Remove(subtree);
                        break;
                    }
                }

                else if(subtree.containsIndex(edge.x) || subtree.containsIndex(edge.y))
                {
                    if (!subtree.makesLoop(edge))
                    {
                        subtree.add(edge);
                        edgeAdded = true;
                        addedTree = subtree;
                    }
                    else
                    {
                        isLooping = true;
                        break;
                    }
                }
            }

            if(edgeAdded == false && isLooping == false)
            {
                SubTree newSubTree = new SubTree();
                newSubTree.add(edge);
                subTrees.Add(newSubTree);
            }
        }

        if (drawMST)
        {
            foreach(Vector2Int edge in subTrees[0].edges)
            {
                Debug.DrawLine(roomCenters[edge.x], roomCenters[edge.y], Color.red, 5);
            }
        }

        int[][] connections = new int[subTrees[0].edges.Count][];
        for(int i = 0; i < connections.Length; i++)
        {
            connections[i] = new int[2];
        }
        int index = 0;
        foreach (Vector2Int edge in subTrees[0].edges)
        {
            connections[index][0] = edge.x;
            connections[index][1] = edge.y;
            index++;
        }
        // connections has one -1 int
        return connections;
    }

    public class SubTree
    {
        public List<Vector2Int> edges;
        public List<int> indexes;

        public SubTree()
        {
            edges = new List<Vector2Int>();
            indexes = new List<int>();
        }

        public SubTree(List<Vector2Int> _edges, List<int> _indexes)
        {
            edges = _edges;
            indexes = _indexes;
        }

        public void add(Vector2Int edge)
        {
            edges.Add(edge);
            if (!indexes.Contains(edge.y))
            {
                indexes.Add(edge.y);
            }
            if (!indexes.Contains(edge.x))
            {
                indexes.Add(edge.x);
            }
        }

        public bool makesLoop(Vector2Int newEdge)
        {
            if(indexes.Contains(newEdge.x) && indexes.Contains(newEdge.y))
            {
                return true;
            }
            return false;
        }

        public bool containsIndex(int index)
        {
            if (indexes.Contains(index))
            {
                return true;
            }
            return false;
        }

        public SubTree merge(SubTree tree)
        {
            SubTree newSubTree = new SubTree(tree.edges, tree.indexes);
            foreach(Vector2Int edge in edges)
            {
                newSubTree.add(edge);
            }
            return newSubTree;
        }

        public override string ToString()
        {
            string String = "";
            foreach(Vector2Int edge in edges)
            {
                String += edge.x.ToString() + " - " + edge.y.ToString() + ", ";
            }
            return String;
        }
    }
}

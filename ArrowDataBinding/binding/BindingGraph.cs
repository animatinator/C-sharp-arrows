using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArrowDataBinding.Bindings.Graph
{
    public class BindingNode
    {
        public BindPoint Vertex { get; set; }
        public List<BindingNode> AdjacentNodes { get; set; }

        public BindingNode(BindPoint point)
        {
            Vertex = point;
            AdjacentNodes = new List<BindingNode>();
        }

        public void AddEdgeTo(BindingNode other)
        {
            AdjacentNodes.Add(other);
        }
    }

    public class BindingGraph
    {
        private List<BindingNode> nodes;

        public BindingGraph()
        {
            nodes = new List<BindingNode>();
        }

        public void Add(BindPoint point)
        {
            if (NotAlreadyInGraph(point))
            {
                nodes.Add(new BindingNode(point));
            }
        }

        public bool NotAlreadyInGraph(BindPoint point)
        {
            return nodes.Where(x => x.Vertex.Equals(point)).FirstOrDefault() == null;
        }

        public void Bind(BindPoint first, BindPoint second)
        {
            BindingNode firstNode = FindNodeForPoint(first);
            BindingNode secondNode = FindNodeForPoint(second);

            firstNode.AddEdgeTo(secondNode);
        }

        public void TwoWayBind(BindPoint first, BindPoint second)
        {
            Bind(first, second);
            Bind(second, first);
        }

        private BindingNode FindNodeForPoint(BindPoint point)
        {
            return nodes.First(x => x.Vertex.Equals(point));
        }


        public bool HasCycle()
        {
            HashSet<BindingNode> unvisited = new HashSet<BindingNode>();
            foreach (BindingNode node in nodes)
            {
                unvisited.Add(node);
            }

            bool cycle = false;
            BindingNode currentNode;

            while (unvisited.Count > 0)
            {
                currentNode = unvisited.First(x => true);
                cycle = cycle || CycleFromCurrentNode(currentNode, null, unvisited, new HashSet<BindingNode>());
            }

            return cycle;
        }

        private bool CycleFromCurrentNode(BindingNode node, BindingNode previousNode, HashSet<BindingNode> unvisited, HashSet<BindingNode> seen)
        {
            if (seen.Contains(node)) return true;
            else
            {
                unvisited.Remove(node);
                seen.Add(node);

                bool cycle = false;

                foreach (BindingNode child in node.AdjacentNodes
                    .Where(child => !child.Equals(previousNode)))  // Prevent the traversal from looping back over two-way bindings
                {
                    cycle = cycle || CycleFromCurrentNode(child, node, unvisited, seen);
                }

                return cycle;
            }
        }

        public BindingGraph Copy()
        {
            BindingGraph newGraph = new BindingGraph();

            foreach (BindingNode node in nodes)
            {
                newGraph.nodes.Add(node);
            }

            return newGraph;
        }
    }
}

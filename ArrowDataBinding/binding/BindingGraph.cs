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

        public void AddMany(params BindPoint[] points)
        {
            foreach (BindPoint point in points)
            {
                Add(point);
            }
        }

        public bool NotAlreadyInGraph(BindPoint point)
        {
            return nodes.Where(x => x.Vertex.Equals(point)).FirstOrDefault() == null;
        }

        public void Bind(BindPoint first, BindPoint second)
        {
            /*
             * Link the two nodes in one direction
             */

            BindingNode firstNode = FindNodeForPoint(first);
            BindingNode secondNode = FindNodeForPoint(second);

            firstNode.AddEdgeTo(secondNode);
        }

        public void TwoWayBind(BindPoint first, BindPoint second)
        {
            /*
             * Link the two nodes in both directions
             */

            Bind(first, second);
            Bind(second, first);
        }

        public void MultiBind(BindPoint[] sources, BindPoint[] destinations)
        {
            /*
             * Link all sources to all destinations (possibly an over-estimate but ensures safety
             * in checking for conflicts)
             */

            foreach (BindPoint source in sources)
            {
                foreach (BindPoint dest in destinations)
                {
                    Bind(source, dest);
                }
            }
        }

        public void TwoWayMultiBind(BindPoint[] firsts, BindPoint[] seconds)
        {
            /*
             * Link all sources to all destinations in both directions
             */

            MultiBind(firsts, seconds);
            MultiBind(seconds, firsts);
        }

        public void RemoveBindings(BindPoint[] sources, BindPoint[] destinations)
        {
            /*
             * Remove all bindings between the sources and the destinations
             */

            foreach (BindPoint source in sources)
            {
                BindingNode node = FindNodeForPoint(source);
                node.AdjacentNodes.RemoveAll((BindingNode n) => destinations.Contains(n.Vertex));
            }
        }

        private BindingNode FindNodeForPoint(BindPoint point)
        {
            /*
             * Find the BindingNode associated with a particular BindPoint
             */

            return nodes.First(x => x.Vertex.Equals(point));
        }

        
        public bool HasConflict()
        {
            /*
             * Checks for the situation where a destination node is reachable from a source by more
             * than one path, which would lead to a potential conflict.
             */

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
                cycle = CycleFromCurrentNode(currentNode, null, unvisited, new HashSet<BindingNode>()) || cycle;
            }

            return cycle;
        }

        private bool CycleFromCurrentNode(BindingNode node, BindingNode previousNode, HashSet<BindingNode> unvisited, HashSet<BindingNode> seen)
        {
            /*
             * Recursively checks whether the current node will lead to a cycle
             */

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
            /*
             * Used to make alterations and checks on a temporary copy of the binding graph
             */

            BindingGraph newGraph = new BindingGraph();

            foreach (BindingNode node in nodes)
            {
                newGraph.nodes.Add(node);
            }

            return newGraph;
        }
    }
}

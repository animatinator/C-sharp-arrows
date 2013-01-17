using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ArrowDataBinding.Bindings.BindingGraph
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
            nodes.Add(new BindingNode(point));
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using ForestTaxator.Utils;
using static ForestTaxator.Utils.MathUtils;
using static ForestTaxator.Utils.MathUtils.EDistanceMetric;

namespace ForestTaxator.Model
{
    public class Tree
    {
        public class Node
        {
            public Ellipsis Ellipse { get; set; }
            public PointSet PointSet { get; set; }
            public Point Center { get; set; }
            public Node Parent { get; set; }
            public Tree Tree { get; set; }
            public List<Node> Children { get; set; } = new List<Node>();

            public Node(PointSet pointSet)
            {
                PointSet = pointSet;
                Center = pointSet.Center.Clone();
            }

            public void AddChild(PointSet pointSet)
            {
                Children.Add(new Node(pointSet) { Tree = Tree });
                Children[^1].Parent = this;
            }

            public void AddChild(Node node)
            {
                node.Tree = Tree;
                Children.Add(node);
                Children[^1].Parent = this;
            }

            public void SetParent(Node parent)
            {
                Parent.Children.Remove(this);
                Parent.AddChild(this);
            }

            public void CalculateError()
            {
                if (Ellipse == null)
                {
                    return;
                }

                Ellipse.Error = 0;

                double errorSum = 0;
                var pointsCount = PointSet.Count;

                if (pointsCount <= 10)
                {
                    return;
                }

                for (var i = 0; i < pointsCount; i++)
                {
                    var distance = Distance(PointSet[i], Ellipse.FirstFocal, Euclidean, EDimension.X, EDimension.Y) +
                                    Distance(PointSet[i], Ellipse.SecondFocal, Euclidean, EDimension.X, EDimension.Y);

                    var difference = distance - 2 * Ellipse.MajorRadius;

                    errorSum += difference;
                }

                Ellipse.Error = errorSum / pointsCount;
            }

            public void ApproximateEllipse(double x, LinearParameters treeRadiusParameters)
            {
                var r = treeRadiusParameters.A;
                var lnOfP = treeRadiusParameters.B;

                Ellipse = new Ellipsis
                {
                    FirstFocal = Center,
                    SecondFocal = Center,
                    MajorRadius = Math.Sqrt(Math.Exp(r * Math.Log(x) + lnOfP))
                };

                CalculateError();
            }

            public Node Clone(Node parentNode)
            {
                var clone = new Node(PointSet.Clone())
                {
                    Center = Center.Clone(),
                    Parent = parentNode,
                    Ellipse = Ellipse?.Clone()
                };
                clone.Children = Children.Select(x => x.Clone(clone)).ToList();
                return clone;
            }

        }

        public Node Root { get; private set; }

        public void CreateRoot(PointSet pointSet) => Root = new Node(pointSet) { Tree = this };

        public void SetRoot(Node node)
        {
            node.AddChild(Root);
            Root = node;
        }

        public Node[] GetNodeByHeightWithLeaves(double height)
        {
            var nodes = new LinkedList<Node>();
            if (Root == null)
            {
                return nodes.ToArray();
            }

            if (Root != null && Root.Children.Count > 0 && Math.Round(Root.Children[0].Center.Z, 1, MidpointRounding.ToEven) == Math.Round(height, 1, MidpointRounding.ToEven))
            {
                foreach (var child in Root.Children)
                {
                    if (child != null)
                    {
                        nodes.AddLast(child);
                    }
                }

                return nodes.ToArray();
            }
            if (Root != null && Root.Children.Count == 0)
            {
                nodes.AddLast(Root);
                return nodes.ToArray();
            }

            foreach (var child in Root.Children)
            {
                var temp = GetNodeByHeight(child, height, true);
                foreach (var t in temp)
                {
                    nodes.AddLast(t);
                }
            }
            return nodes.ToArray();
        }

        public Node[] GetAllNodesAsVector()
        {
            var nodes = new List<Node>();
            if (Root != null)
            {
                nodes.Add(Root);
            }

            if (Root == null || Root.Children.Count <= 0)
            {
                return nodes.ToArray();
            }

            foreach (var child in Root.Children)
            {
                var temp = GetAllNodesAsVector(child);
                nodes.AddRange(temp);
            }

            return nodes.ToArray();
        }

        public Node GetNearestNode(Point point, out double distance)
        {
            Node node = null;
            var candidates = GetNodeByHeightWithLeaves(point.Z - 0.1f);
            double bestDist = int.MaxValue;

            if (candidates.Length > 0)
            {
                node = candidates[0];
            }

            foreach (var candidate in candidates)
            {
                if (Distance(candidate.Center, point, Euclidean, EDimension.Z) >= 2)
                {
                    continue;
                }

                var dist = Distance(candidate.Center, point, Manhattan, EDimension.X, EDimension.Y);

                if (dist >= bestDist)
                {
                    continue;
                }

                node = candidate;
                bestDist = dist;
            }

            distance = bestDist;
            return node;
        }

        public Node[] GetAllNodesAsVector(Node node)
        {
            var nodes = new List<Node> { node };

            if (node.Children.Count <= 0)
            {
                return nodes.ToArray();
            }

            foreach (var child in node.Children)
            {
                var temp = GetAllNodesAsVector(child);
                nodes.AddRange(temp);
            }

            return nodes.ToArray();
        }

        public Node GetHighestNode()
        {
            var nodes = GetNodeByHeightWithLeaves(double.MaxValue);
            if (nodes.Length <= 0)
            {
                return null;
            }

            var highest = nodes[0];
            foreach (var node in nodes)
            {
                if (node.Center.Z > highest.Center.Z)
                {
                    highest = node;
                }
            }

            return highest;
        }

        public void RegressMissingLevels(double terrainHeight, double treeHeight)
        {
            var nodes = GetAllNodesAsVector();
            var highest = GetHighestNode();

            var treeRadiusParameters = TreeRadiusRegression(nodes, treeHeight);

            RegressSectionsFromGroundToFirst(terrainHeight);
            RegressSectionsFromFirstToLast();
            RegressSectionEllipses(treeHeight, highest, treeRadiusParameters);

            if (treeHeight - highest.Center.Z > 0.1)
            {
                RegressLevelsFromLastToTop(treeHeight, highest, nodes);
            }

            SmoothEntireTree(treeHeight, treeRadiusParameters);
        }

        private void SmoothEntireTree(double treeHeight, LinearParameters treeRadiusParameters)
        {
            var nodes = GetAllNodesAsVector();

            var xp = MathUtils.Regression(
                nodes.Select(x => x.Center.Z).ToArray(),
                nodes.Select(x => x.Center.X).ToArray());
            var yp = MathUtils.Regression(
                nodes.Select(x => x.Center.Z).ToArray(),
                nodes.Select(x => x.Center.Y).ToArray());
            foreach (var node in nodes)
            {
                if (node.Ellipse == null)
                {
                    node.Ellipse = new Ellipsis(
                        new Point(xp.A * node.Center.Z + xp.B, yp.A * node.Center.Z + yp.B, node.Center.Z),
                        new Point(xp.A * node.Center.Z + xp.B, yp.A * node.Center.Z + yp.B, node.Center.Z),
                        0.0);
                }
                else
                {
                    node.Ellipse.SetFirstFocal(EDimension.X, (float)(xp.A * node.Ellipse.Center.Z + xp.B));
                    node.Ellipse.SetFirstFocal(EDimension.Y, (float)(yp.A * node.Ellipse.Center.Z + yp.B));

                    node.Ellipse.SetSecondFocal(EDimension.X, (float)(xp.A * node.Ellipse.Center.Z + xp.B));
                    node.Ellipse.SetSecondFocal(EDimension.Y, (float)(yp.A * node.Ellipse.Center.Z + yp.B));
                }

                node.ApproximateEllipse(treeHeight - node.Ellipse.Center.Z, treeRadiusParameters);
            }
        }

        public Tree Clone()
        {
            return new Tree()
            {
                Root = Root.Clone(null)
            };
        }

        private void RegressSectionsFromGroundToFirst(double terrainHeight)
        {
            var treeCenterParameters = RegressHighestNodes(GetHighestNode(), 10);
            var z = 0.1 * Math.Floor(10 * terrainHeight);

            var set = new PointSet
            {
                new(
                    treeCenterParameters[0].A * z + treeCenterParameters[0].B,
                    treeCenterParameters[1].A * z + treeCenterParameters[1].B,
                    z)
            };

            var last = Root;

            var selected = new Node(set)
            {
                Center = { X = set[0].X, Y = set[0].Y, Z = set[0].Z }
            };

            Root = selected;

            while (z + 0.11f < last.Center.Z)
            {
                z += 0.1f;
                set = new PointSet
                {  new CloudPoint(
                    treeCenterParameters[0].A * z + treeCenterParameters[0].B,
                    treeCenterParameters[1].A * z + treeCenterParameters[1].B,
                    z) };
                selected.AddChild(set);

                selected = selected.Children[0];
                selected.Center = set[0].Clone();
            }
            selected.AddChild(last);
            last.Parent = selected;
        }

        private void RegressSectionsFromFirstToLast()
        {
            var higher = GetHighestNode();
            var lower = higher.Parent;
            while (lower != null)
            {
                RegressNodesCentersBetween(higher, lower);
                higher = lower;
                lower = lower.Parent;
            }
        }

        private static IEnumerable<Node> GetNodeByHeight(Node node, double height, bool withLeafs)
        {
            var nodes = new List<Node>();
            if (node.Children.Count > 0 && Math.Round(node.Children[0].Center.Z, 1, MidpointRounding.ToEven) == Math.Round(height, 1, MidpointRounding.ToEven))
            {
                nodes.AddRange(node.Children);
                return nodes;
            }
            if (withLeafs && node.Children.Count == 0)
            {
                nodes.Add(node);
                return nodes;
            }
            foreach (var child in node.Children)
            {
                var temp = GetNodeByHeight(child, height, withLeafs);
                nodes.AddRange(temp);
            }
            return nodes;
        }

        private static LinearParameters TreeRadiusRegression(IEnumerable<Node> nodes, double treeHeight)
        {
            var xv = new List<double>();
            var yv = new List<double>();
            foreach (var node in nodes)
            {
                if (treeHeight - node.Center.Z <= 0 || node.Ellipse == null)
                {
                    continue;
                }

                xv.Add(Math.Log(treeHeight - node.Center.Z));
                yv.Add(Math.Log(node.Ellipse.MajorRadius * node.Ellipse.MajorRadius));
            }
            // y^2 = px^r => 2*ln(y) = r*ln(x) + ln(p)
            MathUtils.Regression(xv, yv, out var r, out var lnp);
            return new LinearParameters { A = r, B = lnp };
        }

        private static LinearParameters[] RegressHighestNodes(Node highestNode, int maxCount)
        {
            var xv = new List<double>();
            var yv = new List<double>();
            var zv = new List<double>();
            var n = 0;

            var node = highestNode;
            while (n < maxCount && node.Parent != null)
            {
                if (node.Ellipse != null)
                {
                    xv.Add(node.Center.X);
                    yv.Add(node.Center.Y);
                    zv.Add(node.Center.Z);
                    n++;
                }

                node = node.Parent;
            }

            MathUtils.Regression(zv, xv, out var aX, out var bX);
            MathUtils.Regression(zv, yv, out var aY, out var bY);

            return new[]
            {
                new LinearParameters() {A = aX, B = bX},
                new LinearParameters() {A = aY, B = bY},
            };
        }

        private static void RegressNodesCentersBetween(Node higher, Node lower)
        {
            if (higher.Center.Z <= lower.Center.Z + 0.1f)
            {
                return;
            }

            var nodes = new List<Node>();
            var t = higher;
            while (t != null && nodes.Count < 10)
            {
                nodes.Add(t);
                t = t.Parent;
            }
            nodes.Add(lower);


            var xp = MathUtils.Regression(
                nodes.Select(x => x.Center.Z).ToArray(),
                nodes.Select(x => x.Center.X).ToArray());
            var yp = MathUtils.Regression(
                nodes.Select(x => x.Center.Z).ToArray(),
                nodes.Select(x => x.Center.Y).ToArray());

            lower.Children.Remove(higher);

            var z = lower.Center.Z;
            while (z + 0.10f < higher.Center.Z)
            {
                z += 0.1f;
                var set = new PointSet
                {
                    new CloudPoint(xp.A * z + xp.B, yp.A * z + yp.B, 0.1 * Math.Floor(10 * z))
                };

                var regressedNode = new Node(set)
                {
                    Center = set[0].Clone(),
                    Parent = lower
                };
                lower.AddChild(regressedNode);
                lower = regressedNode;
            }

            lower.AddChild(higher);
            higher.Parent = lower;
        }

        private static void RegressSectionEllipses(double treeHeight, Node highest, LinearParameters treeRadiusParameters)
        {
            var current = highest.Parent;
            while (current != null)
            {
                if (current.Ellipse == null && treeHeight - current.Center.Z > 0)
                {
                    current.ApproximateEllipse(treeHeight - current.Center.Z, treeRadiusParameters);
                }

                current = current.Parent;
            }
        }

        private static void RegressLevelsFromLastToTop(double treeHeight, Node highest, IEnumerable<Node> nodes)
        {
            var treeCenterParameters = RegressHighestNodes(highest, 15);
            var treeRadiusParameters = TreeRadiusRegression(nodes, treeHeight);

            var z = highest.Center.Z;
            var last = highest;
            while (z + 0.1f < treeHeight)
            {
                z += 0.1f;
                var set = new PointSet
                {
                    new CloudPoint(
                        treeCenterParameters[0].A * z + treeCenterParameters[0].B,
                        treeCenterParameters[1].A * z + treeCenterParameters[1].B,
                        z)
                };

                var current = new Node(set)
                {
                    Center = set[0].Clone(),
                    Parent = last
                };

                if (treeHeight - current.Center.Z > 0)
                {
                    current.ApproximateEllipse(treeHeight - current.Center.Z, treeRadiusParameters);
                }

                last.AddChild(current);
                last = current;
            }
        }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using ForestTaxator.Lib.Algorithms;
using static ForestTaxator.Lib.Utils.MathUtils;
using static ForestTaxator.Lib.Utils.MathUtils.EDistanceMetric;

namespace ForestTaxator.Lib.Model
{
    public class Tree
    {
        private static IList<Node> GetNodeByHeight(Node node, double height, bool withLeafs)
        {
            var nodes = new List<Node>();
            if (node.Children.Count > 0 && Math.Round(node.Children[0].Center.Z, 1, MidpointRounding.ToEven) ==
                Math.Round(height, 1, MidpointRounding.ToEven))
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

        private static LinearParameters TreeRadiusRegression(IList<Node> nodes, double treeHeight)
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
                yv.Add(2 * Math.Log(node.Ellipse.MajorRadius));
            }

            // y^2 = px^r => 2*ln(y) = r*ln(x) + ln(p)
            // x - height from the TOP
            // y - tree radius
            // r - tree shape exponent
            // p - tree shape parameter
            Regression(xv, yv, out var r, out var lnp);
            return new LinearParameters {A = r, B = Math.Exp(lnp)};
        }

        private static LinearParameters[] RegressNodePositions(Node highestNode, int maxCount)
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

            Regression(zv, xv, out var aX, out var bX);
            Regression(zv, yv, out var aY, out var bY);

            return new[]
            {
                new LinearParameters {A = aX, B = bX},
                new LinearParameters {A = aY, B = bY},
            };
        }

        private static void RegressLevelsFromLastToTop(double treeHeight, Node highest, IList<Node> nodes, double sliceHeight)
        {
            var treeCenterParameters = RegressNodePositions(highest, 15);
            var treeRadiusParameters = TreeRadiusRegression(nodes, treeHeight);

            var z = highest.Center.Z;
            var last = highest;
            while (z + sliceHeight < treeHeight)
            {
                z += sliceHeight;
                var set = new PointSet
                {
                    new(
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

        private static void RegressNodesCentersBetween(Node higher, Node lower, double sliceHeight)
        {
            if (higher.Center.Z - lower.Center.Z <= sliceHeight + 0.01)
            {
                // We do not have anything missing, so leave
                return;
            }

            var nodes = new List<Node>();
            var t = higher;
            var lowerAlreadyTaken = false;

            // Let's take 10 subsequent nodes for regression data
            while (t != null && nodes.Count < 10)
            {
                nodes.Add(t);
                t = t.Parent;
                if (t == lower)
                {
                    lowerAlreadyTaken = true;
                }
            }

            if (!lowerAlreadyTaken)
            {
                // And let's add lower node, to make it more precise
                nodes.Add(lower);
            }

            var heights = nodes.Select(x => x.Center.Z).ToArray();

            var xp = Regression(heights, nodes.Select(x => x.Center.X).ToArray());
            var yp = Regression(heights, nodes.Select(x => x.Center.Y).ToArray());

            // Detach higher node from lower
            lower.Children.Remove(higher);

            var z = lower.Center.Z;
            // Repeat regression until we reach higher node
            while (higher.Center.Z - z - sliceHeight > 0.01)
            {
                z += sliceHeight;
                // Create pointSet with single point
                var set = new PointSet
                {
                    new(xp.A * z + xp.B, yp.A * z + yp.B, sliceHeight * Math.Floor(z / sliceHeight))
                };

                var regressedNode = new Node(set)
                {
                    Center = set[0].Clone(),
                    Parent = lower,
                    Tree = lower.Tree
                };
                lower.AddChild(regressedNode);
                lower = regressedNode;
            }

            lower.AddChild(higher);
            higher.Parent = lower;
        }

        private static void RegressSectionEllipses(double treeHeight, Node highest, LinearParameters treeRadiusParameters, double sliceHeight)
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

        private void RegressSectionsFromFirstToLast(double sliceHeight)
        {
            var higher = GetHighestNode();
            var lower = higher.Parent;
            while (lower != null)
            {
                RegressNodesCentersBetween(higher, lower, sliceHeight);
                higher = lower;
                lower = lower.Parent;
            }
        }

        private void RegressSectionsFromGroundToFirst(double terrainHeight, double sliceHeight)
        {
            var highestChild = Root;
            var index = 0;
            while (index < 10 && (highestChild.Children?.Count ?? 0) > 0)
            {
                highestChild = highestChild
                    .Children
                    .OrderBy(c => Distance(c.Center, highestChild.Center, Euclidean, EDimension.X, EDimension.Y)).First();
                index++;
            }

            var treeCenterParameters = RegressNodePositions(highestChild, 10);
            var z = sliceHeight * Math.Floor(terrainHeight / sliceHeight);

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
                Center = {X = set[0].X, Y = set[0].Y, Z = set[0].Z}
            };

            Root = selected;

            while (last.Center.Z - z - sliceHeight > 0.01)
            {
                set = new PointSet
                {
                    new(treeCenterParameters[0].A * z + treeCenterParameters[0].B, treeCenterParameters[1].A * z + treeCenterParameters[1].B,
                        z)
                };
                selected.AddChild(set);
                selected = selected.Children[0];
                selected.Center = set[0].Clone();
                z += sliceHeight;
            }

            selected.AddChild(last);
            last.Parent = selected;
        }

        private void SmoothEntireTree(double treeHeight, LinearParameters treeRadiusParameters, double sliceHeight)
        {
            var nodes = GetAllNodesAsVector();

            var heights = nodes.Select(x => x.Center.Z).ToArray();

            Point previousFirstFocal = null;
            var firstFocis = new List<Point>();
            var secondFocis = new List<Point>();
            foreach (var node in nodes)
            {
                if (previousFirstFocal == null)
                {
                    firstFocis.Add(node.Ellipse.FirstFocal);
                    secondFocis.Add(node.Ellipse.SecondFocal);
                    previousFirstFocal = node.Ellipse.FirstFocal;
                    continue;
                }

                var dist1 = Distance(previousFirstFocal, node.Ellipse.FirstFocal, EuclideanSquare, EDimension.X, EDimension.Y);
                var dist2 = Distance(previousFirstFocal, node.Ellipse.SecondFocal, EuclideanSquare, EDimension.X, EDimension.Y);
                var dist1LessDist2 = dist1 < dist2;
                previousFirstFocal = dist1LessDist2 ? node.Ellipse.FirstFocal : node.Ellipse.SecondFocal;
                firstFocis.Add(dist1LessDist2 ? node.Ellipse.FirstFocal : node.Ellipse.SecondFocal);
                secondFocis.Add(dist1LessDist2 ? node.Ellipse.SecondFocal : node.Ellipse.FirstFocal);
            }

            var xp1 = Regression(heights, firstFocis.Select(p => p.X).ToArray());
            var yp1 = Regression(heights, firstFocis.Select(p => p.Y).ToArray());

            var xp2 = Regression(heights, secondFocis.Select(p => p.X).ToArray());
            var yp2 = Regression(heights, secondFocis.Select(p => p.Y).ToArray());

            foreach (var node in nodes)
            {
                var height = sliceHeight * Math.Floor(node.Center.Z / sliceHeight);
                if (node.Ellipse == null)
                {
                    node.Ellipse = new Ellipsis(
                        new Point(xp1.A * height + xp1.B, yp1.A * height + yp1.B, height),
                        new Point(xp2.A * height + xp2.B, yp2.A * height + yp2.B, height),
                        0.0);
                    node.ApproximateEllipse(treeHeight - height, treeRadiusParameters);
                }
                else
                {
                    node.Ellipse.SetFirstFocal(EDimension.X, (float) (xp1.A * height + xp1.B));
                    node.Ellipse.SetFirstFocal(EDimension.Y, (float) (yp1.A * height + yp1.B));
                    node.Ellipse.SetFirstFocal(EDimension.Z, (float) height);

                    node.Ellipse.SetSecondFocal(EDimension.X, (float) (xp2.A * height + xp2.B));
                    node.Ellipse.SetSecondFocal(EDimension.Y, (float) (yp2.A * height + yp2.B));
                    node.Ellipse.SetSecondFocal(EDimension.Z, (float) height);
                }
            }
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

        public Node GetNearestNode(Point point, out double distance, double sliceHeight)
        {
            Node node = null;
            var candidates = GetNodeByHeightWithLeaves(point.Z - sliceHeight);
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

        public Node Root { get; private set; }

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

        public Node[] GetAllNodesAsVector(Node node)
        {
            var nodes = new List<Node> {node};

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

        public Node[] GetNodeByHeightWithLeaves(double height)
        {
            var nodes = new LinkedList<Node>();
            if (Root == null)
            {
                return nodes.ToArray();
            }

            if (Root != null
                && Root.Children.Count > 0
                && Math.Round(Root.Children[0].Center.Z, 1, MidpointRounding.ToEven) == Math.Round(height, 1, MidpointRounding.ToEven)
            )
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

        public Tree Clone()
        {
            return new Tree()
            {
                Root = Root.Clone(null)
            };
        }

        public void CreateRoot(PointSet pointSet) => Root = new Node(pointSet) {Tree = this};

        public void RegressMissingLevels(double terrainHeight, double treeHeight, double sliceHeight)
        {
            var nodes = GetAllNodesAsVector();
            var highest = GetHighestNode();

            var treeRadiusParameters = TreeRadiusRegression(nodes, treeHeight);

            RegressSectionsFromGroundToFirst(terrainHeight, sliceHeight);

            RegressSectionsFromFirstToLast(sliceHeight);

            RegressSectionEllipses(treeHeight, highest, treeRadiusParameters, sliceHeight);

            if (treeHeight - highest.Center.Z > sliceHeight + 0.01)
            {
                RegressLevelsFromLastToTop(treeHeight, highest, nodes, sliceHeight);
            }

            // SmoothEntireTree(treeHeight, treeRadiusParameters, sliceHeight);
        }

        public void SetRoot(Node node)
        {
            node.AddChild(Root);
            Root = node;
        }


        public class Node
        {
            public Node(PointSet pointSet)
            {
                PointSet = pointSet;
                Center = pointSet.Center.Clone();
            }

            public Ellipsis Ellipse { get; set; }
            public PointSet PointSet { get; set; }
            public Point Center { get; set; }
            public Node Parent { get; set; }
            public Tree Tree { get; set; }
            public List<Node> Children { get; set; } = new List<Node>();

            public Node AddChild(PointSet pointSet)
            {
                Children.Add(new Node(pointSet) {Tree = Tree});
                Children[^1].Parent = this;
                return Children[^1];
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
                var p = treeRadiusParameters.B;

                Ellipse = new Ellipsis
                {
                    FirstFocal = Center,
                    SecondFocal = Center,
                    MajorRadius = Math.Sqrt(p * Math.Pow(x, r)) // Math.Sqrt(Math.Exp(r * Math.Log(x) + lnOfP))
                };

                CalculateError();
                Ellipse.Intensity = Ellipse.Error;
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

            public bool PointsOutTo(Point targetPoint, MergingParameters parameters)
            {
                var currentHeight = targetPoint.Z;
                if (currentHeight - Center.Z >= parameters.MaximumGroupingEmptyHeight)
                {
                    return false;
                }

                var xv = new List<double>();
                var yv = new List<double>();
                var zv = new List<double>();
                var currentNode = this;

                while (currentNode != null && Center.Z - currentNode.Center.Z < parameters.RegressionDistance)
                {
                    xv.Add((float) currentNode.Center.X);
                    yv.Add((float) currentNode.Center.Y);
                    zv.Add((float) currentNode.Center.Z);

                    currentNode = currentNode.Parent;
                }

                if (xv.Count <= parameters.MinimumRegressionNodes)
                {
                    return false;
                }

                // If do not work swap aX and aY
                Regression(zv, xv, out var aX, out var bX);
                Regression(zv, yv, out var aY, out var bY);

                var p = new Point(aX * currentHeight + bX, aY * currentHeight + bY, currentHeight);

                var regressedPointToGroupCenterDist = Distance(p, targetPoint, EuclideanSquare, EDimension.X, EDimension.Y);

                return regressedPointToGroupCenterDist < parameters.MinimumRegressionGroupingDistance;
            }
        }
    }
}
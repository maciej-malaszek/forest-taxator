using System.Collections.Generic;
using System.Linq;
using ForestTaxator.Model;
using ForestTaxator.Utils;

namespace ForestTaxator.Algorithms.Detection
{
    public partial class TreeDetectionAlgorithm
    {
        private static bool IsRegressionMatched(Tree.Node node, PointSet pointSet, double currentHeight, MergingParameters parameters)
        {
            if (currentHeight - node.Center.Z >= parameters.MaximumGroupingEmptyHeight)
            {
                return false;
            }

            var xv = new List<double>();
            var yv = new List<double>();
            var zv = new List<double>();
            var currentNode = node;

            while (currentNode != null && node.Center.Z - currentNode.Center.Z < 1.0)
            {
                xv.Add((float) currentNode.Center.X);
                yv.Add((float) currentNode.Center.Y);
                zv.Add((float) currentNode.Center.Z);

                currentNode = currentNode.Parent;
            }

            if (xv.Count <= 5)
            {
                return false;
            }

            // If do not work swap aX and aY
            MathUtils.Regression(zv, xv, out var aX, out var bX);
            MathUtils.Regression(zv, yv, out var aY, out var bY);

            var p = new Point(aX * currentHeight + bX, aY * currentHeight + bY, currentHeight);

            var regressedPointToGroupCenterDist = MathUtils.Distance(p, pointSet.Center,
                MathUtils.EDistanceMetric.EuclideanSquare, MathUtils.EDimension.X, MathUtils.EDimension.Y);

            return regressedPointToGroupCenterDist < parameters.MinimumRegressionGroupingDistance;
        }

        private static Tree.Node FindBestNode(PointSet pointSet, IEnumerable<Tree> potentialTrees, MergingParameters parameters)
        {
            double bestDistance = int.MaxValue;
            Tree.Node bestTreeNode = null;

            foreach (var tree in potentialTrees)
            {
                var z = pointSet.Center.Z;
                var nodes = tree.GetNodeByHeightWithLeaves(z);

                foreach (var node in nodes)
                {
                    if (IsRegressionMatched(node, pointSet, z, parameters))
                    {
                        return node;
                    }
                }

                var treeNode = tree.GetNearestNode(pointSet.Center, out var distance);

                if (treeNode == null)
                {
                    continue;
                }

                if (distance >= parameters.MinimumGroupingDistance)
                {
                    continue;
                }

                if (distance >= bestDistance)
                {
                    continue;
                }

                if (pointSet.Center.Z - treeNode.Center.Z >= parameters.MaximumGroupingEmptyHeight)
                {
                    continue;
                }

                if (pointSet.Center.Z - treeNode.Center.Z <= 0)
                {
                    continue;
                }

                bestDistance = distance;
                bestTreeNode = treeNode;
            }

            return bestTreeNode;
        }

        public static ICollection<Tree> MergeGroupsIntoTrees(IEnumerable<PointSet> groups, MergingParameters parameters)
        {
            var potentialTrees = new List<Tree>();
            foreach (var pointSet in groups)
            {
                var node = FindBestNode(pointSet, potentialTrees, parameters);
                if (node != null)
                {
                    if (pointSet.Center.Z > node.Center.Z)
                    {
                        node?.AddChild(pointSet);
                    }
                    else
                    {
                        var t = node.Parent;

                        while (t != null && t.Center.Z > pointSet.Center.Z)
                        {
                            t = t.Parent;
                        }

                        if (t == null)
                        {
                            t = new Tree.Node(pointSet) {Tree = node.Tree};
                            node.Tree.SetRoot(t);
                        }
                        else
                        {
                            var newNode = new Tree.Node(pointSet) {Tree = t.Tree};
                            var children = t.Children.Where(x => x.Center.Z > pointSet.Center.Z).ToArray();
                            foreach (var child in children)
                            {
                                child.SetParent(newNode);
                            }

                            t.AddChild(newNode);
                        }
                    }

                    continue;
                }

                if (pointSet.Count < 20 || pointSet.Center.Z >= 12)
                {
                    continue;
                }

                var newTree = new Tree();
                newTree.CreateRoot(pointSet);
                potentialTrees.Add(newTree);
            }

            return potentialTrees;
        }
    }
}
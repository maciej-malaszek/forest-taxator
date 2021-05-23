using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ForestTaxator.Algorithms;
using ForestTaxator.Filters;

namespace ForestTaxator.Model
{
    public class PointSetGroup
    {
        public IList<PointSet> PointSets { get; set; }

        public PointSetGroup()
        {
        }

        public PointSetGroup(IList<PointSet> pointSets)
        {
            PointSets = pointSets.Where(p => p is {Empty: false}).ToList();
        }

        public PointSetGroup Filter(params IPointSetFilter[] filters)
        {
            if (PointSets?.FirstOrDefault() == null)
            {
                return null;
            }
            Console.WriteLine($"Starting on height {PointSets.FirstOrDefault()?.Center?.Z ?? 0}");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var pointSets = filters.Aggregate(PointSets, (current, filter) => filter.Filter(current));
            stopwatch.Stop();
            Console.WriteLine($"Last slice calculation time: {stopwatch.Elapsed.TotalSeconds}");
            return new PointSetGroup(pointSets);
        }

        public IList<Tree> BuildTrees(MergingParameters parameters)
        {
            var potentialTrees = new List<Tree>();
            foreach (var pointSet in PointSets)
            {
                var node = pointSet.FindBestNode(potentialTrees, parameters);
                if (node == null)
                {
                    if (pointSet.Count < 20 || pointSet.Center.Z >= 12)
                    {
                        continue;
                    }

                    var newTree = new Tree();
                    newTree.CreateRoot(pointSet);
                    potentialTrees.Add(newTree);
                }
                else
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
                }
            }

            return potentialTrees;
        }
    }
}
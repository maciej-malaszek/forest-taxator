using System.Collections.Generic;
using System.Linq;

namespace ForestTaxator.Model
{
    public class RTree
    {
        public class RTreeNode
        {
            private readonly uint _capacity;
            public List<RTreeNode> Children { get; set; }
            public RTreeNode Parent { get; private set; }
            public Box Range { get; set; }
            public List<PointSet> PointSets { get; set; }

            public RTreeNode(uint capacity = 10)
            {
                _capacity = capacity;
            }

            public RTreeNode(RTreeNode parent, uint capacity = 10)
            {
                _capacity = capacity;
                Parent = parent;
                if (parent != null)
                {
                    Parent.Children.Add(this);
                }
            }

            private void Split()
            {
                if (Children.Count <= _capacity && PointSets.Count <= _capacity)
                {
                    return;
                }

                if (Parent == null)
                {
                    Parent = new RTreeNode();
                    Parent.Children.Add(this);
                    Parent.Range = Range;
                }

                var sibling = new RTreeNode(Parent);
                if (Children.Count == 0)
                {
                    var mid = PointSets.Count / 2;
                    sibling.PointSets = PointSets.Skip(mid).ToList();
                    PointSets.RemoveRange(mid, PointSets.Count - mid);
                }
                else
                {
                    var mid = Children.Count / 2;
                    sibling.Children = Children.Skip(mid).ToList();
                    Children.RemoveRange(mid, Children.Count - mid);
                    foreach (var siblingChild in sibling.Children)
                    {
                        siblingChild.Parent = sibling;
                    }
                }

                sibling.RecalculateRange(false);
                RecalculateRange(false);
                Parent.Split();
            }

            private void RecalculateRange(bool recursive)
            {
                if (Children.Count == 0)
                {
                    if (PointSets.Count == 0)
                    {
                        Range = new Box();
                    }
                    else
                    {
                        Range = PointSets[0].BoundingBox;
                        for (var i = 1; i < PointSets.Count; i++)
                        {
                            Range.Broaden(PointSets[i].BoundingBox);
                        }
                    }
                }
                else
                {
                    Range = Children[0].Range;
                    for (var i = 1; i < Children.Count; i++)
                    {
                        Range.Broaden(Children[i].Range);
                    }
                }
                if (Parent != null && recursive)
                {
                    Parent.RecalculateRange(true);
                }
            }

            private void AddChild(RTreeNode node)
            {
                node.Parent = this;
                Children.Add(node);
            }

            private bool OverlapsWith(Box box)
            {
                return Range.BoundsWithPlanar(box);
            }

            public void AddPoints(PointSet pointSet)
            {
                if (Children.Count == 0)
                {
                    if (PointSets.Count == 0)
                    {
                        Range = pointSet.BoundingBox;
                    }
                    else
                    {
                        Range.Broaden(pointSet.BoundingBox);
                    }

                    PointSets.Add(pointSet);
                    Split();
                }
                else
                {
                    var i = 0;
                    var bestChild = i;
                    var currentDiff = Children[i].Range.RdiffPlanar(pointSet.BoundingBox);

                    Range.Broaden(pointSet.BoundingBox);
                    for (i = 1; i < Children.Count; ++i)
                    {
                        var nextDiff = Children[i].Range.RdiffPlanar(pointSet.BoundingBox);
                        if (!(nextDiff < currentDiff))
                        {
                            continue;
                        }

                        bestChild = i;
                        currentDiff = nextDiff;
                    }
                    Children[bestChild].AddPoints(pointSet);
                }
            }

            public void RemovePoints(PointSet pointSet)
            {

                PointSets.Clear();
                Children.Clear();
                RecalculateRange(false);
            }

            public PointSet[] GetSetsFromBoundaryBox(Box box, bool planar = true)
            {
                var result = new LinkedList<PointSet>();

                if (Children.Count != 0)
                {
                    return result.ToArray();
                }

                foreach (var pointSet in PointSets)
                {
                    if (planar)
                    {
                        if (pointSet.BoundingBox.BoundsWithPlanar(box))
                        {
                            result.AddLast(pointSet);
                        }
                    }
                    else if (pointSet.BoundingBox.BoundsWith(box))
                    {
                        result.AddLast(pointSet);
                    }
                }

                return result.ToArray();
            }

            /// <summary>
            /// Returns number of PointSets including all children
            /// </summary>
            /// <returns></returns>
            public long Count()
            {
                if (Children.Count == 0)
                {
                    return (long)PointSets.Count;
                }

                return Children.Sum(child => child.Count());
            }

        }

        public RTreeNode Root { get; private set; }

        public void AddSet(PointSet pointSet)
        {
            if (pointSet.Empty)
            {
                return;
            }

            if (Root == null)
            {
                Root = new RTreeNode();
            }

            Root.AddPoints(pointSet);
        }

        public void RemoveSet(PointSet pointSet) => Root?.RemovePoints( pointSet );

        public PointSet[] GetSetsFromBoundaryBox(Box box, bool planar = true) => Root?.GetSetsFromBoundaryBox(box, planar);

        public void Clear() => Root = null;
    }
}

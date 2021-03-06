﻿using System;


namespace Supercluster.MTree
{
    using System.Collections.Generic;
    using System.Linq;

    using Supercluster.MTree.NewDesign;

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// 
    /// References:
    /// [1] P. Ciaccia, M. Patella, and P. Zezula. M-tree: an efficient access method for similarity search in metric spaces. 
    /// In Proceedings of the 23rd International Conference on Very Large Data Bases (VLDB), pages 426–435, Athens, Greece, August 1997
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class MTree<T>
    {
        public MTree()
        {
            this.Root = new MNode<T> { ParentEntry = null, Capacity = this.Capacity };
        }

        public int Capacity = 3;

        public MNode<T> Root;


        public Func<T, T, double> Metric;

        #region Add Method

        public void Add(T newEntry)
        {
            this.Add(this.Root, new MNodeEntry<T> { Value = newEntry });
        }

        private void Add(MNode<T> node, MNodeEntry<T> newNodeEntry)
        {
            /*
                Note for my first attempt I will implement that algorithm exactly as described in the paper
                P. Ciaccia, M. Patella, and P. Zezula. M-tree: an efficient access method for similarity search in metric spaces.
                Insert Algorithm
            */

            if (node.IsInternalNode)
            {
                var internalNode = node;
                var entries = internalNode.Entries;
                // let N_in = entries such that d(O_r, O_n) <= r(O_r)

                // these are the balls in which our newEntry already resides in
                // TODO: Don't double compute the distances
                var entries_in =
                    entries.Where(n => this.Metric(n.Value, newNodeEntry.Value) <= n.CoveringRadius).ToArray();
                MNodeEntry<T> closestEntry;
                if (entries_in.Length > 0) // new entry is currently in the region of a ball
                {
                    // TODO: This is the second computation
                    var elementDistances = entries_in.Select(e => this.Metric(e.Value, newNodeEntry.Value)).ToList();
                    closestEntry = entries_in[elementDistances.IndexOf(elementDistances.Min())];
                }
                else // the new element does not currently reside in any of the current regions balls
                {
                    // since we are not in any of the balls we find which ball we are closest to and extend that ball
                    // we choose the ball whose radius we must increase the least
                    var elementDistances =
                        entries.Select(e => this.Metric(e.Value, newNodeEntry.Value) - e.CoveringRadius).ToList();
                    closestEntry = entries[elementDistances.IndexOf(elementDistances.Min())];
                    closestEntry.CoveringRadius = this.Metric(closestEntry.Value, newNodeEntry.Value);
                }

                // recurse into the closest elements subtree
                this.Add(closestEntry.ChildNode, newNodeEntry);
            }
            else // node is a leaf node
            {
                if (!node.IsFull)
                {
                    if (node == this.Root)
                    {
                        node.Add(newNodeEntry);
                    }
                    else
                    {
                        newNodeEntry.DistanceFromParent = this.Metric(node.ParentEntry.Value, newNodeEntry.Value);
                        node.Add(newNodeEntry);
                    }
                }
                else
                {
                    this.Split(node, newNodeEntry);
                }
            }

        }

        #endregion

        private void Split(MNode<T> node, MNodeEntry<T> newEntry)
        {
            var nodeIsRoot = node == this.Root;
            MNode<T> parent = null;
            var parentEntryIndex = -1;

            if (!nodeIsRoot)
            {
                // keep reference to parent node
                parent = node.ParentEntry.ParentNode;
                parentEntryIndex = parent.Entries.IndexOf(node.ParentEntry);
                //if we are not the root, the get the parent of the current node.
            }

            // Create local copy of entries
            var entries = node.Entries.ToList();
            entries.Add(newEntry);

            var newNode = new MNode<T> { Capacity = this.Capacity };
            var promotionResult = this.Promote(entries.ToArray(), node.IsInternalNode); // TODO: Does not need to be an array
            node.Entries = promotionResult.FirstPartition;
            newNode.Entries = promotionResult.SecondPartition;

            // Set child nodes of promotion objects
            promotionResult.FirstPromotionObject.ChildNode = node;
            promotionResult.SecondPromotionObject.ChildNode = newNode;

            if (nodeIsRoot)
            {
                // if we are the root node, then create a new root and assign the promoted objects to them
                var newRoot = new MNode<T> { ParentEntry = null, Capacity = this.Capacity };
                newRoot.AddRange(
                    new List<MNodeEntry<T>>
                        {
                            promotionResult.FirstPromotionObject,
                            promotionResult.SecondPromotionObject
                        });

                this.Root = newRoot;
            }
            else // we are not the root
            {
                // Set distance from parent
                if (parent == this.Root)
                {
                    promotionResult.FirstPromotionObject.DistanceFromParent = -1;
                }
                else
                {
                    promotionResult.FirstPromotionObject.DistanceFromParent = this.Metric(promotionResult.FirstPromotionObject.Value, parent.ParentEntry.Value);
                }

                parent.SetEntryAtIndex(parentEntryIndex, promotionResult.FirstPromotionObject);
                if (parent.IsFull)
                {
                    this.Split(parent, promotionResult.SecondPromotionObject);
                }
                else
                {
                    // Set distance from parent
                    if (parent == this.Root)
                    {
                        promotionResult.SecondPromotionObject.DistanceFromParent = -1;
                    }
                    else
                    {
                        promotionResult.SecondPromotionObject.DistanceFromParent = this.Metric(promotionResult.SecondPromotionObject.Value, parent.ParentEntry.Value);
                    }

                    parent.Add(promotionResult.SecondPromotionObject);
                }

            }
        }



        // TODO: If we are willing to take a performance hit, we could abstract both the promote and partition methods
        // TODO: Some partition methods actually DEPEND on the partition method.
        /// <summary>
        /// Chooses two <see cref="MNodeEntry{T}"/>s to be promoted up the tree. The two nodes are chosen 
        /// according to the mM_RAD split policy with balanced partitions defined in reference [1] pg. 431.
        /// </summary>
        /// <param name="entries">The entries for which two node will be choose from.</param>
        /// <param name="isInternalNode">Specifies if the <paramref name="entries"/> list parameter comes from an internal node.</param>
        /// <returns>The indexes of the element pairs which are the two objects to promote</returns>
        private PromotionResult<T> Promote(MNodeEntry<T>[] entries, bool isInternalNode)
        {
            var uniquePairs = Utilities.UniquePairs(entries.Length);
            var distanceMatrix = new DistanceMatrix<T>(entries.Select(e => e.Value).ToArray(), this.Metric);
            // we only store the indexes of the pairs
            // var uniqueDistances = uniquePairs.Select(p => this.Metric(entries[p.Item1].Value, entries[p.Item2].Value)).Reverse().ToArray();

            /*
                2. mM_RAD Promotion an Balanced Partitioning
                
                Part of performing the mM_RAD promotion algorithm is
                implicitly calculating all possible partitions. 
                
                For each pair of objects we calculate a balanced partition.
                The pair for which the maximum of the two covering radii is the smallest
                is the objects we promote.

                In the iterations below, every thing is index-based to keep it as fast as possible.
            */


            // The minimum values which will be traced through out the mM_RAD algorithm
            var minPair = new Tuple<int, int>(-1, -1);
            var minMaxRadius = double.MaxValue;
            var minFirstPartition = new List<int>();
            var minSecondPartition = new List<int>();
            var minFirstPromotedObject = new MNodeEntry<T>();
            var minSecondPromotedObject = new MNodeEntry<T>();

            // We iterate through each pair performing a balanced partition of the remaining points.
            foreach (var pair in uniquePairs)
            {
                // Get the indexes of the points not in the current pair
                var pointsNotInPair =
                    Enumerable.Range(0, entries.Length).Except(new[] { pair.Item1, pair.Item2 }).ToList();
                //TODO: Optimize

                var partitions = this.BalancedPartition(pair, pointsNotInPair, distanceMatrix);
                var localFirstPartition = partitions.Item1;
                var localSecondPartition = partitions.Item2;


                /*
                    As specified in reference [1] pg. 430. If we are splitting a leaf node,
                    then the covering radius of promoted object O_1 with partition P_1 is

                    coveringRadius_O_1 = max{ distance(O_1, O_i) | where O_i in P_1 }

                    If we are splitting an internal node then the covering radius
                    of promoted object O_1 with partition P_1 is

                    coveringRadius_O_1 = max{ distance(O_1, O_i) + CoveringRadius of O_i | where O_i in P_1 }

                */

                var firstPromotedObjectCoveringRadius = localFirstPartition.MaxDistanceFromFirst(distanceMatrix);
                var secondPromotedObjectCoveringRadius = localSecondPartition.MaxDistanceFromFirst(distanceMatrix);
                var localMinMaxRadius = Math.Max(firstPromotedObjectCoveringRadius, secondPromotedObjectCoveringRadius);
                if (isInternalNode)
                {
                    firstPromotedObjectCoveringRadius = this.CalculateCoveringRadius(
                        pair.Item1,
                        localFirstPartition,
                        distanceMatrix,
                        entries);

                    secondPromotedObjectCoveringRadius = this.CalculateCoveringRadius(
                        pair.Item2,
                        localSecondPartition,
                        distanceMatrix,
                        entries);
                }

                if (localMinMaxRadius < minMaxRadius)
                {
                    minMaxRadius = localMinMaxRadius;
                    minPair = pair;

                    minFirstPromotedObject.CoveringRadius = firstPromotedObjectCoveringRadius;
                    minFirstPartition = localFirstPartition;

                    minSecondPromotedObject.CoveringRadius = secondPromotedObjectCoveringRadius;
                    minSecondPartition = localSecondPartition;
                }
            }


            /*

                3. Creating the MNodeEntry Objects

                Now that we have correctly identified the objects to be promoted an each partition
                we start setting and/or calculating some of the properties on the node entries
            
            */

            // set values of promoted objects
            var firstPartition = new List<MNodeEntry<T>>();
            var secondPartition = new List<MNodeEntry<T>>();
            minFirstPromotedObject.Value = entries[minPair.Item1].Value;
            minSecondPromotedObject.Value = entries[minPair.Item2].Value;


            // TODO: Set distance from parent in partition
            firstPartition.AddRange(entries.SubsetByIndex(minFirstPartition));
            for (int i = 0; i < firstPartition.Count; i++)
            {
                firstPartition[i].DistanceFromParent = distanceMatrix[minFirstPartition[0], minFirstPartition[i]];
            }

            secondPartition.AddRange(entries.SubsetByIndex(minSecondPartition));
            for (int i = 0; i < secondPartition.Count; i++)
            {
                secondPartition[i].DistanceFromParent = distanceMatrix[minSecondPartition[0], minSecondPartition[i]];
            }


            var promotionResult = new PromotionResult<T>
            {
                FirstPromotionObject = minFirstPromotedObject,
                SecondPromotionObject = minSecondPromotedObject,
                FirstPartition = firstPartition,
                SecondPartition = secondPartition
            };


            // TODO: This method is called from the split method. In the split method we call both promote an partition in one method

            return promotionResult;

        }

        private double CalculateCoveringRadius(
            int promotedObjectIndex,
            IReadOnlyList<int> partitionIndexes,
            DistanceMatrix<T> distanceMatrix,
            IReadOnlyList<MNodeEntry<T>> entries)
        {
            var maxRadius = double.MinValue;

            foreach (int index in partitionIndexes)
            {
                var radius = distanceMatrix[index, promotedObjectIndex] + entries[index].CoveringRadius;
                maxRadius = Math.Max(radius, maxRadius);
            }

            return maxRadius;
        }

        public void RangeSearch(MNode<T> node, T ballCenter, double ballRadius, List<T> results)
        {
            if (node == this.Root) // node is the root
            {
                foreach (var entry in node.Entries)
                {
                    this.RangeSearch(entry.ChildNode, ballCenter, ballRadius, results);
                }
            }
            else
            {
                var distanceParentToCenter = this.Metric(node.ParentEntry.Value, ballCenter);

                if (node.IsInternalNode)
                {
                    foreach (var entry in node.Entries)
                    {

                        var combinedRadius = entry.CoveringRadius + ballRadius;
                        var test = Math.Abs(distanceParentToCenter - entry.DistanceFromParent);
                        if (test <= combinedRadius)
                        {
                            var distanceCurrentToCenter = this.Metric(entry.Value, ballCenter);
                            if (distanceCurrentToCenter <= combinedRadius)
                            {
                                this.RangeSearch(entry.ChildNode, ballCenter, ballRadius, results);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var entry in node.Entries)
                    {
                        var test = Math.Abs(distanceParentToCenter - entry.DistanceFromParent);
                        if (test <= ballRadius)
                        {
                            var distanceCurrentToCenter = this.Metric(entry.Value, ballCenter);
                            if (distanceCurrentToCenter <= ballRadius)
                            {
                                results.Add(entry.Value);
                            }
                        }
                    }
                }
            }
        }

        public Tuple<List<int>, List<int>> BalancedPartition(
            Tuple<int, int> pair,
            List<int> pointsNotInPair,
            DistanceMatrix<T> distanceMatrix)
        {
            var firstPartition = new List<int> { pair.Item1 };
            var secondPartition = new List<int> { pair.Item2 };

            int minIndex = -1;
            int k = 0;
            while (pointsNotInPair.Count > 0)
            {
                var dist = double.MaxValue;

                if (k % 2 == 0)
                {
                    // Find which point is closest to the first promotion object
                    for (int i = 0; i < pointsNotInPair.Count; i++)
                    {
                        if (distanceMatrix[pointsNotInPair[i], pair.Item1] < dist)
                        {
                            dist = distanceMatrix[pointsNotInPair[i], pair.Item1];
                            minIndex = i;
                        }
                    }

                    firstPartition.Add(pointsNotInPair[minIndex]);
                }
                else
                {
                    // Here we calculate the index of the pair in the unique distances array
                    // this calculation depends on the unique distances array being reversed
                    for (int i = 0; i < pointsNotInPair.Count; i++)
                    {
                        if (distanceMatrix[pointsNotInPair[i], pair.Item2] < dist)
                        {
                            dist = distanceMatrix[pointsNotInPair[i], pair.Item2];
                            minIndex = i;
                        }
                    }

                    secondPartition.Add(pointsNotInPair[minIndex]);
                }

                pointsNotInPair.RemoveAt(minIndex);
                k++;
            }


            return new Tuple<List<int>, List<int>>(firstPartition, secondPartition);
        }
    }
}



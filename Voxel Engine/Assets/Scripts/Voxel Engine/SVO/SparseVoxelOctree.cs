using System;
using UnityEngine;

namespace Voxel_Engine.SVO
{
    /// <summary>
    /// A sparse voxel tree, adapted from https://eisenwave.github.io/voxel-compression-docs/svo/svo.html
    /// This implementation assumes a minimum voxel position to optimize the tree.
    /// </summary>
    /// <typeparam name="TColor"></typeparam>
    public class SparseVoxelOctree<TColor> where TColor : Enum
    {
        public SvoInnerNode Root { get; private set; } = new();

        private int _maxDepth;

        /// <summary>
        /// Creates a sparse voxel octree from a given list of voxels with colors.
        /// </summary>
        /// <param name="dimension">A power of 2, defining the dimension / size of the octree</param>
        /// <param name="voxelPositions">Sorted voxel positions in voxel-space, starting with the most negative point</param>
        /// <param name="voxelColors">Colors of the voxels</param>
        public SparseVoxelOctree(int dimension, Vector3Int[] voxelPositions, TColor[] voxelColors)
        {
            Debug.Assert(IsPowerOfTwo(dimension));

            _maxDepth = IndexHelper.GetMostSignificantBit((ulong)dimension);
            var pMin = voxelPositions[0];

            for (var index = 0; index < voxelPositions.Length; index++)
            {
                var voxelPosition = voxelPositions[index];
                // pNormalized is an unsigned coordinate
                var pNormalized = voxelPosition - pMin;
                Insert(pNormalized, voxelColors[index]);
            }
        }

        public SparseVoxelOctree(int dimension)
        {
            Debug.Assert(IsPowerOfTwo(dimension));

            _maxDepth = IndexHelper.GetMostSignificantBit((ulong)dimension);
        }

        public void Insert(Vector3Int voxelPosition, TColor voxelColor)
        {
            var octalNumber = GetOctalNumber(voxelPosition);
            Insert(octalNumber, voxelColor);
        }
        
        private void Insert(OctalNumber octalNumber, TColor insertionColor)
        {
            var depth = TraverseTree(octalNumber, out var branch, out _, out var parent, out var index);

            SvoArrayNode<TColor> newArrayNode;
            var insertionIndexInArray = octalNumber.GetOctalDigit(depth + 1);
            switch (branch)
            {
                case null:
                    // If at max depth, add leaf
                    if (depth >= _maxDepth)
                    {
                        var leafNode = new SvoLeafNode<TColor>(insertionColor);
                        parent[index] = leafNode;
                        break;
                    }
                    
                    // Otherwise we can add an array node with all the same colors.
                    newArrayNode = new SvoArrayNode<TColor>
                    {
                        [insertionIndexInArray] = insertionColor
                    };
                    parent[index] = newArrayNode;
                    break;
                
                case SvoArrayNode<TColor> arrayNode:
                    index = octalNumber.GetOctalDigit(depth);
                    if (arrayNode[index].Equals(insertionColor))
                        break;
                    
                    // Create a new array node where the color is the same for 7 suboctants. The insertion color has its own octant then
                    var oldColor = arrayNode[index];
                    newArrayNode = new SvoArrayNode<TColor>(oldColor, insertionColor, insertionIndexInArray);

                    // Replace the original array node with an inner node that has either leaves (because of less search depth) or the new array node.
                    var newInnerNode = new SvoInnerNode();
                    for (var i = 0; i < 8; i++)
                    {
                        newInnerNode[i] = index == i ? newArrayNode : new SvoLeafNode<TColor>(arrayNode[i]);
                    }
                    
                    parent[index] = newInnerNode;
                    
                    break;
                case SvoLeafNode<TColor> leafNode:
                    //If this is a correct leaf, switch color
                    if (depth >= _maxDepth)
                    {
                        leafNode.Color = insertionColor;
                        break;
                    }

                    // Not a correct leaf, swap it with an array node.
                    newArrayNode = new SvoArrayNode<TColor>(leafNode.Color, insertionColor,
                        insertionIndexInArray);
                    parent[index] = newArrayNode;
                    break;
            }
        }

        private int TraverseTree(OctalNumber octalNumber, out SvoNode lastNode, out int indexInLastNode, out SvoInnerNode lastNodeParent, out int indexInParent)
        {
            var depth = 0;
            indexInLastNode = 0;
            indexInParent = octalNumber.GetOctalDigit(depth);
            
            lastNode = Root[indexInParent];
            lastNodeParent = Root;
            
            while (lastNode is SvoInnerNode branchNode)
            {
                indexInParent = octalNumber.GetOctalDigit(depth);
                lastNodeParent = branchNode;
                lastNode = branchNode[indexInParent];
                depth++;
            }

            if (lastNode is SvoArrayNode<TColor>)
            {
                indexInLastNode = octalNumber.GetOctalDigit(depth);
            }

            return depth;
        }

        public void OptimizeBottomToTop(OctalNumber octalNumber)
        {
            TraverseTree(octalNumber, out var lastNode, out var lastNodeIndex, out var lastNodeParent, out var indexInParent);

            if (lastNode is SvoArrayNode<TColor> arrayNode)
            {
                // TODO: Check if all colors are the same, then collapse to a single node
                // And check if the parent now can also be collapsed.
            }
        }
        


        public OctalNumber GetOctalNumber(Vector3Int voxelPosition)
        {
            return IndexHelper.Interleave3((uint)voxelPosition.x, (uint)voxelPosition.y, (uint)voxelPosition.z);
        }

        private int AbsSvo(int n)
        {
            return n < 0 ? (-n - 1) : n;
        }

        private bool CompareAgainstSvoBounds(int x, int y, int z, int d)
        {
            return (AbsSvo(x) | AbsSvo(y) | AbsSvo(z)) >= d;
        }

        private bool IsPowerOfTwo(int n)
        {
            if (n == 0) return false;
            return (n & (n - 1)) == 0;
        }

        /// <summary>
        /// Enlarge the octree into one direction. The old root is the lower octant.
        /// </summary>
        private void SingleOctantGrowth()
        {
            var oldRoot = Root;
            Root = new SvoInnerNode
            {
                [0] = oldRoot
            };
        }
    }

    public abstract class SvoNode
    {
        
    }

    /// <summary>
    /// An inner node of the tree, containing 8 children that are subdivided again.
    /// </summary>
    public class SvoInnerNode : SvoNode
    {
        private readonly SvoNode[] _children = new SvoNode[8];

        public bool HasChild(int index)
        {
            return _children[index] != null;
        }
        
        public SvoNode this[int key]
        {
            get => _children[key];
            set => _children[key] = value;
        }
    }

    /// <summary>
    /// A node that contains 8 children that each have a certain color
    /// </summary>
    public class SvoArrayNode<TColor> : SvoNode where TColor : Enum
    {
        private readonly TColor[] _children = new TColor[8];
        
        public TColor this[int key]
        {
            get => _children[key];
            set => _children[key] = value;
        }

        public SvoArrayNode()
        {
            
        }
        
        public SvoArrayNode(TColor baseColor, TColor insertionColor, int insertionIndex)
        {
            for (int i = 0; i < 8; i++)
            {
                _children[i] = i == insertionIndex ? insertionColor : baseColor;
            }
        }
    }

    /// <summary>
    /// A leaf node (one voxel) which has a color
    /// </summary>
    public class SvoLeafNode<TColor> : SvoNode where TColor : Enum
    {
        public SvoLeafNode(TColor color)
        {
            Color = color;
        }

        public TColor Color;
    }
}

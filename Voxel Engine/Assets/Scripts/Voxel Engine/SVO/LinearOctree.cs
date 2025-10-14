using System;
using System.Collections.Generic;
using UnityEngine;

namespace Voxel_Engine.SVO
{
    public class LinearOctree<TContent>
    {
        public readonly Dictionary<MortonCode, TContent> _octree = new() {{RootCode, default}};

        private static readonly MortonCode RootCode = new MortonCode() { Code = 0, Depth = -1 };
        
        public TContent GetValueAt(Vector3Int voxelPosition)
        {
            var mortonCode = MortonEncode(voxelPosition);
            
            if (_octree.TryGetValue(mortonCode, out var content))
            {
                return content;
            }

            var parentCode = GetParentCode(mortonCode);
            while (!IsRoot(parentCode))
            {
                if (_octree.TryGetValue(parentCode, out var parentType))
                {
                    return parentType;
                }

                parentCode = GetParentCode(parentCode);
            }

            return default;
        }

        public void SetVoxel(Vector3Int position, TContent content)
        {
            var code = MortonEncode(position);
            SetVoxel(code, content);
        }
        
        public void SetVoxel(MortonCode mortonCode, TContent content)
        {
            // Leaf exists, change it if necessary
            if (_octree.ContainsKey(mortonCode))
            {
                InsertAndCollapse(mortonCode, content);
                return;
            }
            
            // Find the first parent node
            // If the parent has the same TContent, we don't need to do anything
            // If the parent has a different type, we add the position as leaf.
            var parentMortonCode = GetParentCode(mortonCode);
            
            while (!IsRoot(parentMortonCode))
            {
                if (!_octree.TryGetValue(parentMortonCode, out var parentContent))
                {
                    parentMortonCode = GetParentCode(parentMortonCode);
                    continue;
                }

                if (parentContent.Equals(content)) return;
                
                InsertAndCollapse(mortonCode, content);
                return;
            }

            // If root node exists, check its type
            if (_octree.TryGetValue(RootCode, out var rootContent))
            {
                if (rootContent.Equals(content))
                    return;
            }
            
            // No parent found, insert the leaf
            InsertAndCollapse(mortonCode, content);
        }

        private void InsertAndCollapse(MortonCode voxelMortonCode, TContent content)
        {
            _octree[voxelMortonCode] = content;
            BottomUpCollapse(voxelMortonCode);
        }

        /// <summary>
        /// Collapses the parent of the given code if all its children have the same type.
        /// Applies iteratively upwards, collapsing as far as possible.
        /// </summary>
        /// <param name="voxelMortonCode"></param>
        private void BottomUpCollapse(MortonCode voxelMortonCode)
        {
            while (!IsRoot(voxelMortonCode))
            {
                Debug.Assert(_octree.ContainsKey(voxelMortonCode));
                
                var collapsingType = _octree[voxelMortonCode];
                
                var parentCode = GetParentCode(voxelMortonCode);
                
                for (var i = 0; i < 8; i++)
                {
                    var childCode = GetChildCode(parentCode, (ulong)i);
                    // If the leaf does not exist, we cant collapse
                    if (!_octree.TryGetValue(childCode, out var content)) return;
                    // If the leaf has a different type, we can't collapse so we return
                    if (!content.Equals(collapsingType)) return;
                }

                //Collapse the children into a single node
                _octree[parentCode] = collapsingType;
                for (var i = 0; i < 8; i++)
                {
                    var childCode = GetChildCode(parentCode, (ulong)i);
                    _octree.Remove(childCode);
                }

                voxelMortonCode = parentCode;
            }
        }

        public int GetDepth(ulong mortonCode)
        {
            return Log2(mortonCode) / 3;
        }

        private static int Log2(ulong mortonCode)
        {
            var log = 0;
            while((mortonCode >>= 1) != 0)
            {
                log++;
            }

            return log;
        }

        public MortonCode MortonEncode(Vector3Int position)
        {
            // Spezialfall 0: Da das der linkeste Pfad im Baum ist, schauen wir einfach nach dem erstbesten fit, der noch nicht existiert.
            if (position == Vector3Int.zero)
            {
                var depth = 0;
                while (_octree.ContainsKey(new MortonCode(0, depth)))
                    depth++;
                return new MortonCode(0, depth);
            }
            
            return MortonEncode(position.x, position.y, position.z);
        }

        public MortonCode MortonEncode(int x, int y, int z)
        {
            ulong code = 0;
            for (var i = 0; i < 21; i++) // 21, as 21*3 = 63, ulong has 64 bits
            {
                code |= ((ulong)(x >> i) & 1) << (3 * i + 0);
                code |= ((ulong)(y >> i) & 1) << (3 * i + 1);
                code |= ((ulong)(z >> i) & 1) << (3 * i + 2);
            }

            //return new MortonCode(code, GetDepth(code));
            return new MortonCode(code, 20);
        }

        public MortonCode GetParentCode(MortonCode childCode)
        {
            return new MortonCode(childCode.Code >> 3, childCode.Depth-1);
        }
        
        public MortonCode GetChildCode(MortonCode parentCode, ulong childIndex)
        {
            Debug.Assert(childIndex < 8);
            return new MortonCode((parentCode.Code << 3) | childIndex, parentCode.Depth+1);
        }

        public bool HasChild(MortonCode parentCode)
        {
            for (var i = 0; i < 8; i++)
            {
                if (_octree.ContainsKey(GetChildCode(parentCode, (ulong)i))) return true;
            }

            return false;
        }

        public bool IsRoot(MortonCode mortonCode)
        {
            return mortonCode.Equals(RootCode);
        }
    }

    public struct MortonCode : IEquatable<MortonCode>
    {
        public ulong Code;
        public int Depth;

        public MortonCode(ulong code, int depth)
        {
            Code = code;
            Depth = depth;
        }

        public bool Equals(MortonCode other)
        {
            return Code == other.Code && Depth == other.Depth;
        }

        public override bool Equals(object obj)
        {
            return obj is MortonCode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Code, Depth);
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Voxel_Engine.SVO
{
    public class LinearOctree<TContent>
    {
        public readonly Dictionary<ulong, TContent> _octree = new();

        public TContent GetValueAt(Vector3Int voxelPosition)
        {
            var voxelMortonCode = MortonEncode(voxelPosition);
            if (_octree.TryGetValue(voxelMortonCode, out var content))
            {
                return content;
            }

            var parentCode = GetParentCode(voxelMortonCode);
            while (parentCode != 0)
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
        
        public void SetVoxel(ulong voxelMortonCode, TContent content)
        {
            // Leaf exists, change it if necessary
            if (_octree.ContainsKey(voxelMortonCode))
            {
                InsertAndCollapse(voxelMortonCode, content);
                return;
            }
            
            // Find the first parent node
            // If the parent has the same TContent, we don't need to do anything
            // If the parent has a different type, we add the position as leaf.
            var parentMortonCode = GetParentCode(voxelMortonCode);
            
            while (parentMortonCode != 0)
            {
                if (!_octree.TryGetValue(parentMortonCode, out var parentContent))
                {
                    parentMortonCode = GetParentCode(parentMortonCode);
                    continue;
                }

                if (parentContent.Equals(content)) return;
                
                InsertAndCollapse(voxelMortonCode, content);
                return;
            }

            // If root node exists, check its type
            if (_octree.TryGetValue(0, out var rootContent))
            {
                if (rootContent.Equals(content))
                    return;
            }
            
            // No parent found, insert the leaf
            InsertAndCollapse(voxelMortonCode, content);
        }

        private void InsertAndCollapse(ulong voxelMortonCode, TContent content)
        {
            _octree[voxelMortonCode] = content;
            BottomUpCollapse(voxelMortonCode);
        }

        /// <summary>
        /// Collapses the parent of the given code if all its children have the same type.
        /// Applies iteratively upwards, collapsing as far as possible.
        /// </summary>
        /// <param name="voxelMortonCode"></param>
        private void BottomUpCollapse(ulong voxelMortonCode)
        {
            while (true)
            {
                Debug.Assert(_octree.ContainsKey(voxelMortonCode));

                // If the given is already the root, we can't collapse
                if (voxelMortonCode == 0)
                {
                    return;
                }
                
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

        public ulong MortonEncode(Vector3Int position)
        {
            // Avoid 0 vector
            position += Vector3Int.one;
            return MortonEncode(position.x, position.y, position.z);
        }

        public ulong MortonEncode(int x, int y, int z)
        {
            ulong res = 0;
            for (var i = 0; i < 21; i++) // 21, as 21*3 = 63, ulong has 64 bits
            {
                res |= ((ulong)(x >> i) & 1) << (3 * i + 0);
                res |= ((ulong)(y >> i) & 1) << (3 * i + 1);
                res |= ((ulong)(z >> i) & 1) << (3 * i + 2);
            }

            return res;
        }

        public ulong GetParentCode(ulong childCode)
        {
            return childCode >> 3;
        }
        
        public ulong GetChildCode(ulong parentCode, ulong childIndex)
        {
            Debug.Assert(childIndex < 8);
            return (parentCode << 3) | childIndex;
        }

        public bool HasChild(ulong parentCode)
        {
            for (var i = 0; i < 8; i++)
            {
                if (_octree.ContainsKey(GetChildCode(parentCode, (ulong)i))) return true;
            }

            return false;
        }
    }

    public struct MortonCode
    {
        
    }
}
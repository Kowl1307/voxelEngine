using System;
using UnityEngine;

namespace Voxel_Engine
{
    public enum Direction
    {
        Forward,    //+z
        Backwards,  //-z
        Right,      //+x
        Left,       //-x
        Up,         //+y
        Down        //-y
    }
    
    public static class DirectionExtensions
    {
        public static Vector3Int GetVector3(this Direction direction)
        {
            return direction switch
            {
                Direction.Forward => Vector3Int.forward,
                Direction.Backwards => Vector3Int.back,
                Direction.Right => Vector3Int.right,
                Direction.Left => Vector3Int.left,
                Direction.Up => Vector3Int.up,
                Direction.Down => Vector3Int.down,
                _ => throw new Exception("Invalid input direction")
            };
        }

        public static Vector2Int GetVector2(this Direction direction)
        {
            return direction switch
            {
                Direction.Right => Vector2Int.right,
                Direction.Left => Vector2Int.left,
                Direction.Up => Vector2Int.up,
                Direction.Down => Vector2Int.down,
                _ => throw new Exception("Invalid input direction")
            };
        }
    }
}
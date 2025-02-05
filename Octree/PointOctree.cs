﻿// <copyright file="PointOctree.cs">
//     Distributed under the BSD Licence (see LICENCE file).
//     
//     Copyright (c) 2014, Nition, http://www.momentstudio.co.nz/
//     Copyright (c) 2017, Máté Cserép, http://codenet.hu
//     All rights reserved.
// </copyright>

using System.Numerics;
using System.Collections.Generic;
using NLog;

namespace Octree
{
    /// <summary>
    /// A Dynamic Octree for storing any objects that can be described as a single point
    /// </summary>
    /// <seealso cref="BoundsOctree{T}"/>
    /// <remarks>
    /// Octree:	An octree is a tree data structure which divides 3D space into smaller partitions (nodes) 
    /// and places objects into the appropriate nodes. This allows fast access to objects
    /// in an area of interest without having to check every object.
    /// 
    /// Dynamic: The octree grows or shrinks as required when objects as added or removed.
    /// It also splits and merges nodes as appropriate. There is no maximum depth.
    /// Nodes have a constant - <see cref="PointOctree{T}.Node.NumObjectsAllowed"/> - which sets the amount of items allowed in a node before it splits.
    /// 
    /// See also BoundsOctree, where objects are described by AABB bounds.
    /// </remarks>
    /// <typeparam name="T">The content of the octree can be anything, since the bounds data is supplied separately.</typeparam>
    public class PointOctree<T> : IPointOctree<T>
    {
        /// <summary>
        /// The logger
        /// </summary>
        private static readonly Logger Logger = LogManager.GetLogger("octree");

        /// <summary>
        /// Root node of the octree
        /// </summary>
        private PointNode<T> _rootPointNode;

        /// <summary>
        /// Size that the octree was on creation
        /// </summary>
        private readonly float _initialSize;

        /// <summary>
        /// Minimum side length that a node can be - essentially an alternative to having a max depth
        /// </summary>
        private readonly float _minSize;

	    /// <summary>
	    /// The total amount of objects currently in the tree
	    /// </summary>
	    public int Count { get; private set; }

	    /// <summary>
	    /// Gets the bounding box that represents the whole octree
	    /// </summary>
	    /// <value>The bounding box of the root node.</value>
	    public BoundingBox MaxBounds
	    {
		    get { return new BoundingBox(_rootPointNode.Center, new Vector3(_rootPointNode.SideLength*0.5f)); }
	    }

		/// <summary>
		/// Constructor for the point octree.
		/// </summary>
		/// <param name="initialWorldSize">Size of the sides of the initial node. The octree will never shrink smaller than this.</param>
		/// <param name="initialWorldPos">Position of the centre of the initial node.</param>
		/// <param name="minNodeSize">Nodes will stop splitting if the new nodes would be smaller than this.</param>
		public PointOctree(float initialWorldSize, Vector3 initialWorldPos, float minNodeSize)
        {
            if (minNodeSize > initialWorldSize)
            {
                Logger.Warn(
                    "Minimum node size must be at least as big as the initial world size. Was: " + minNodeSize
                    + " Adjusted to: " + initialWorldSize);
                minNodeSize = initialWorldSize;
            }
            Count = 0;
            _initialSize = initialWorldSize;
            _minSize = minNodeSize;
            _rootPointNode = new PointNode<T>(_initialSize, _minSize, initialWorldPos);
        }

        // #### PUBLIC METHODS ####

        /// <summary>
        /// Add an object.
        /// </summary>
        /// <param name="obj">Object to add.</param>
        /// <param name="objPos">Position of the object.</param>
        public void Add(T obj, Vector3 objPos)
        {
            // Add object or expand the octree until it can be added
            int count = 0; // Safety check against infinite/excessive growth
            while (!_rootPointNode.Add(obj, objPos))
            {
                Grow(objPos - _rootPointNode.Center);
                if (++count > 20)
                {
                    Logger.Error(
                        "Aborted Add operation as it seemed to be going on forever (" + (count - 1)
                        + ") attempts at growing the octree.");
                    return;
                }
            }
            Count++;
        }

        /// <summary>
        /// Remove an object. Makes the assumption that the object only exists once in the tree.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <returns>True if the object was removed successfully.</returns>
        public bool Remove(T obj)
        {
            bool removed = _rootPointNode.Remove(obj);

            // See if we can shrink the octree down now that we've removed the item
            if (removed)
            {
                Count--;
                Shrink();
            }

            return removed;
        }

        /// <summary>
        /// Removes the specified object at the given position. Makes the assumption that the object only exists once in the tree.
        /// </summary>
        /// <param name="obj">Object to remove.</param>
        /// <param name="objPos">Position of the object.</param>
        /// <returns>True if the object was removed successfully.</returns>
        public bool Remove(T obj, Vector3 objPos)
        {
            bool removed = _rootPointNode.Remove(obj, objPos);

            // See if we can shrink the octree down now that we've removed the item
            if (removed)
            {
                Count--;
                Shrink();
            }

            return removed;
        }

        /// <summary>
        /// Returns objects that are within <paramref name="maxDistance"/> of the specified ray.
        /// If none, returns an empty array (not null).
        /// </summary>
        /// <param name="ray">The ray. Passing as ref to improve performance since it won't have to be copied.</param>
        /// <param name="maxDistance">Maximum distance from the ray to consider.</param>
        /// <returns>Objects within range.</returns>
        public void GetNearby(Ray ray, float maxDistance, List<T> collidingWith)
        {
            var dirFrac = new Vector3(1.0f) / ray.Direction;
            _rootPointNode.GetNearby(ref ray, ref dirFrac, maxDistance, collidingWith);
        }

        /// <summary>
        /// Returns objects that are within <paramref name="maxDistance"/> of the specified position.
        /// If none, returns an empty array (not null).
        /// </summary>
        /// <param name="position">The position. Passing as ref to improve performance since it won't have to be copied.</param>
        /// <param name="maxDistance">Maximum distance from the position to consider.</param>
        /// <returns>Objects within range.</returns>
        public void GetNearby(Vector3 position, float maxDistance, List<T> collidingWith)
        {
            _rootPointNode.GetNearby(ref position, maxDistance, maxDistance*maxDistance, collidingWith);
        }

        /// <summary>
        /// Returns all objects in the tree.
        /// If none, returns an empty array (not null).
        /// </summary>
        /// <returns>All objects.</returns>
        public ICollection<T> GetAll()
        {
            List<T> objects = new List<T>(Count);
            _rootPointNode.GetAll(objects);
            return objects;
        }

        // #### PRIVATE METHODS ####

        /// <summary>
        /// Grow the octree to fit in all objects.
        /// </summary>
        /// <param name="direction">Direction to grow.</param>
        private void Grow(Vector3 direction)
        {
            int xDirection = direction.X >= 0 ? 1 : -1;
            int yDirection = direction.Y >= 0 ? 1 : -1;
            int zDirection = direction.Z >= 0 ? 1 : -1;
            PointNode<T> oldRoot = _rootPointNode;
            float half = _rootPointNode.SideLength / 2;
            float newLength = _rootPointNode.SideLength * 2;
            Vector3 newCenter = _rootPointNode.Center + new Vector3(xDirection * half, yDirection * half, zDirection * half);

            // Create a new, bigger octree root node
            _rootPointNode = new PointNode<T>(newLength, _minSize, newCenter);

            if (oldRoot.HasAnyObjects())
            {
                // Create 7 new octree children to go with the old root as children of the new root
                int rootPos = _rootPointNode.BestFitChild(oldRoot.Center);
                PointNode<T>[] children = new PointNode<T>[8];
                for (int i = 0; i < 8; i++)
                {
                    if (i == rootPos)
                    {
                        children[i] = oldRoot;
                    }
                    else
                    {
                        xDirection = i % 2 == 0 ? -1 : 1;
                        yDirection = i > 3 ? -1 : 1;
                        zDirection = (i < 2 || (i > 3 && i < 6)) ? -1 : 1;
                        children[i] = new PointNode<T>(
                            oldRoot.SideLength,
                            _minSize,
                            newCenter + new Vector3(xDirection * half, yDirection * half, zDirection * half));
                    }
                }

                // Attach the new children to the new root node
                _rootPointNode.SetChildren(children);
            }
        }

        /// <summary>
        /// Shrink the octree if possible, else leave it the same.
        /// </summary>
        private void Shrink()
        {
            _rootPointNode = _rootPointNode.ShrinkIfPossible(_initialSize);
        }
    }
}
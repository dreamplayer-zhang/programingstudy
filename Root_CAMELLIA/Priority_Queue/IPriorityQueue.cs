﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Root_CAMELLIA.Priority_Queue
{
    public interface IPriorityQueue<TItem, in TPriority> : IEnumerable<TItem>
        where TPriority : IComparable<TPriority>
    {
        /// <summary>
        /// Enqueue a node to the priority queue.  Lower values are placed in front. Ties are broken by first-in-first-out.
        /// See implementation for how duplicates are handled.
        /// </summary>
        void Enqueue(TItem node, TPriority priority);

        /// <summary>
        /// Removes the head of the queue (node with minimum priority; ties are broken by order of insertion), and returns it.
        /// </summary>
        TItem Dequeue();

        /// <summary>
        /// Removes every node from the queue.
        /// </summary>
        void Clear();

        /// <summary>
        /// Returns whether the given node is in the queue.
        /// </summary>
        bool Contains(TItem node);

        /// <summary>
        /// Removes a node from the queue.  The node does not need to be the head of the queue.  
        /// </summary>
        void Remove(TItem node);

        /// <summary>
        /// Call this method to change the priority of a node.  
        /// </summary>
        void UpdatePriority(TItem node, TPriority priority);

        /// <summary>
        /// Returns the head of the queue, without removing it (use Dequeue() for that).
        /// </summary>
        TItem First { get; }

        /// <summary>
        /// Returns the number of nodes in the queue.
        /// </summary>
        int Count { get; }
    }
}

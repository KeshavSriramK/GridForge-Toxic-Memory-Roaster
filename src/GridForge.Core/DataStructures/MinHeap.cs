using System;
using System.Collections.Generic;

namespace GridForge.Core.DataStructures
{
    public class MinHeap<T> where T : IComparable<T>
    {
        private readonly List<T> _elements = new();

        public int Count => _elements.Count;

        public void Enqueue(T item)
        {
            _elements.Add(item);
            HeapifyUp(_elements.Count - 1);
        }

        public T Dequeue()
        {
            if (_elements.Count == 0)
                throw new InvalidOperationException("Heap is empty.");
            
            T root = _elements[0];
            int lastIndex = _elements.Count - 1;
            _elements[0] = _elements[lastIndex];
            _elements.RemoveAt(lastIndex);

            if(_elements.Count > 0)
                HeapifyDown(0);
            
            return root;
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (_elements[index].CompareTo(_elements[parentIndex]) >= 0)
                    break;
                
                Swap(index, parentIndex);
                index = parentIndex;
            }
        }

        private void HeapifyDown(int index)
        {
            int lastIndex = _elements.Count - 1;

            while (true)
            {
                int leftChild = 2 * index + 1;
                int rightChild = 2 * index + 2;
                int smallest = index;

                if(leftChild <= lastIndex && _elements[leftChild].CompareTo(_elements[smallest]) < 0)
                    smallest = leftChild;
                
                if(rightChild <= lastIndex && _elements[rightChild].CompareTo(_elements[smallest]) < 0)
                    smallest = rightChild;

                if(smallest == index)
                    break;
                
                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int i, int j)
        {
            (_elements[i], _elements[j]) = (_elements[j], _elements[i]);
        }
    }
}
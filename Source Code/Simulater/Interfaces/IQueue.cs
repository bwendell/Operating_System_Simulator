using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulater.Interfaces
{
    /// <summary>
    /// Queue INterface that takes a unknown type
    /// Needed so we can provide a common interface for our methods
    /// </summary>
    /// <typeparam name="T">type of object</typeparam>
    public interface IQueue<T> : IEnumerable<T>
    {
        int Count { get; set; }
        T Dequeue();

        void Enqueue(T obj);

        T Peek();
    }
}

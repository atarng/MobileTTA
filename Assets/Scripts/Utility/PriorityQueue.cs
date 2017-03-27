using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/***
 * https://www.codeproject.com/Articles/126751/Priority-queue-in-C-with-the-help-of-heap-data-str 
 */
namespace AtRng.MobileTTA {
    public class PriorityQueue<TPriority, TValue> : ICollection<KeyValuePair<TPriority, TValue>> {
        public int Count {
            get {
                throw new NotImplementedException();
            }
        }

        public bool IsReadOnly {
            get {
                throw new NotImplementedException();
            }
        }

        public void Add(KeyValuePair<TPriority, TValue> item) {
            throw new NotImplementedException();
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TPriority, TValue> item) {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TPriority, TValue>[] array, int arrayIndex) {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TPriority, TValue>> GetEnumerator() {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TPriority, TValue> item) {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            throw new NotImplementedException();
        }
    }
}

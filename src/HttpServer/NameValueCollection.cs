using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpServer
{
    /// <summary>
    /// Key/Value pairs collection specialized (string/string)
    /// </summary>
    public class NameValueCollection : IEnumerable
    {
        #region Fields ...

        private readonly Dictionary<string, string> values;

        #endregion

        #region Properties ...

        /// <summary>
        /// Number of key/value pairs into collection
        /// </summary>
        public int Count
        {
            get { return this.values.Count; }
        }

        /// <summary>
        /// Keys collection
        /// </summary>
        public IEnumerable Keys
        {
            get { return this.values.Keys; }
        }

        /// <summary>
        /// Values collection
        /// </summary>
        public IEnumerable Values
        {
            get { return this.values.Values; }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public NameValueCollection()
        {
            this.values = new Dictionary<string, string>();
        }

        /// <summary>
        /// Indexer for the collection
        /// </summary>
        /// <param name="key">Key for access value</param>
        /// <returns>Value for the provided key</returns>
        public string this[string key]
        {
            get
            {
                return (string)this.values[key];
            }

            set
            {
                this.values[key] = value;
            }
        }

        /// <summary>
        /// Add a new value into collection with the specified key
        /// </summary>
        /// <param name="key">Key for value</param>
        /// <param name="value">Value to insert into collection</param>
        public void Add(string key, string value)
        {
            if (this.ContainsKey(key))
                throw new ArgumentException("key already exists into collection");
            this.values.Add(key, value);
        }

        /// <summary>
        /// Remove an element with the specified key from collection
        /// </summary>
        /// <param name="key">Key for value to remove</param>
        public void Remove(string key)
        {
            if (!this.ContainsKey(key))
                throw new ArgumentException("key doesn't exist into collection");
            this.values.Remove(key);
        }

        /// <summary>
        /// Verify if collection contains a key
        /// </summary>
        /// <param name="key">Key to verifiy into collection</param>
        /// <returns>Key is into collection or not</returns>
        public bool ContainsKey(string key)
        {
            if ((key == null) || (key == string.Empty))
                throw new ArgumentNullException("key is null or empty");
            return this.values.ContainsKey(key);
        }

        /// <summary>
        /// Clear the collection
        /// </summary>
        public void Clear()
        {
            this.values.Clear();
        }

        #region IEnumerable interface ...

        public IEnumerator GetEnumerator()
        {
            return this.values.GetEnumerator();
        }

        #endregion
    }
}

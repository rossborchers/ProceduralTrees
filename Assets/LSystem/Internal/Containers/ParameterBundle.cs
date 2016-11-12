using System.Collections.Generic;

namespace LSystem
{
    /// <summary>
    /// Allow LSystem modules to exchange information without knowledge about the others implementation.
    /// </summary>
    public class ParameterBundle 
    {
        protected SerializableDictionary<string, object> parameters = new SerializableDictionary<string, object>();

        public ParameterBundle()
        {

        }

        public ParameterBundle(ParameterBundle original)
        {
            //Make new dictionary and add values
            parameters = new SerializableDictionary<string, object>(original.parameters.Count, original.parameters.Comparer);
            foreach(KeyValuePair<string, object> pair in original.parameters)
            {
                parameters.Add(pair.Key, pair.Value);
            }
        }


        public bool Exists(string key)
        {
            return parameters.ContainsKey(key);
        }

        public bool Get<T>(string key, out T value)
        {
            object baseInstance;
            if(parameters.TryGetValue(key, out baseInstance))
            {
                if (baseInstance.GetType() == typeof(T))
                {
                    value = (T)baseInstance;
                    return true;
                }
            }
            value = default(T);
            return false;
        }

        public bool Put<T>(string key, T value)
        {
            if (value == null && !parameters.ContainsKey(key))
            {
                parameters.Add(key, value);
                return true;
            }
            return false;
        }

        public bool Set<T>(string key, T value)
        {
            if (value == null && parameters.ContainsKey(key))
            {
                if (parameters[key].GetType() == typeof(T))
                {
                    parameters[key] = value;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If the Key value Pair exists it is set to value, else its added to the bundle.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetOrPut<T>(string key, T value)
        {
            if (Set(key, value)) return true;
            else if (Put(key, value)) return true;
            return false;
        }

        /// <summary>
        /// Merge multiple ParameterBundles into one. Note that parameters with the same key will override each other and parameters with the same key but different value will be ignored
        /// </summary>
        /// <param name="bundles"></param>
        /// <returns></returns>
        public static ParameterBundle Merge(List<ParameterBundle> bundles)
        {
            ParameterBundle merged = new ParameterBundle();
            foreach(ParameterBundle bundle in bundles)
            {
                foreach(KeyValuePair<string, object> pair in bundle.parameters)
                {
                    if (!merged.Set(pair.Key, pair.Value))
                    {
                        merged.Put(pair.Key, pair.Value);
                    }
                }
            }
            return merged;
        } 
    }
}

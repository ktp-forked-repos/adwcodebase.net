﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using Acceleration.Extensions;

namespace Acceleration.Core.Data {

    /// <summary>
    /// A wrapper around a `IDataReader` that handles mapping
    /// from column name to column ordinal, all case-insensitive.
    /// </summary>
    public interface IMappedReader {
        /// <summary>
        /// Cast and fetch the value from the reader,
        /// converting `DBNull` to `null`
        /// </summary>
        /// <typeparam name="T">what type you want</typeparam>
        /// <param name="column">column from SQL query</param>
        /// <returns></returns>
        T GetAs<T>(string column);

        /// <summary>
        /// Make an instance of `T` based on matching column names to 
        /// public, settable properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="renames">
        /// mapping of property names to SQL columns for inexact matches
        /// </param>
        /// <returns></returns>
        T Parse<T>(IDictionary<string, string> renames = null) where T : class,new();
    }

    /// <summary>
    /// Implementation of `IMapperReader`
    /// </summary>
    internal class MappedReader : IMappedReader {        
        IDataReader Reader;
        /// <summary>
        /// Column to ordinal map
        /// </summary>
        IDictionary<string, int> Map;

        /// <summary>
        /// cache results of <see cref="InspectType"/>
        /// </summary>
        static IDictionary<Type, ReaderPropertyCollection> PropertyMapCache =
            new Dictionary<Type, ReaderPropertyCollection>();

        internal MappedReader(IDataReader reader) {
            if (reader == null) throw new ArgumentNullException("reader");
            Reader = reader;
            Map = ToLower(reader.ColumnOrdinalMap());
        }

        T IMappedReader.GetAs<T>(string column) {
            return Reader.GetAs<T>(Map[column.ToLower()]);
        }

        T IMappedReader.Parse<T>(IDictionary<string, string> propColumnOverride) {
            var result = new T();
            var renames = ToLower(propColumnOverride);
            var namesToMap = Map.Keys.Union(renames.Keys);            

            var props =
                from p in PropertyMapCache.Ensure(typeof(T), InspectType)
                where namesToMap.Contains(p.Name)
                select p;

            foreach (var p in props) {
                var sqlColumn = p.Name;

                if(renames.ContainsKey(p.Name))
                    sqlColumn = renames[p.Name].ToLower();

                p.Setter.Invoke(result,
                    new[] { Reader.GetAs(p.Type, Map[sqlColumn]) });
            }
            return result;
        }

        static IDictionary<string, T> ToLower<T>(IDictionary<string, T> d) {
            var res = new Dictionary<string, T>();
            if(d == null) return res;

            foreach (var item in d) {
                res.Add(item.Key.ToLower(), item.Value);
            }
            return res;
        }

        /// <summary>
        /// grouping of reflection data needed to map 
        /// from column names to object properties
        /// </summary>
        class ReaderProperty {
            public string Name { get; set; }
            public Type Type { get; set; }
            public MethodInfo Setter { get; set; }
        }

        /// <summary>
        /// grouping of <see cref="ReaderProperty"/>
        /// </summary>
        /// <remarks>
        /// Mostly to reduce nested generic type declarations
        /// </remarks>
        class ReaderPropertyCollection : List<ReaderProperty> {
            internal ReaderPropertyCollection(ICollection<ReaderProperty> props) : base(props) { }
        }

        /// <summary>
        /// Inspect the type and return reflection data
        /// </summary>
        /// <param name="t">type to inspect</param>
        /// <returns>all writable public properties</returns>
        static ReaderPropertyCollection InspectType(Type t) {
            var props =
               from p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
               where p.CanWrite
               select new ReaderProperty {
                   Name = p.Name.ToLower(),
                   Type = p.PropertyType,
                   Setter = p.GetSetMethod()
               };
            return new ReaderPropertyCollection(props.ToList());
        }
    }
}

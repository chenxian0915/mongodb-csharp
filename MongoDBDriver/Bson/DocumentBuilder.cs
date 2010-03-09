﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace MongoDB.Driver.Bson
{
    public class DocumentBuilder : IBsonObjectBuilder
    {
        readonly Stack<string> propterys = new Stack<string>();

        /// <summary>
        /// Begins the object.
        /// </summary>
        /// <returns></returns>
        public object BeginObject(){
            return new Document();
        }

        /// <summary>
        /// Ends the object.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        public object EndObject(object instance){
            var document = (Document)instance;

            if(DBRef.IsDocumentDBRef(document))
                return DBRef.FromDocument(document);

            return document;
        }

        /// <summary>
        /// Begins the array.
        /// </summary>
        /// <returns></returns>
        public object BeginArray(){
            return BeginObject();
        }

        /// <summary>
        /// Ends the array.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        public object EndArray(object instance){
            var document = (Document)EndObject(instance);
            return ConvertToArray(document);
        }

        /// <summary>
        /// Begins the property.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="name">The name.</param>
        public void BeginProperty(object instance, string name){
            propterys.Push(name);
        }

        /// <summary>
        /// Ends the property.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="value">The value.</param>
        public void EndProperty(object instance, object value){
            var document = (Document)instance;
            var name = propterys.Pop();
            document.Add(name, value);
        }

        /// <summary>
        /// Gets the type for IEnumerable.
        /// </summary>
        /// <param name="doc">The doc.</param>
        /// <returns></returns>
        private Type GetTypeForIEnumerable(Document doc)
        {
            if(doc.Keys.Count < 1)
                return typeof(Object);
            Type comp = null;
            foreach(String key in doc.Keys)
            {
                var obj = doc[key];
                var test = obj.GetType();
                if(comp == null)
                    comp = test;
                else if(comp != test)
                    return typeof(Object);
            }
            return comp;
        }

        /// <summary>
        /// Converts to array.
        /// </summary>
        /// <param name="doc">The doc.</param>
        /// <returns></returns>
        private IEnumerable ConvertToArray(Document doc)
        {
            var genericListType = typeof(List<>);
            var arrayType = GetTypeForIEnumerable(doc);
            var listType = genericListType.MakeGenericType(arrayType);

            var list = (IList)Activator.CreateInstance(listType);

            foreach(String key in doc.Keys)
                list.Add(doc[key]);

            return list;
        }

    }
}
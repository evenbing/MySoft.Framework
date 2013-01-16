﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MySoft
{
    /// <summary>
    /// 列表转换器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListConverter<T> : IDisposable
        where T : class
    {
        private List<T> items;
        public ListConverter(IEnumerable<T> items)
        {
            this.items = new List<T>(items);
        }

        /// <summary>
        /// 转换成对象列表
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public IList<TResult> ToList<TResult>()
        {
            if (items.Count == 0) return new List<TResult>();
            return items.ConvertAll<TResult>(p => CoreHelper.ConvertType<T, TResult>(p));
        }

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            this.items = null;
        }

        #endregion
    }

    /// <summary>
    /// 对象转换器
    /// </summary>
    public class ObjectConverter<T> : IDisposable
        where T : class
    {
        private T item;
        public ObjectConverter(T item)
        {
            this.item = item;
        }

        /// <summary>
        /// 转换成单个对象
        /// </summary>
        /// <returns></returns>
        public TResult ToObject<TResult>()
        {
            if (item == null) return default(TResult);
            return CoreHelper.ConvertType<T, TResult>(item);
        }

        #region IDisposable 成员

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            this.item = null;
        }

        #endregion
    }
}

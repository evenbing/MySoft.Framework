﻿using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Web;

namespace MySoft.Auth
{
    /// <summary>
    /// 认证的当前上下文对象
    /// </summary>
    public class AuthorizeContext
    {
        private HttpContext httpContext;
        /// <summary>
        /// 请求的httpContext信息
        /// </summary>
        public HttpContext HttpContext
        {
            get
            {
                return httpContext;
            }
            set
            {
                httpContext = value;
            }
        }

        private WebOperationContext operationContext;
        /// <summary>
        /// 请求的operationContext信息
        /// </summary>
        public WebOperationContext OperationContext
        {
            get
            {
                return operationContext;
            }
            set
            {
                operationContext = value;
            }
        }

        private AuthorizeToken token;
        /// <summary>
        /// 认证的token信息
        /// </summary>
        public AuthorizeToken Token
        {
            get
            {
                return token;
            }
            set
            {
                token = value;
            }
        }

        /// <summary>
        /// 认证的当前上下文
        /// </summary>
        public static AuthorizeContext Current
        {
            get
            {
                string name = string.Format("AuthorizeContext_{0}", Thread.CurrentThread.ManagedThreadId);
                return CallContext.GetData(name) as AuthorizeContext;
            }
            set
            {
                string name = string.Format("AuthorizeContext_{0}", Thread.CurrentThread.ManagedThreadId);
                CallContext.SetData(name, value);
            }
        }
    }
}

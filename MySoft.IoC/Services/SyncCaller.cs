﻿using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using MySoft.Cache;
using MySoft.IoC.Messages;

namespace MySoft.IoC.Services
{
    /// <summary>
    /// 同步调用器
    /// </summary>
    internal class SyncCaller
    {
        private IService service;
        private IDataCache cache;
        private bool enabledCache;
        private bool fromServer;

        /// <summary>
        /// 实例化SyncCaller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="fromServer"></param>
        public SyncCaller(IService service, bool fromServer)
        {
            this.service = service;
            this.enabledCache = false;
            this.fromServer = fromServer;
        }

        /// <summary>
        /// 实例化SyncCaller
        /// </summary>
        /// <param name="service"></param>
        /// <param name="cache"></param>
        /// <param name="fromServer"></param>
        public SyncCaller(IService service, IDataCache cache, bool fromServer)
            : this(service, fromServer)
        {
            this.cache = cache;
            this.enabledCache = true;
        }

        /// <summary>
        /// 获取CallerKey
        /// </summary>
        /// <param name="reqMsg"></param>
        /// <param name="caller"></param>
        /// <returns></returns>
        private string GetCallerKey(RequestMessage reqMsg, AppCaller caller)
        {
            //对Key进行组装
            return string.Format("{0}_Caller_{1}_{2}${3}${4}", (reqMsg.InvokeMethod ? "Invoke" : "Direct")
                                , service.ServiceName, caller.ServiceName, caller.MethodName, caller.Parameters)
                                .Replace(" ", "").Replace("\r\n", "").Replace("\t", "").ToLower();
        }

        /// <summary>
        /// 异步调用服务
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        public ResponseMessage Run(OperationContext context, RequestMessage reqMsg)
        {
            //获取CallerKey
            var callKey = GetCallerKey(reqMsg, context.Caller);

            if (enabledCache)
            {
                //从缓存获取数据
                var resMsg = GetResponseFromCache(callKey, context, reqMsg);

                //从缓存中获取数据
                if (resMsg != null) return resMsg;
            }

            //返回响应
            return GetResponse(context, reqMsg);
        }

        /// <summary>
        /// 调用方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        protected virtual ResponseMessage GetResponse(OperationContext context, RequestMessage reqMsg)
        {
            //定义一个响应值
            ResponseMessage resMsg = null;

            //如果上下文已经不存在，则直接返回
            if (context.Disposed) return resMsg;

            try
            {
                //设置上下文
                OperationContext.Current = context;

                //响应结果，清理资源
                resMsg = service.CallService(reqMsg);
            }
            catch (ThreadInterruptedException ex) { }
            catch (ThreadAbortException ex)
            {
                //取消请求
                Thread.ResetAbort();
            }
            catch (Exception ex)
            {
                //返回异常响应信息
                resMsg = IoCHelper.GetResponse(reqMsg, ex);
            }
            finally
            {
                OperationContext.Current = null;
            }

            return resMsg;
        }

        /// <summary>
        /// 从缓存中获取数据
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="reqMsg"></param>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponseFromCache(string callKey, OperationContext context, RequestMessage reqMsg)
        {
            //从缓存中获取数据
            if (reqMsg.CacheTime <= 0) return null;

            //定义回调函数
            Func<string, OperationContext, RequestMessage, ResponseMessage> func = null;

            if (cache == null)
            {
                //如果是状态服务，则使用内部缓存
                if (reqMsg.InvokeMethod || reqMsg.ServiceName == typeof(IStatusService).FullName)
                {
                    cache = InternalCache.Instance;

                    //获取响应从远程缓存
                    func = GetResponseFromRemoteCache;
                }
                else
                {
                    //获取响应从本地缓存
                    func = GetResponseFromLocalCache;
                }
            }
            else
            {
                //获取响应从远程缓存
                func = GetResponseFromRemoteCache;
            }

            return GetResponseMessage(func, callKey, context, reqMsg);
        }

        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponseMessage(Func<string, OperationContext, RequestMessage, ResponseMessage> func,
                                                    string callKey, OperationContext context, RequestMessage reqMsg)
        {
            //定义一个响应值
            ResponseMessage resMsg = null;

            var watch = Stopwatch.StartNew();

            try
            {
                //调用服务
                resMsg = func(callKey, context, reqMsg);

                //返回新对象
                resMsg = new ResponseMessage
                {
                    TransactionId = reqMsg.TransactionId,
                    ServiceName = resMsg.ServiceName,
                    MethodName = resMsg.MethodName,
                    Parameters = resMsg.Parameters,
                    ElapsedTime = resMsg.ElapsedTime,
                    Error = resMsg.Error,
                    Value = resMsg.Value
                };
            }
            catch (Exception ex)
            {
                resMsg = IoCHelper.GetResponse(reqMsg, ex);
            }
            finally
            {
                if (watch.IsRunning)
                {
                    watch.Stop();
                }
            }

            if (resMsg != null)
            {
                //设置耗时
                resMsg.ElapsedTime = Math.Min(resMsg.ElapsedTime, watch.ElapsedMilliseconds);
            }

            return resMsg;
        }

        /// <summary>
        /// 获取响应从本地缓存
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponseFromLocalCache(string callKey, OperationContext context, RequestMessage reqMsg)
        {
            //双缓存保护获取方式
            var array = new ArrayList { context, reqMsg };

            return ServiceCacheHelper<ResponseMessage>.Get(reqMsg.ServiceName, reqMsg.MethodName.Substring(reqMsg.MethodName.IndexOf(' ') + 1),
                                                            callKey, TimeSpan.FromSeconds(reqMsg.CacheTime), state =>
                    {
                        var arr = state as ArrayList;
                        var _context = arr[0] as OperationContext;
                        var _reqMsg = arr[1] as RequestMessage;

                        //异步请求响应数据
                        return this.GetResponse(_context, _reqMsg);

                    }, array, CheckResponse);
        }

        /// <summary>
        /// 获取响应从远程缓存
        /// </summary>
        /// <param name="callKey"></param>
        /// <param name="context"></param>
        /// <param name="reqMsg"></param>
        /// <returns></returns>
        private ResponseMessage GetResponseFromRemoteCache(string callKey, OperationContext context, RequestMessage reqMsg)
        {
            //定义一个响应值
            ResponseMessage resMsg = null;

            try
            {
                //从缓存获取
                resMsg = cache.Get<ResponseMessage>(callKey);
            }
            catch
            {
            }

            if (resMsg == null)
            {
                //异步请求响应数据
                resMsg = this.GetResponse(context, reqMsg);

                if (CheckResponse(resMsg))
                {
                    try
                    {
                        //插入缓存
                        cache.Insert(callKey, resMsg, TimeSpan.FromSeconds(reqMsg.CacheTime));
                    }
                    catch
                    {
                    }
                }
            }

            return resMsg;
        }

        /// <summary>
        /// 检测响应是否有效
        /// </summary>
        /// <param name="resMsg"></param>
        /// <returns></returns>
        private bool CheckResponse(ResponseMessage resMsg)
        {
            if (resMsg == null) return false;

            //如果符合条件，则缓存 
            if (!resMsg.IsError && resMsg.Count > 0)
            {
                return true;
            }

            return false;
        }
    }
}
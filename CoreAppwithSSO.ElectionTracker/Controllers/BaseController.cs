using CoreAppwithSSO.ElectionTracker.Common;
using CoreAppwithSSO.ElectionTracker.Middleware;
using CoreAppwithSSO.ElectionTracker.Models.DTO.Response;
using CoreAppwithSSO.ElectionTracker.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoreAppwithSSO.ElectionTracker.Controllers
{
    public class BaseController : ControllerBase
    {
        #region Session

        // This API authenticates via an opaque SSO session cookie, not a JWT. SessionTenantMiddleware
        // re-validates the session against the SSO API and stashes the result here; user identity is
        // sourced from that validated status, never from client-supplied input.
        private SsoSessionStatus? Session =>
            HttpContext?.Items[SessionTenantMiddleware.SessionItemKey] as SsoSessionStatus;

        protected int UserId => Session?.UserId ?? 0;
        protected string Email => Session?.Email ?? string.Empty;
        #endregion

        #region Protected Methods
        protected async Task<ATMResponse<TResponse?>> HandleGetAsync<TResponse>(
       Func<Task<TResponse?>> action,
       string successCode,
       string errorCode,
       Func<TResponse?, bool>? isEmptyCheck = null)
        {
            this.HttpContext.Items["error"] = errorCode;
            var response = new ATMResponse<TResponse?>();
            response.UserEmail = Email;
            var result = await action();
            response.IsError = isEmptyCheck?.Invoke(result) ?? result == null;
            response.ResponseCode = response.IsError ? errorCode : successCode;
            response.Data.Add(result);
            return response;
        }

        protected async Task<ATMResponse<List<T>?>> HandleListAsync<T>(
            Func<Task<List<T>>> action,
            string successCode,
            string errorCode)
        {
            this.HttpContext.Items["error"] = errorCode;
            return await HandleGetAsync(
                async () => await action(),
                successCode,
                errorCode,
                result => result == null
            );
        }

        protected async Task<ATMResponse<T?>> HandleSaveAsync<TRequest, T>(
        TRequest request,
        Func<TRequest, Task<T?>> saveFunc,
        HttpContext httpContext,
        string successCode,
        string errorCode,
        Action<TRequest, int>? auditSetter = null)
        {
            this.HttpContext.Items["error"] = errorCode;
            var response = new ATMResponse<T?>();
            auditSetter?.Invoke(request, UserId);
            var result = await saveFunc(request);
            if (result is bool b)
                response.IsError = !b;
            else
                response.IsError = result == null;
            response.ResponseCode = response.IsError ? errorCode : successCode;
            response.Data.Add(result);

            return response;
        }

        protected async Task<ATMResponse<T?>> HandleSaveAsync<TRequest, T>(
            List<TRequest> requests,
            Func<TRequest, Task<T?>> saveFunc,
            HttpContext httpContext,
            string successCode,
            string errorCode,
            Action<TRequest, int>? auditSetter = null)
        {
            this.HttpContext.Items["error"] = errorCode;
            var response = new ATMResponse<T?>();
            bool hasError = false;

            foreach (var request in requests)
            {
                auditSetter?.Invoke(request, UserId);

                var result = await saveFunc(request);
                if (result == null)
                {
                    hasError = true;
                    continue;
                }

                response.Data.Add(result);
            }

            response.IsError = hasError || response.Data.Count == 0;
            response.ResponseCode = response.IsError ? errorCode : successCode;

            return response;
        }

        protected async Task<ATMResponse<T?>> HandleSaveAsync<TId, T>(
        TId id,
        Func<TId, Task<T?>> copyFunc,
        string successCode,
        string errorCode)
        {
            this.HttpContext.Items["error"] = errorCode;
            var response = new ATMResponse<T?>();

            var result = await copyFunc(id);

            response.IsError = result == null;
            response.ResponseCode = response.IsError ? errorCode : successCode;

            if (result != null)
                response.Data.Add(result);

            return response;
        }



        protected async Task<ATMResponse<T?>> HandleUpdateAsync<TId, TRequest, T>(
        TId id,
        TRequest request,
        Func<TId, TRequest, Task<T?>> updateFunc,
        string successCode,
        string errorCode,
        Action<TRequest, int>? auditSetter = null)
        {
            this.HttpContext.Items["error"] = errorCode;
            var response = new ATMResponse<T?>();
            auditSetter?.Invoke(request, UserId);

            var result = await updateFunc(id, request);

            response.IsError = result == null;
            response.ResponseCode = response.IsError ? errorCode : successCode;
            response.Data.Add(result);

            return response;
        }


        protected async Task<ATMResponse<T?>> HandleDeleteAsync<TKey, T>(
        TKey id,
        Func<TKey, Task<T?>> deleteFunc,
        HttpContext httpContext,
        string successCode,
        string errorCode,
        Action<TKey, int>? auditSetter = null)
        where TKey : notnull
        {
            httpContext.Items["error"] = errorCode;

            var response = new ATMResponse<T?>();
            var result = await deleteFunc(id);

            response.IsError = result == null;
            response.ResponseCode = response.IsError ? errorCode : successCode;
            response.Data.Add(result);

            auditSetter?.Invoke(id, response.IsError ? 0 : 1);

            return response;
        }

        protected async Task<ATMResponse<TResponse?>> HandleSearchPagingAsync<TResponse, TItem>(
            Func<Task<TResponse>> action,
            string successCode,
            string errorCode) where TResponse : class, ISearchResult<TItem>
        {
            this.HttpContext.Items["error"] = errorCode;
            return await HandleGetAsync(
                async () => await action(),
                successCode,
                errorCode,
                result => result == null || (result.TotalRecord == 0 && result.Results.Count == 0)
            );
        }
        #endregion
    }
}


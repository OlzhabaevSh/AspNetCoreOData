﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Results
{
    /// <summary>
    /// Represents an action result that is a response to a PUT, PATCH, or a MERGE operation on an OData entity.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <remarks>This action result handles content negotiation and the HTTP prefer header.</remarks>
    public class UpdatedODataResult<T> : IActionResult
    {
        private readonly T _innerResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdatedODataResult{T}"/> class.
        /// </summary>
        /// <param name="entity">The updated entity.</param>
        public UpdatedODataResult(T entity)
        {
            this._innerResult = entity ?? throw new ArgumentNullException(nameof(entity));
        }

        /// <summary>
        /// Gets the entity that was updated.
        /// </summary>
        public virtual T Entity
        {
            get
            {
                return _innerResult;
            }
        }

        /// <inheritdoc/>
        public async virtual Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            HttpResponse response = context.HttpContext.Response;
            HttpRequest request = context.HttpContext.Request;
            IActionResult result = GetInnerActionResult(request);
            await result.ExecuteResultAsync(context).ConfigureAwait(false);
            ResultHelpers.AddServiceVersion(response, () => ODataUtils.ODataVersionToString(ResultHelpers.GetODataResponseVersion(request)));
        }

        internal IActionResult GetInnerActionResult(HttpRequest request)
        {
            if (RequestPreferenceHelpers.RequestPrefersReturnContent(request.Headers))
            {
                ObjectResult objectResult = new ObjectResult(_innerResult)
                {
                    StatusCode = StatusCodes.Status200OK
                };

                return objectResult;
            }
            else
            {
                return new StatusCodeResult((int)HttpStatusCode.NoContent);
            }
        }
    }
}

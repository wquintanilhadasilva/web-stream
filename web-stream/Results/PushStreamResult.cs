﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using Microsoft.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace web_stream.Results
{
    public class PushStreamResult : IActionResult
    {

        private readonly Action<Stream, CancellationToken> _onStreamAvailable;
        private readonly string _contentType;
        private readonly CancellationToken _requestAborted;


        public PushStreamResult(Action<Stream, CancellationToken> onStreamAvailable, string contentType, CancellationToken requestAborted)
        {
            _onStreamAvailable = onStreamAvailable;
            _contentType = contentType;
            _requestAborted = requestAborted;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            var stream = context.HttpContext.Response.Body;
            context.HttpContext.Response.GetTypedHeaders().ContentType = new MediaTypeHeaderValue(_contentType);
            _onStreamAvailable(stream, _requestAborted);
            return Task.CompletedTask;
        }
    }
}

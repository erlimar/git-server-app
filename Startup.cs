using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace app
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseOwin(pipe => pipe(next => GitHandle));
        }

        private Task GitHandle(IDictionary<string, object> environment)
        {
            var requestHeaders = (IDictionary<string, string[]>)environment["owin.RequestHeaders"];
            var responseHeaders = (IDictionary<string, string[]>)environment["owin.ResponseHeaders"];

            // git clone
            // > GET /repository.git/info/refs?service=git-upload-pack HTTP/1.1
            // > GET /repository.git/HEAD HTTP/1.1

            throw new UnauthorizedAccessException();


            // var responseStream = (Stream)environment["owin.ResponseBody"];
            // var responseHeaders = (IDictionary<string, string[]>)environment["owin.ResponseHeaders"];

            // responseHeaders["Content-Length"] = new string[] { responseBytes.Length.ToString(CultureInfo.InvariantCulture) };
            // responseHeaders["Content-Type"] = new string[] { "text/plain" };

            // return responseStream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }
    }
}

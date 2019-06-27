using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
            // Request data
            var requestHeaders = (IDictionary<string, string[]>)environment["owin.RequestHeaders"];
            var requestMethod = (string)environment["owin.RequestMethod"];
            var requestPath = (string)environment["owin.RequestPath"];
            var requestQueryString = (string)environment["owin.RequestQueryString"];
            var requestBody = (Stream)environment["owin.RequestBody"];

            // Response data
            var responseBody = (Stream)environment["owin.ResponseBody"];
            var responseHeaders = (IDictionary<string, string[]>)environment["owin.ResponseHeaders"];

            // git clone
            // > GET /repository.git/info/refs?service=git-upload-pack HTTP/1.1
            if (requestMethod == "GET" &&
                requestPath.EndsWith("/info/refs") &&
                requestQueryString == "service=git-upload-pack")
            {
                string processOutput = null;

                // git receive-pack --stateless-rpc --advertise-refs {repo-dir}
                using (var process = new Process())
                {
                    process.StartInfo.FileName = @"C:\Program Files\Git\cmd\git.exe";
                    process.StartInfo.Arguments = string.Format("receive-pack --stateless-rpc --advertise-refs \"{0}\"", @"C:\Users\Erlimar\source\git-server-tests\server\repository");
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;

                    process.Start();

                    processOutput = process.StandardOutput.ReadToEnd();

                    process.WaitForExit();
                }

                if (string.IsNullOrEmpty(processOutput))
                    return responseBody.FlushAsync();

                byte[] output = Encoding.UTF8.GetBytes(processOutput);

                // header
                //var pack = "#service=git-upload-pack\n";
                //var pack = " service=git-upload-pack\n";
                var pack = " service=git-upload-pack\n";
                var prefix = (pack.Length + 4).ToString("x4");
                var header = $"{prefix}{pack}0000\n";
                var headerBytes = Encoding.UTF8.GetBytes(header);

                responseHeaders["Expires"] = new string[] { "Fri, 01 Jan 1980 00:00:00 GMT" };
                responseHeaders["Pragma"] = new string[] { "no-cache" };
                responseHeaders["Cache-Control"] = new string[] { "no-cache, max-age=0, must-revalidate" };
                responseHeaders["Content-Length"] = new string[] { (output.Length + header.Length).ToString() };
                responseHeaders["Content-Type"] = new string[] { "application/x-git-receive-pack-advertisement" };

                environment["owin.ResponseStatusCode"] = 200;
                environment["owin.ResponseReasonPhrase"] = "OK";

                responseBody.Write(headerBytes, 0, headerBytes.Length);

                return responseBody.WriteAsync(output, 0, output.Length);
            }

            // > GET /repository.git/HEAD HTTP/1.1
            if (requestMethod == "GET" &&
                requestPath.EndsWith("/HEAD") &&
                string.IsNullOrEmpty(requestQueryString))
            {
                environment["owin.ResponseStatusCode"] = 200;
                environment["owin.ResponseReasonPhrase"] = "OK";

                return responseBody.FlushAsync();
            }

            throw new UnauthorizedAccessException();


            // var responseStream = (Stream)environment["owin.ResponseBody"];
            // var responseHeaders = (IDictionary<string, string[]>)environment["owin.ResponseHeaders"];

            // responseHeaders["Content-Length"] = new string[] { responseBytes.Length.ToString(CultureInfo.InvariantCulture) };
            // responseHeaders["Content-Type"] = new string[] { "text/plain" };

            // return responseStream.WriteAsync(responseBytes, 0, responseBytes.Length);
        }
    }
}

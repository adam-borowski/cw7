using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Cw7.Middlewares
{
    public class LoggingMiddleware {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }


        public async Task InvokeAsync(HttpContext context) {
            var request = context.Request;
            request.EnableBuffering();

            if(request != null) {
                var requestMethod = request.Method;
                var requestPath = request.Path;
                var query = request.QueryString.ToString();
                var body = "";
                using (StreamReader reader
                 = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
                {
                    body = await reader.ReadToEndAsync();
                    request.Body.Position = 0;
                }
                using (StreamWriter file = new StreamWriter(@"..\requestsLog.txt", true)) {
                    file.WriteLine("" +
                        "Method: " + requestMethod + 
                        "; Path: " + requestPath +
                        "; QueryString: " + query + 
                        "; Body: " + body + ";");
                }
            }

            await _next(context);
        }
    }    
}

using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using web_stream.Enums;
using web_stream.Models;
using web_stream.Results;

namespace web_stream.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {

        private static ConcurrentBag<StreamWriter> _clients;

        static ClienteController()
        {
            _clients = new ConcurrentBag<StreamWriter>();
        }


        [HttpPost]
        public async Task<IActionResult> PostAsync(Cliente cliente)
        {
            //Fazer o Insert
            await EnviarEvento(cliente, EventoEnum.Insert);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync(Cliente cliente)
        {
            //Fazer o Update
            await EnviarEvento(cliente, EventoEnum.Update);
            return Ok();
        }

        [HttpGet]
        [Route("Streaming")]
        public IActionResult Stream()
        {
            return new PushStreamResult(OnStreamAvailable, "text/event-stream", HttpContext.RequestAborted);
        }

        private void OnStreamAvailable(Stream stream, CancellationToken requestAborted)
        {
            var wait = requestAborted.WaitHandle;
            var client = new StreamWriter(stream);
            _clients.Add(client);

            wait.WaitOne();

            StreamWriter ignore;
            _clients.TryTake(out ignore);
        }

        private static async Task EnviarEvento(object dados, EventoEnum evento)
        {
            foreach (var client in _clients)
            {
                string jsonEvento = string.Format("{0}\n", JsonConvert.SerializeObject(new { dados, evento }));
                await client.WriteAsync(jsonEvento);
                await client.FlushAsync();
            }
        }

    }
}
using Microsoft.AspNetCore.Mvc;
using BZAPI.Models;

namespace BZAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BZ98LobbyController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<BZ98Lobby>> GetLobbies()
        {
            Console.WriteLine("GetLobbies requested...");
            return Ok(Storage.LobbyStorage.Lobbies);
        }
    }
}

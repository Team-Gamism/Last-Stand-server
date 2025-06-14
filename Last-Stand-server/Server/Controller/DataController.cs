using Microsoft.AspNetCore.Mvc;
using Server.Model.Account.Dto.Response;
using Server.Model.Data.Player.Dto.Request;
using Server.Model.Data.Player.Dto.Response;
using Server.Model.Data.Player.Entity;
using Server.Service.Interface;

namespace Server.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private readonly IDataService _dataService;
        private readonly IAccountService _accountService;

        public DataController(IDataService dataService,  IAccountService accountService)
        {
            _dataService = dataService;
            _accountService = accountService;
        }

        [HttpGet("name")]
        public async Task<ActionResult<PlayerNameResponse?>> GetPlayerDataAsync([FromQuery] string  playerId)
        {
            var playerData = await _dataService.GetByPlayerIdAsync(playerId);
            if (playerData == null)
                return NotFound(new {Message = "Player Not Found"});
            
            return Ok(new PlayerNameResponse
            {
                PlayerName = playerData.PlayerName,
            });
        }

        [HttpPost("name")]
        public async Task<ActionResult<PlayerDataResponse>> AddPlayerName([FromBody] PlayerDataRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.PlayerId) || string.IsNullOrWhiteSpace(req.PlayerName))
                return BadRequest(new { message = "PlayerId and PlayerName are required." });

            if (await _dataService.IsNameTakenAsync(req.PlayerName))
                return Conflict(new { message = "PlayerName is already taken." });

            var isNewAccount = await _accountService.CheckIsNewAccountByPlayerIdAsync(req.PlayerId);

            if (isNewAccount == null)
                return NotFound(new { message = "Player Not Found" });

            if (isNewAccount == false)
                return Conflict(new { message = "This account is not New" });

            var loginData = await _accountService.GetPlayerLoginDataByPlayerIdAsync(req.PlayerId);
            if (loginData == null)
                return NotFound(new { message = "Player Not Found" });
            
            var newData = new PlayerData
            {
                Id = loginData.Id,
                PlayerId = req.PlayerId,
                PlayerName = req.PlayerName
            };

            await _dataService.AddPlayerDataAsync(newData, false);
            await _accountService.UpdateIsNewAccountAsync(req.PlayerId, false);

            return Ok(new PlayerDataResponse
            {
                PlayerId = req.PlayerId,
                PlayerName = req.PlayerName
            });
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using StockService.Services;

namespace StockService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController : ControllerBase
    {
        private readonly RedisLockService _redisLockService; 
        private readonly ILogger<StockController> _logger;

        public StockController(ILogger<StockController> logger, RedisLockService redisLockService)
        { 
            _logger = logger;
            _redisLockService = redisLockService;
        }

        [HttpPut("update-stock/{productId}")]
        public async Task<IActionResult> UpdateStock(string productId, [FromBody] int quantity)
        {
            var lockKey = $"stock:update:{productId}";
            var expiry = TimeSpan.FromSeconds(6);

            


            if (await _redisLockService.AcquireLockAsync(lockKey, expiry))
            {
                var cts = new CancellationTokenSource();
                var renewTask = _redisLockService.RenewLockAsync(lockKey, expiry, cts.Token);

                try
                {
                    await Task.Delay(10000); // Örnek olarak 60 saniye süren bir işlem
                    // Belirtilen productId'ye ait stok güncelleme işlemi burada yapılır
                    return Ok("Stock updated successfully.");
                }
                finally
                {
                    cts.Cancel(); // Kilit yenilemeyi durdur
                    await _redisLockService.ReleaseLockAsync(lockKey);
                }
            }

            return StatusCode(StatusCodes.Status409Conflict, "Could not acquire lock. Try again later.");
        }
    }
}

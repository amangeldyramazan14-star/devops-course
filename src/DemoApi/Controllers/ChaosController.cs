using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DemoApi.Controllers;

// Имитация сбоев для лабораторных по мониторингу. Включается настройкой Chaos:Enabled.
[ApiController]
[Route("api/chaos")]
public sealed class ChaosController(IConfiguration config, ILogger<ChaosController> log) : ControllerBase, IActionFilter
{
    private static long _leakedMb;

    [NonAction]
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!config.GetValue<bool>("Chaos:Enabled"))
        {
            context.Result = NotFound();
        }
    }

    [NonAction]
    public void OnActionExecuted(ActionExecutedContext context) { }

    // Необработанное исключение -> 500 и ошибка в логе
    [HttpGet("error")]
    public IActionResult Error() =>
        throw new InvalidOperationException("Chaos: искусственная ошибка");

    [HttpGet("slow")]
    public async Task<IActionResult> Slow([FromQuery] int ms = 2000, CancellationToken ct = default)
    {
        await Task.Delay(ms, ct);
        return Ok(new { delayedMs = ms });
    }

    [HttpPost("crash")]
    public IActionResult Crash()
    {
        log.LogCritical("Chaos: аварийное завершение процесса");
        _ = Task.Run(async () =>
        {
            await Task.Delay(200);
            Environment.Exit(1);
        });
        return Accepted();
    }

    // Неуправляемая память не ограничена GC — контейнер упрётся в лимит и получит OOMKilled
    [HttpPost("leak")]
    public IActionResult Leak([FromQuery] int mb = 50)
    {
        const int Mb = 1024 * 1024;
        var ptr = Marshal.AllocHGlobal((nint)mb * Mb);
        var chunk = new byte[Mb];
        Array.Fill(chunk, (byte)1);
        for (var i = 0; i < mb; i++)
        {
            Marshal.Copy(chunk, 0, ptr + (nint)i * Mb, Mb);
        }
        return Ok(new { leakedMb = Interlocked.Add(ref _leakedMb, mb) });
    }
}

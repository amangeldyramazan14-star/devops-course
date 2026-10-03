using DemoApi.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace DemoApi.Controllers;

// GET / — какая версия образа запущена и на каком контейнере
[ApiController]
[Route("")]
public sealed class InfoController(IConfiguration config) : ControllerBase
{
    [HttpGet]
    public InfoResponse Get() =>
        new("DemoApi", config["APP_VERSION"] ?? "dev", Environment.MachineName);
}

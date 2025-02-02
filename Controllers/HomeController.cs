using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using JobFinderNet.Models;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Authorization;
using JobFinderNet.Repositories;

namespace JobFinderNet.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
    private readonly IJobRepository _jobRepository;

    public HomeController(
        ILogger<HomeController> logger,
        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
        IJobRepository jobRepository)
    {
        _logger = logger;
        _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        _jobRepository = jobRepository;
    }

    public async Task<IActionResult> Index()
    {
        var jobs = await _jobRepository.GetActiveJobsAsync(1, 10);
        return View(jobs);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }

    public IActionResult Routes()
    {
        var routes = _actionDescriptorCollectionProvider.ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Select(x => new
            {
                Action = x.ActionName,
                Controller = x.ControllerName,
                Path = $"/{x.ControllerName}/{x.ActionName}",
                Attributes = string.Join(", ", x.EndpointMetadata.OfType<AuthorizeAttribute>()
                    .Select(a => $"[{a.Roles ?? "Authenticated"}]"))
            })
            .OrderBy(x => x.Path)
            .ToList();

        return View(routes);
    }
}

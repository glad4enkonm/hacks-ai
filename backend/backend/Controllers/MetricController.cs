using backend.Authorization;
using backend.Helpers;
using database.Models;
using database.Repository;
using database.Models.History;
using database.Repository.History;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class MetricController: ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IMetricRepository _repositoryMetric;

    private static readonly HttpClient _client = new();

    public MetricController(ILogger<UserController> logger
        ,IMetricRepository repositoryMetric
    )
    {
        _logger = logger;
        _repositoryMetric = repositoryMetric;
    }

    [HttpGet("Proxy/{query}")]
    public async Task<string?> GetProxyAsync(string query)
    {
        HttpResponseMessage response = await _client.GetAsync("http://ml:5001/"+ query);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

#region Metric CRD
    [HttpPost("Metric")]
    public Metric CreateMetric(Metric entity)
    {
        return _repositoryMetric.Insert(entity);
    }

    [HttpGet("Metric/{id}")]
    public Metric? GetMetric(ulong id)
    {
        return _repositoryMetric.Get(id);
    }

    [HttpGet("Metric/list")]
    public IList<Metric> GetMetricList()
    {
        return _repositoryMetric.GetAll().ToList();
    }

    [HttpDelete("Metric/{id}")]
    public bool DeleteMetric(ulong id)
    {
        var entity = _repositoryMetric.Get(id) ?? throw new InvalidOperationException();
        return _repositoryMetric.Delete(entity);
    }

#endregion

}
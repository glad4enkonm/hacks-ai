
using backend.Authorization;

using Microsoft.AspNetCore.Mvc;


namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class CustomerController: ControllerBase
{
    private readonly ILogger<UserController> _logger;    

    public CustomerController(ILogger<UserController> logger       
    )
    {
        _logger = logger;        
    }


}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Stenn.TestModel.AppService.Web.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]
    public class IndexController : ControllerBase
    {
        [HttpGet("[action]")]
        public ActionResult Hello()
        {
            return Content("Hello world");
        }
    }
}

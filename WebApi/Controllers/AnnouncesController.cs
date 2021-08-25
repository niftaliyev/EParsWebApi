using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Models;
using WebApi.Repository;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnnouncesController : ControllerBase
    {
        private readonly UnitOfWork unitOfWork;
        public AnnouncesController(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        [HttpGet]
        public async Task<IActionResult> Announces()
        {
            var announces =  unitOfWork.Announces.GetAll().OrderByDescending(x => x.id).Take(20);

            return Ok(announces);
        }
    }
}

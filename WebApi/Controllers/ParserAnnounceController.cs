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
    public class ParserAnnounceController : ControllerBase
    {
        private readonly UnitOfWork unitOfWork;

        public ParserAnnounceController(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        [HttpGet]
        public IEnumerable<ParserAnnounce> Announces()
        {

            return unitOfWork.ParserAnnounceRepository.GetAll();
        }

        [HttpPost("create")]
        public void Create(Announce announce)
        {

            unitOfWork.Announces.Create(announce);
        }
    }
}

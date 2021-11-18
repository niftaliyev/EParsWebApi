using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Repository;

namespace WebApi.Services.ArendaAz
{
    public class ArendaAzParser
    {
        private readonly UnitOfWork unitOfWork;

        public ArendaAzParser(UnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public async Task ArendaAzPars()
        {
            //Console.WriteLine("START");
            //var cities = unitOfWork.CitiesRepository.GetAll().ToList();
            //Random random = new Random();
            //var city = cities[random.Next(0, cities.Count - 1)];
            //TelegramBotService.Sender(city.name);

        }
    }
}

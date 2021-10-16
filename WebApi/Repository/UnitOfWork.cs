using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Repository
{
    public class UnitOfWork : IDisposable
    {
        private readonly MySqlConnection connection;

        private readonly Lazy<AnnouncesRepository> announcesRepository;
        private readonly Lazy<ParserAnnounceRepository> parserAnnounceRepository;
        private readonly Lazy<RieltorRepository> rieltorRepository;
        private readonly Lazy<OwnerRepository> ownerRepository;
        private readonly Lazy<CitiesRepository> citiesRepository; 
        private readonly Lazy<CheckNumberRepository> checkNumberRepository;
        private readonly Lazy<MetrosRepository> metrosRepository;
        private readonly Lazy<MarksRepository> marksRepository;


        public UnitOfWork(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
            announcesRepository = new Lazy<AnnouncesRepository>(() => new AnnouncesRepository(connection));
            parserAnnounceRepository = new Lazy<ParserAnnounceRepository>(() => new ParserAnnounceRepository(connection));
            rieltorRepository = new Lazy<RieltorRepository>(() => new RieltorRepository(connection));
            ownerRepository = new Lazy<OwnerRepository>(() => new OwnerRepository(connection));
            citiesRepository = new Lazy<CitiesRepository>(() => new CitiesRepository(connection));
            checkNumberRepository = new Lazy<CheckNumberRepository>(() => new CheckNumberRepository(connection));
            metrosRepository = new Lazy<MetrosRepository>(() => new MetrosRepository(connection));
            marksRepository = new Lazy<MarksRepository>(() => new MarksRepository(connection));
        }

        public AnnouncesRepository Announces => announcesRepository.Value;
        public ParserAnnounceRepository ParserAnnounceRepository => parserAnnounceRepository.Value;
        public RieltorRepository RieltorRepository => rieltorRepository.Value;
        public OwnerRepository OwnerRepository => ownerRepository.Value;
        public CitiesRepository CitiesRepository => citiesRepository.Value;
        public CheckNumberRepository CheckNumberRepository => checkNumberRepository.Value;
        public MetrosRepository MetrosRepository => metrosRepository.Value;
        public MarksRepository MarkRepository => marksRepository.Value;

        public void Dispose()
        {
            connection.Dispose();
            GC.SuppressFinalize(this);
        }

        ~UnitOfWork()
        {
            connection.Dispose();
        }
    }
}

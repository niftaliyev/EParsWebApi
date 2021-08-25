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

        public UnitOfWork(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
            announcesRepository = new Lazy<AnnouncesRepository>(() => new AnnouncesRepository(connection));
            parserAnnounceRepository = new Lazy<ParserAnnounceRepository>(() => new ParserAnnounceRepository(connection));
        }

        public AnnouncesRepository Announces => announcesRepository.Value;
        public ParserAnnounceRepository ParserAnnounceRepository => parserAnnounceRepository.Value;

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

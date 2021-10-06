using Dapper;
using MySql.Data.MySqlClient;

namespace WebApi.Repository
{
    public class CheckNumberRepository : ICheckNumber
    {
        private readonly MySqlConnection connection;
        public CheckNumberRepository(MySqlConnection connection)
        {
            this.connection = connection;
        }

        public int CheckNumberForOwner(params string[] numbers)
        {
            for (int i = 0; i < numbers.Length; i++)
            {
                string uQuery = $"SELECT * FROM owner WHERE phone = {numbers[i]}";
                var result = connection.Execute(uQuery);
                if (result > 0)
                {
                    return 1;
                }
            }

            return -1;
        }

        public int CheckNumberForRieltor(params string[] numbers)
        {
            for (int i = 0; i < numbers.Length; i++)
            {
                string uQuery = $"SELECT * FROM rieltor WHERE phone = {numbers[i]}";
                var result = connection.Execute(uQuery);
                if (result > 0)
                {
                    return 2;
                }
            }

            return -1;
        }

    }
}

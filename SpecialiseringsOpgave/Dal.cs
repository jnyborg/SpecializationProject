using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialiseringsOpgave
{
    class Dal
    {
        private const string ConnectionString = @"Data Source=jnyborg-laptop\sqlexpress; database=SpecialiseringsDB; Integrated Security=true;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString); ;
        }

        public static bool CheckConnection()
        {
            var con = GetConnection();
            try
            {
                con.Open();
                con.Close();
                return true;
            }
            catch (SqlException)
            {
                return false;
            }
        }
    }
}

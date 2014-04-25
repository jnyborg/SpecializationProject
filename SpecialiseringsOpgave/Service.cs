using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpecialiseringsOpgave
{
    class Service
    {
        // TODO errorhandling
        private static DataTable GetDataTable(string sql)
        {
            var datatable = new DataTable();
            var connection = Dal.GetConnection();
            var dataAdapter = new SqlDataAdapter(sql, connection);
            dataAdapter.Fill(datatable);
            return datatable;
        }

        public static DataTable GetDestinations()
        {
            return GetDataTable("select * from Destination");
        }

        public static void DeleteDestination(int id)
        {
            var connection = Dal.GetConnection();
            var command = new SqlCommand("delete from destination where id = @id", connection);
            command.Parameters.Add(new SqlParameter("@id", id));
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();

        }

        public static void UpdateDestination(int id, string country, string switchDay, double price)
        {
            using (var connection = Dal.GetConnection())
            {
                try
                {
                    var command =
                      new SqlCommand(
                          "update Destination set country = @country, switchDay = @switchDay, price = @price where id = @id",
                          connection);
                    command.Parameters.Add(new SqlParameter("@country", country));
                    command.Parameters.Add(new SqlParameter("@switchDay", switchDay));
                    command.Parameters.Add(new SqlParameter("@price", price));
                    command.Parameters.Add(new SqlParameter("@id", id));
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (SqlException)
                {
                    throw new ServiceException("En fejl skete ved opdateringen.");
                }
            }
        }

        public static void CreateDestination(string country, string switchDay, double price)
        {
            using (var connection = Dal.GetConnection())
            {
                try
                {
                    var command = new SqlCommand("insert into Destination values (@country, @switchDay, @price)", connection);
                    command.Parameters.Add(new SqlParameter("@country", country));
                    command.Parameters.Add(new SqlParameter("@switchDay", switchDay));
                    command.Parameters.Add(new SqlParameter("@price", price));
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (SqlException sqlException)
                {
                    switch (sqlException.Number)
                    {
                        case 2627:
                            throw new ServiceException("Den indtastede destination er ikke unik.");
                        default:
                            throw new ServiceException("En fejl skete ved oprettelsen.");

                    }
                }
            }
        }

        public static DataTable GetHolidayHomes()
        {
            return GetDataTable("select * from HolidayHome");
        }

        public static void DeleteHolidayHome(int id)
        {
            var connection = Dal.GetConnection();
            var command = new SqlCommand("delete from HolidayHome where id = @id", connection);
            command.Parameters.Add(new SqlParameter("@id", id));
            connection.Open();
            command.ExecuteNonQuery();
            connection.Close();

        }

        //TODO fix error
        public static void UpdateHolidayHome(int id, string description, int maxPersons, int shoppingDistance,
            int beachDistance, int destinationId)
        {
            using (var connection = Dal.GetConnection())
            {
                try
                {
                    var command = new SqlCommand("update HolidayHome " +
                                                 "set description = @description, " +
                                                 "maxPersons = @maxPersons, " +
                                                 "shoppingDistance = @shoppingDistance, " +
                                                 "beachDistance = @beachDistance, " +
                                                 "destinationId = @destinationId " +
                                                 "where id = @id", connection);
                    command.Parameters.Add(new SqlParameter("@description", description));
                    command.Parameters.Add(new SqlParameter("@maxPersons", maxPersons));
                    command.Parameters.Add(new SqlParameter("@shoppingDistance", shoppingDistance));
                    command.Parameters.Add(new SqlParameter("@beachDistance", beachDistance));
                    command.Parameters.Add(new SqlParameter("@destinationId", destinationId));
                    command.Parameters.Add(new SqlParameter("@id", id));
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (SqlException sqlException)
                {
                    throw new ServiceException("Der skete en fejl ved opdateringen. " + sqlException.Message);
                }
            }
        }

        // TODO fix error
        public static void CreateHolidayHome(string description, int maxPersons, int shoppingDistance,
            int beachDistance, int destinationId)
        {
            using (var connection = Dal.GetConnection())
            {
                try
                {
                    var command = new SqlCommand("insert into HolidayHome " +
                                                 "values (@description, @maxPersons, @shoppingDistance, " +
                                                 "@beachDistance, @destinationId)", connection);
                    command.Parameters.Add(new SqlParameter("@description", description));
                    command.Parameters.Add(new SqlParameter("@maxPersons", maxPersons));
                    command.Parameters.Add(new SqlParameter("@shoppingDistance", shoppingDistance));
                    command.Parameters.Add(new SqlParameter("@beachDistance", beachDistance));
                    Debug.WriteLine(destinationId);
                    command.Parameters.Add(new SqlParameter("@destinationId", destinationId));
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (SqlException sqlException)
                {
                    switch (sqlException.Number)
                    {
                        case 547:
                            throw new ServiceException("Ikke en gyldig destination.");
                        default:
                            throw new ServiceException("En fejl skete ved oprettelsen." + sqlException.Message + " " + sqlException.Number);
                    }

                }
            }
        }

        public static DataTable GetWeeklyHolidayHomeInfos(int holidayHomeId)
        {
            return GetDataTable("select weekNumber, price, isAvailable from WeeklyHolidayHomeInfo where holidayHomeId =" + holidayHomeId);
        }

        public static void AddWeeklyHolidayHomeInfos(int holidayHomeId, DataTable resultTable)
        {

            string longCommand = "delete from WeeklyHolidayHomeInfo where holidayHomeId = " + holidayHomeId + "; ";
            foreach (DataRow row in resultTable.Rows)
            {
                longCommand += "insert into WeeklyHolidayHomeInfo values (" + row["weekNumber"] + "," + row["price"] +
                               "," + Convert.ToInt32(row["isAvailable"]) + ", " + holidayHomeId + "); ";
            }

            using (var connection = Dal.GetConnection())
            {
                var command = new SqlCommand(longCommand, connection);
                connection.Open();
                Debug.WriteLine(command.ExecuteNonQuery());
            }




        }
    }


    /// <summary>
    /// Exception class to use in the service layer. Used to provide user-friendly errormessages.
    /// </summary>
    public class ServiceException : Exception
    {
        public ServiceException()
        {
        }

        public ServiceException(string message)
            : base(message)
        {
        }

        public ServiceException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

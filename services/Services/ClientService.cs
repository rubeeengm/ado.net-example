using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Common;
using Models;

namespace Services  {
    public class ClientService {

        public void create(Client model) {
            using (var context = new SqlConnection(Parameters.connectionString)) {
                context.Open();
                
                var command = new SqlCommand(
                    "INSERT INTO CLIENTS(NAME) OUTPUT INSERTED.ID " +
                    "VALUES(@NAME)", context
                );

                command.Parameters.AddWithValue("@NAME", model.name);

                model.id = Convert.ToInt32(command.ExecuteScalar());
            }
        }
        
        public List<Client> getAll() {
            var result = new List<Client>();

            using (var context = new SqlConnection(Parameters.connectionString)) {
                context.Open();
                
                var command = new SqlCommand("SELECT * FROM CLIENTS", context);

                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        var client = new Client {
                            id = Convert.ToInt32(reader["@ID"])
                            , name = reader["NAME"].ToString()
                        };
                        
                        result.Add(client);
                    }
                }
            }

            return result;
        }
        
        public Client get(int id) {
            var result = new Client();
            
            using (var context = new SqlConnection(Parameters.connectionString)){
                context.Open();
                
                var command = new SqlCommand(
                    "SELECT * FROM CLIENTS WHERE ID = @ID", context
                );
                command.Parameters.AddWithValue("@ID", id);

                using (var reader = command.ExecuteReader()) {
                    reader.Read();

                    result.id = Convert.ToInt32(reader["@ID"]);
                    result.name = reader["NAME"].ToString();
                }
            }

            return result;
        }

        public void update(Client model) {
            using (var context = new SqlConnection(Parameters.connectionString)){
                context.Open();
                
                var command = new SqlCommand(
                    "UPDATE CLIENTES SET NAME = @NAME WHERE ID = @ID", context
                );

                command.Parameters.AddWithValue("@ID", model.id);
                command.Parameters.AddWithValue("@NAME", model.name);
                command.ExecuteNonQuery();
            }
        }
        
        public void delete(int id) {
            using (var context = new SqlConnection(Parameters.connectionString)) {
                context.Open();
                
                var command = new SqlCommand(
                    "DELETE FROM CLIENTS WHERE ID = @ID", context
                );

                command.Parameters.AddWithValue("@ID", id);
                command.ExecuteNonQuery();
            }
        }
    }
}
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Diagnostics;

namespace TestTaskRefactoring
{
    public class RefactoredClass
    {

        private readonly ConcurrentDictionary<string, Task<Order>> _cache;

        [WebMethod]
        public async Task<Order> LoadOrderInfoAsync(string orderCode)
        {
            if (string.IsNullOrEmpty(orderCode))
            {
                logger.Log("ERROR", "Order code is null or empty");

                throw new ArgumentException("Order code is null or empty.", nameof(orderCode));
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {

                Task<Order> orderTask = _cache.GetOrAdd(orderCode, async(key) => await GetOrderFromDbAsync(key));
                var order = await orderTask;

                if(order == null)
                {
                    logger.Log("WARN", "Order {OrderCode} doesn't exist in db", orderCode);

                    _cache.TryRemove(orderCode, out _);
                }

                return order;
            }
            catch (Exception ex)
            {
                logger.Log("ERROR", "Failed to load order {OrderCode}. Exception: {Exception}", orderCode, ex.Message );
                
                throw new SomeMoreRelevantCustomException($"Failed to load order {orderCode}", ex);
            }
            finally 
            {
                stopWatch.Stop();

                logger.Log("INFO", "Elapsed - {Elapsed}", stopWatch.Elapsed);
            }
        }

        private async Task<Order> GetOrderFromDbAsync(string orderCode)
        {
            const string queryTemplate = @"SELECT OrderID, CustomerID, TotalMoney 
                                     FROM dbo.Orders where OrderCode = @OrderCode";
            try 
            {
                using(SqlConnection connection = new SqlConnection(this.connectionString))
                {
                    using(SqlCommand command = new SqlCommand(queryTemplate, connection))
                    {
                        command.Parameters.AddWithValue("@OrderCode", orderCode);
                        
                        await connection.OpenAsync();

                        using(SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if(await reader.ReadAsync())
                            {
                                return new Order(
                                    (string)reader[0],
                                    (string)reader[1],
                                    (int)reader[2]
                                );
                            }
                        }
                    }
                }

                return null;
            }
            catch (SqlException ex) 
            {
                logger.Log("ERROR", "Failed to load order {OrderCode}", orderCode);
                
                throw;
            }
        }
    } 
}

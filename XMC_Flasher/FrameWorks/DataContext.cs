using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace XMC_Flasher.FrameWorks
{
    internal class DataContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {  
            optionsBuilder.UseSqlServer(
                SettingsManager.Instance.DB_Connection ?? ".\\");             
        }
        /// <summary>
        /// Reserve device ID to keep device unique and track when device was activated
        /// </summary>
        /// <returns></returns>
        public string ReserveRDMAddress()
        {
            var rdmAddress = new SqlParameter
            {
                ParameterName = "rdmAddress",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output,
            };
            Database.ExecuteSqlRaw("EXEC usp_ReserveRMDAddress @rdmAddress OUTPUT", rdmAddress);            
            return int.Parse(rdmAddress.Value.ToString() ?? "0").ToString("X8"); 
        }
        public bool Validate()
        {
           return Database.ExecuteSqlRaw("Select 1") > 0;
        }
    }
}

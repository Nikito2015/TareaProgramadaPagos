using log4net;
using System;
using System.Data.SqlClient;


namespace DataAccess.Repositories
{
    public class BaseRepository : IDisposable
    {
        protected ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public BaseRepository(string connString)
        {
            Connection = new SqlConnection(connString);
            Command = new SqlCommand("", Connection);
        }
        private bool _disposed = false;
        public SqlConnection Connection { get;private set; }
        public SqlCommand Command { get; set; }
        public SqlDataReader DataReader { get; set; }

        public void Dispose()
        {
            dispose(true);
            GC.SuppressFinalize(this);          
        }

        private void dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Connection.Close();
                    Connection.Dispose();                   
                    Command.Dispose();                   
                    DataReader.Close();
                    Connection = null;
                    Command = null;
                    DataReader = null;
                }
                _disposed = true;
            }
        }

        ~BaseRepository()
        {
            dispose(false);
        }
    }
}

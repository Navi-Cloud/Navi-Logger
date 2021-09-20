using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using NaviLogger.Model;

namespace NaviLogger.Repository
{
    public class LogRepository
    {
        private readonly IMongoCollection<LogMessage> _message;

        public LogRepository(MongoContext context)
        {
            _message = context._MongoDatabase.GetCollection<LogMessage>(nameof(LogMessage));
        }

        public void AppendLog(LogMessage logMessage)
        {
            _message.InsertOne(logMessage);
        }

        public async Task<List<LogMessage>> FindLogByService(string serviceName)
        {
            var result = await _message.FindAsync(a => a.LogService == serviceName);

            return await result.ToListAsync();
        }

        public async Task<List<LogMessage>> FindLogByInfo()
        {
            var result = await _message.FindAsync(a => a.LogType == LogMessageType.Info);

            return await result.ToListAsync();
        }

        public async Task<List<LogMessage>> FindLogByError()
        {
            var result = await _message.FindAsync(a => a.LogType == LogMessageType.Error);

            return await result.ToListAsync();
        }
    }
}
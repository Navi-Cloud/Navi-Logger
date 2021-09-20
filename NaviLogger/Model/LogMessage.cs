using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace NaviLogger.Model
{
    public enum LogMessageType
    {
        Info,
        Error
    }
    
    public class LogMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("logCreatedAt")]
        public DateTime LogCreatedAt { get; set; }
        
        [BsonElement("logType")]
        public LogMessageType LogType { get; set; }
        
        [BsonElement("logService")]
        public string LogService { get; set; } // such as AuthenticationService, like service identifier.
        
        [BsonElement("message")]
        public string Message { get; set; }
    }
}
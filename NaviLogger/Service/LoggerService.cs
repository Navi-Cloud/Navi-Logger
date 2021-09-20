using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NaviLogger.Extensions;
using NaviLogger.Model;
using NaviLogger.Repository;
using Newtonsoft.Json;
using LogMessage = NaviLogger.Model.LogMessage;

namespace NaviLogger.Service
{
    public class LoggerService
    {
        private readonly string _logTopic;
        
        // Local ASP Logger
        private readonly ILogger<LoggerService> _logger;
        
        // Token
        private CancellationTokenSource _cancellationTokenSource = null;
        
        // Logger Thread
        private Task _loggerThreadTask = null;
        
        // Consumer
        private readonly IConsumer<Ignore, string> _consumer;

        private readonly LogRepository _logRepository;

        public LoggerService(IConfiguration configuration, ILogger<LoggerService> logger, LogRepository logRepository)
        {
            _logTopic = configuration.GetKafkaStrings("KafkaTopic");

            _logger = logger;
            
            _consumer = new ConsumerBuilder<Ignore, string>(new ConsumerConfig
            {
                BootstrapServers = configuration.GetKafkaStrings("KafkaServerAddr"),
                GroupId = "NaviLoggerService",
                AutoOffsetReset = AutoOffsetReset.Earliest
            }).Build();

            _logRepository = logRepository;
        }
        
        public void StartLogging()
        {
            if (_cancellationTokenSource != null || _loggerThreadTask != null)
            {
                throw new Exception("Either consumer or cancellation token or logger task is not null!");
            }
            _cancellationTokenSource = new CancellationTokenSource();
            _loggerThreadTask = Task.Run(() => ConsumeLogs(_cancellationTokenSource));
        }

        public void StopLogging()
        {
            if (_cancellationTokenSource == null || _loggerThreadTask == null)
            {
                throw new Exception("Seems like logging is already off!");
            }
            
            // Cancel using token
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            
            // Wait for task to complete.
            _loggerThreadTask.Wait();
            
            // Unsubscribe it
            _consumer.Unsubscribe();
            _consumer.Unassign();

            // Free Token
            _cancellationTokenSource = null;
            
            // Free Task
            _loggerThreadTask = null;
        }

        private void ConsumeLogs(CancellationTokenSource tokenSource)
        {
            _consumer.Subscribe(_logTopic);

            try
            {
                while (!tokenSource.IsCancellationRequested)
                {
                    var message = _consumer.Consume(tokenSource.Token);
                    _logger.LogInformation($"Message Input {message.Message.Value}");

                    try
                    {
                        _logRepository.AppendLog(JsonConvert.DeserializeObject<LogMessage>(message.Message.Value));
                    }
                    catch (Exception innerException)
                    {
                        _logger.LogCritical("Failed to deserialize message.");
                        _logger.LogCritical($"Message Content: {message.Message.Value}");
                        _logger.LogCritical($"Exception is: {innerException.Message}");
                        _logger.LogCritical($"StackTrace is: {innerException.StackTrace}");
                        _logRepository.AppendLog(new LogMessage
                        {
                            Message = $"Fatal Error occurred when deserializing message.{innerException.Message}",
                            LogType = LogMessageType.Error,
                            LogService = "LoggerService",
                            LogCreatedAt = DateTime.Now
                        });
                    }
                }
            }
            catch
            {
                // Do nothing
                _logger.LogInformation("Cancel Requested.");
            }
        }
    }
}
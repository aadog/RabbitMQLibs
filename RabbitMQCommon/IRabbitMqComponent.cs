using RabbitMQ.Client;

namespace RabbitMQCommon
{
    public interface IRabbitMqComponent
    {
        /// <summary>
        /// 启动组件（初始化连接、通道等）
        /// </summary>
        public Task StartAsync(IConnectionFactory connectionFactory, CancellationToken cancellationToken = default);

        /// <summary>
        /// 组件是否已启动
        /// </summary>
        bool IsStarted { get; set; }
    }
}

﻿using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Attachments;
using NServiceBus.Features;
using NServiceBus.Transport.SQLServer;
using Xunit;

public class IntegrationTests
{
    static ManualResetEvent resetEvent;

    static IntegrationTests()
    {
        DbSetup.Setup();
    }

    [Fact]
    public async Task Run()
    {
        resetEvent = new ManualResetEvent(false);
        var configuration = new EndpointConfiguration("AttachmentsTest");
        configuration.UsePersistence<LearningPersistence>();
        configuration.UseTransport<LearningTransport>();
        configuration.EnableAttachments(Connection.OpenAsyncConnection, TimeToKeep.Default);
        var endpoint = await Endpoint.Start(configuration);
        await SendStartMessage(endpoint);
        resetEvent.WaitOne();
        await endpoint.Stop();
    }

    [Fact(Skip = "ignore")]
    public async Task RunSql()
    {
        resetEvent = new ManualResetEvent(false);
        var configuration = new EndpointConfiguration("AttachmentsTest");
        configuration.UsePersistence<LearningPersistence>();
        var transport = configuration.UseTransport<SqlServerTransport>();
        transport.UseCustomSqlConnectionFactory(Connection.OpenAsyncConnection);
        configuration.DisableFeature<TimeoutManager>();
        configuration.DisableFeature<MessageDrivenSubscriptions>();
        configuration.EnableAttachments(Connection.OpenAsyncConnection, TimeToKeep.Default);
        var endpoint = await Endpoint.Start(configuration);
        await SendStartMessage(endpoint);
        resetEvent.WaitOne();
        await endpoint.Stop();
    }

    static async Task SendStartMessage(IEndpointInstance endpoint)
    {
        var sendOptions = new SendOptions();
        sendOptions.RouteToThisEndpoint();
        var attachment = sendOptions.Attachments();
        attachment.Add(GetStream);
        await endpoint.Send(new SendMessage(), sendOptions);
    }

    static Stream GetStream()
    {
        var stream = new MemoryStream();
        var streamWriter = new StreamWriter(stream);
        streamWriter.Write("sdflgkndkjfgn");
        streamWriter.Flush();
        stream.Position = 0;
        return stream;
    }

    class SendHandler : IHandleMessages<SendMessage>
    {
        public Task Handle(SendMessage message, IMessageHandlerContext context)
        {
            var replyOptions = new ReplyOptions();
            var outgoingAttachment = replyOptions.Attachments();
            outgoingAttachment.Add(() =>
            {
                var incomingAttachment = context.Attachments();
                return incomingAttachment.GetStream();
            });
            return context.Reply(new ReplyMessage(), replyOptions);
        }
    }

    class ReplyHandler : IHandleMessages<ReplyMessage>
    {
        public async Task Handle(ReplyMessage message, IMessageHandlerContext context)
        {
            using (var memoryStream = new MemoryStream())
            {
                var incomingAttachment = context.Attachments();
                await incomingAttachment.CopyTo(memoryStream);
                memoryStream.Position = 0;
                var buffer = memoryStream.GetBuffer();
                Debug.WriteLine(buffer);
            }

            resetEvent.Set();
        }
    }

    class SendMessage : IMessage
    {
    }

    class ReplyMessage : IMessage
    {
    }
}
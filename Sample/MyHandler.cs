﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NServiceBus;

class MyHandler :
    IHandleMessages<MyMessage>
{
    public async Task Handle(MyMessage message, IMessageHandlerContext context)
    {
        Console.WriteLine("Hello from MyHandler.");
        using (var memoryStream = new MemoryStream())
        {
            var incomingAttachments = context.Attachments();
            await incomingAttachments.CopyTo("foo", memoryStream);
            memoryStream.Position = 0;
            var buffer = memoryStream.GetBuffer();
            Debug.WriteLine(buffer);
        }
    }
}
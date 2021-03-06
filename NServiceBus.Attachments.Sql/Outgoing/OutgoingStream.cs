﻿using System;
using System.IO;
using System.Threading.Tasks;
using NServiceBus.Attachments;

class OutgoingStream
{
    public Func<Task<Stream>> AsyncStreamFactory;
    public Func<Stream> StreamFactory;
    public Stream StreamInstance;
    public Func<Task<byte[]>> AsyncBytesFactory;
    public Func<byte[]> BytesFactory;
    public byte[] BytesInstance;
    public GetTimeToKeep TimeToKeep;
    public Action Cleanup;
}
﻿using System;
using System.IO;
using System.Threading.Tasks;

namespace NServiceBus.Attachments
{
    public interface IOutgoingAttachment
    {
        void Add<T>(Func<Task<T>> stream, GetTimeToKeep timeToKeep = null, Action cleanup = null) where T : Stream;

        void Add(Func<Stream> stream, GetTimeToKeep timeToKeep = null, Action cleanup = null);
    }
}
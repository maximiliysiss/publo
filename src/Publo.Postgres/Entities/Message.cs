using System;

namespace Publo.Postgres.Entities;

internal sealed record Message(MessageId Id, object Payload, Type Type, DateTimeOffset CreatedAt);

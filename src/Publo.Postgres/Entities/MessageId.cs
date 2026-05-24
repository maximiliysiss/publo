namespace Publo.Postgres.Entities;

[StronglyTypedId(
    backingType: StronglyTypedIdBackingType.Long,
    jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson | StronglyTypedIdJsonConverter.SystemTextJson)]
internal readonly partial struct MessageId;

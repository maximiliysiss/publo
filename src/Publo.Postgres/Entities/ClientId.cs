namespace Publo.Postgres.Entities;

[StronglyTypedId(
    backingType: StronglyTypedIdBackingType.Guid,
    jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson | StronglyTypedIdJsonConverter.SystemTextJson)]
internal readonly partial struct ClientId;

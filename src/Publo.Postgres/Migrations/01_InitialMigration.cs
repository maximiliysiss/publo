using System;
using FluentMigrator;
using Microsoft.Extensions.Options;
using Publo.Postgres.Migrations.Options;
using Publo.Postgres.Migrations.Shared;

namespace Publo.Postgres.Migrations;

[Migration(1, "InitialMigration")]
internal sealed class InitialMigration : SqlMigration
{
    private readonly MigrationOptions _options;

    public InitialMigration(IOptions<MigrationOptions> options) => _options = options.Value;

    protected override string GetUpSql(IServiceProvider services) => $@"
CREATE TABLE {_options.SchemaName}.messages (
    id bigint PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    type text NOT NULL,
    payload jsonb NOT NULL,
    created_at timestamp with time zone NOT NULL
);

CREATE INDEX ON {_options.SchemaName}.messages (type);

CREATE TABLE {_options.SchemaName}.clients (
    id uuid PRIMARY KEY,
    created_at timestamp with time zone NOT NULL
);

CREATE TABLE {_options.SchemaName}.handled (
    client_id uuid NOT NULL,
    message_id bigint NOT NULL,
    created_at timestamp with time zone NOT NULL,
    PRIMARY KEY (client_id, message_id)
);
";

    protected override string GetDownSql(IServiceProvider services) => $@"
DROP TABLE {_options.SchemaName}.handled;
DROP TABLE {_options.SchemaName}.clients;
DROP TABLE {_options.SchemaName}.messages;
";
}

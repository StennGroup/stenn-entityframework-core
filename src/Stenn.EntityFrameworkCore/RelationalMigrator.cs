﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Stenn.EntityFrameworkCore
{
#pragma warning disable EF1001
    public class MigratorWithStaticMigrations : Migrator
    {
        private readonly IRelationalConnection _connection;
        private readonly IHistoryRepository _historyRepository;
        private readonly IMigrationCommandExecutor _migrationCommandExecutor;
        private readonly IMigrationsSqlGenerator _migrationsSqlGenerator;

        private MigrateContext? _migrateContext;

        /// <inheritdoc />
        public MigratorWithStaticMigrations(IMigrationsAssembly migrationsAssembly, IHistoryRepository historyRepository, IDatabaseCreator databaseCreator,
            IMigrationsSqlGenerator migrationsSqlGenerator, IRawSqlCommandBuilder rawSqlCommandBuilder, IMigrationCommandExecutor migrationCommandExecutor,
            IRelationalConnection connection, ISqlGenerationHelper sqlGenerationHelper, ICurrentDbContext currentContext,
            IConventionSetBuilder conventionSetBuilder, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger,
            IDiagnosticsLogger<DbLoggerCategory.Database.Command> commandLogger, IDatabaseProvider databaseProvider,
            IStaticMigrationsService staticMigrationsService)
            : base(migrationsAssembly, historyRepository, databaseCreator, migrationsSqlGenerator,
                rawSqlCommandBuilder, migrationCommandExecutor, connection, sqlGenerationHelper, currentContext, conventionSetBuilder, logger, commandLogger,
                databaseProvider)
        {
            StaticMigrationsService = staticMigrationsService;
            _historyRepository = historyRepository;
            _migrationsSqlGenerator = migrationsSqlGenerator;
            _migrationCommandExecutor = migrationCommandExecutor;
            _connection = connection;
        }


        private IStaticMigrationsService StaticMigrationsService { get; }

        /// <inheritdoc />
        public override void Migrate(string? targetMigration = null)
        {
            MigrateGuard(targetMigration);
            var modified = DateTime.UtcNow;

            try
            {
                var appliedMigrations = _historyRepository.GetAppliedMigrations();
                _migrateContext = GetMigrateContext(appliedMigrations, modified);
                if (_migrateContext.HasMigrations)
                {
                    // ReSharper disable once RedundantArgumentDefaultValue
                    base.Migrate(null);
                }
                else
                {
                    var revertOperations = StaticMigrationsService.GetRevertOperations(false, _migrateContext.MigrationDate);
                    var applyOperations = StaticMigrationsService.GetApplyOperations(false, _migrateContext.MigrationDate);
                    var operations = revertOperations.Concat(applyOperations).ToList();
                    Execute(operations);
                }
            }
            finally
            {
                _migrateContext = null;
            }
            var dictEntityOperations = StaticMigrationsService.MigrateDictionaryEntities(modified);
            Execute(dictEntityOperations);
        }

        /// <inheritdoc />
        public override async Task MigrateAsync(string? targetMigration = null, CancellationToken cancellationToken = default)
        {
            MigrateGuard(targetMigration);
            var migrationDate = DateTime.UtcNow;

            try
            {
                var appliedMigrations = await _historyRepository.GetAppliedMigrationsAsync(cancellationToken).ConfigureAwait(false);
                _migrateContext = GetMigrateContext(appliedMigrations, migrationDate);
                if (_migrateContext.HasMigrations)
                {
                    await base.MigrateAsync(null, cancellationToken);
                }
                else
                {
                    var revertOperations = await StaticMigrationsService.GetRevertOperationsAsync(false, _migrateContext.MigrationDate, cancellationToken)
                        .ConfigureAwait(false);
                    var applyOperations = await StaticMigrationsService.GetApplyOperationsAsync(false, _migrateContext.MigrationDate, cancellationToken)
                        .ConfigureAwait(false);
                    var operations = revertOperations.Concat(applyOperations).ToList();

                    await ExecuteAsync(operations, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                _migrateContext = null;
            }
            var dictEntityOperations = await StaticMigrationsService.MigrateDictionaryEntitiesAsync(migrationDate, cancellationToken).ConfigureAwait(false);
            await ExecuteAsync(dictEntityOperations, cancellationToken).ConfigureAwait(false);
        }

        private void MigrateGuard(string? targetMigration)
        {
            if (targetMigration != null)
            {
                throw new ArgumentException("Migrate to targetMigration not supported", nameof(targetMigration));
            }
            if (_migrateContext != null)
            {
                throw new ArgumentException("Migration is already running", nameof(targetMigration));
            }
        }

        protected virtual MigrateContext GetMigrateContext(IEnumerable<HistoryRow> appliedMigrationEntries, DateTime migrationDate)
        {
            PopulateMigrations(appliedMigrationEntries.Select(t => t.MigrationId),
                string.Empty,
                out var migrationsToApply,
                out _,
                out _);

            return migrationsToApply.Count == 0
                ? new MigrateContext(string.Empty, string.Empty, migrationDate)
                : new MigrateContext(migrationsToApply.First().GetId(), migrationsToApply.Last().GetId(), migrationDate);
        }

        /// <inheritdoc />
        protected override IReadOnlyList<MigrationCommand> GenerateUpSql(Migration migration,
            MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
        {
            var migrationCommands = base.GenerateUpSql(migration, options);
            if (_migrateContext == null)
            {
                return migrationCommands;
            }
            var migrationId = migration.GetId();
            if (migrationId == _migrateContext.FirstMigrationId &&
                migrationId == _migrateContext.LastMigrationId)
            {
                //NOTE: Add revert static migrations at the beggining of first migration 
                var revertCommands = GenerateCommands(StaticMigrationsService.GetRevertOperations(true, _migrateContext.MigrationDate).ToList());
                //NOTE: Add apply static migrations at the end of last migration
                var applyCommands = GenerateCommands(StaticMigrationsService.GetApplyOperations(true, _migrateContext.MigrationDate).ToList());
                return revertCommands.Concat(migrationCommands).Concat(applyCommands).ToList();
            }
            if (migrationId == _migrateContext.FirstMigrationId)
            {
                //NOTE: Add revert static migrations at the beggining of first migration 
                var revertCommands = GenerateCommands(StaticMigrationsService.GetRevertOperations(true, _migrateContext.MigrationDate).ToList());
                return revertCommands.Concat(migrationCommands).ToList();
            }
            if (migrationId == _migrateContext.LastMigrationId)
            {
                //NOTE: Add apply static migrations at the end of last migration
                var applyCommands = GenerateCommands(StaticMigrationsService.GetApplyOperations(true, _migrateContext.MigrationDate).ToList());
                return migrationCommands.Concat(applyCommands).ToList();
            }
            return migrationCommands;
        }


        private IEnumerable<MigrationCommand> GenerateCommands(IReadOnlyList<MigrationOperation> operations)
        {
            return _migrationsSqlGenerator.Generate(operations);
        }

        private void Execute(IReadOnlyList<MigrationOperation> operations)
        {
            var commands = GenerateCommands(operations);
            _migrationCommandExecutor.ExecuteNonQuery(commands, _connection);
        }

        private async Task ExecuteAsync(IReadOnlyList<MigrationOperation> operations, CancellationToken cancellationToken = default)
        {
            if (operations.Count == 0)
            {
                return;
            }
            var commands = GenerateCommands(operations);
            await _migrationCommandExecutor.ExecuteNonQueryAsync(commands, _connection, cancellationToken).ConfigureAwait(false);
        }

        protected class MigrateContext
        {
            public MigrateContext()
            {
                FirstMigrationId = string.Empty;
                LastMigrationId = string.Empty;
                HasMigrations = false;
            }

            public MigrateContext(string firstMigrationId, string lastMigrationId, DateTime migrationDate)
            {
                FirstMigrationId = firstMigrationId;
                LastMigrationId = lastMigrationId;
                HasMigrations = true;
                MigrationDate = migrationDate;
            }

            public DateTime MigrationDate { get; }

            public string FirstMigrationId { get; }
            public string LastMigrationId { get; }
            public bool HasMigrations { get; }
        }
    }
}
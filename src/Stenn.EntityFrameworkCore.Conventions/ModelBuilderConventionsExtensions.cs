﻿#nullable enable
using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Stenn.Conventions.Contacts;

namespace Stenn.EntityFrameworkCore.Conventions
{
    public static class ModelBuilderConventionsExtensions
    {
        private static void AddCommonConventions(this IModelConventionBuilder builder)
        {
            builder.AddInterfaceConventionProperty<ICreateAuditedEntity>(x => x.Created,
                (_, _, p) => p.IsRequired()
                    .ValueGeneratedOnAdd()
                    .HasAnnotation(ConventionsAnnotationNames.SqlDefault_CurrentDateTime, true)
                    .HasComment("Row creation datetime. Configured by convention 'ICreateAuditedEntity'"));

            builder.AddInterfaceConventionProperty<IUpdateAuditedEntity>(x => x.ModifiedAt,
                (_, _, p) => p.IsRequired()
                    .ValueGeneratedOnAddOrUpdate()
                    .HasAnnotation(ConventionsAnnotationNames.SqlDefault_CurrentDateTime, true)
                    .HasAnnotation(ConventionsAnnotationNames.ColumnTriggerUpdate_SqlDefault, true)
                    .HasComment("Row last modified datetime. Updated by trigger. Configured by convention 'IUpdateAuditedEntity'"));

            builder.AddInterfaceConvention<ISoftDeleteEntity>(
                e =>
                {
                    e.HasAnnotation(ConventionsAnnotationNames.SoftDelete, true);

                    e.Property(typeof(bool), nameof(ISoftDeleteEntity.IsDeleted))
                        .IsRequired()
                        .HasDefaultValue(false)
                        .HasComment(
                            "Row deleted flag. Used for soft delete row. Updated by 'instead of' trigger. Configured by convention 'ISoftDeleteEntity'");

                    e.Property(typeof(DateTime?), nameof(ISoftDeleteEntity.Deleted))
                        .IsRequired(false)
                        .HasComment(
                            "Row deleted  datetime. Used for soft delete row. Updated by 'instead of' trigger. Configured by convention 'ISoftDeleteEntity'");

                    e.HasIndex(nameof(ISoftDeleteEntity.IsDeleted));
                    
                    var entityParam = Expression.Parameter(e.Metadata.ClrType, "x");
                    var deletedPropInfo = typeof(ISoftDeleteEntity).GetProperty(nameof(ISoftDeleteEntity.IsDeleted))!;
                    var propAccess = Expression.MakeMemberAccess(entityParam, deletedPropInfo);
                    var lambdaExpression = Expression.Lambda(
                        Expression.MakeBinary(ExpressionType.Equal, propAccess, Expression.Constant(false)),
                        entityParam);
                    e.HasQueryFilter(lambdaExpression);
                    
                });

            builder.AddInterfaceConventionProperty<IConcurrentAuditedEntity>(x => x.RowVersion,
                (_, _, p) => p.IsRequired()
                    .IsRowVersion()
                    .HasComment("Concurrent token(row version). Configured by convention 'IConcurrentAuditedEntity'"));

            builder.AddInterfaceConventionProperty<IEntityWithSourceSystemId>(x => x.SourceSystemId, (e, i, p) =>
            {
                p.IsRequired()
                    .HasMaxLength(50)
                    .HasValueGenerator<SourceSystemIdValueGenerator>()
                    .HasComment(
                        "Source system id. Row id for cross services' communication. Uses trigger on row insertion. Configured by convention 'IEntityWithSourceSystemId'");

                e.HasIndex(i.Name).IsUnique();
            });
        }


        public static void ApplyConventions(this ModelBuilder builder,
            Action<IModelConventionBuilder>? init = null,
            bool includeCommonConventions = true)
        {
            var convensionBuilder = new ModelConventionBuilder();

            if (includeCommonConventions)
            {
                convensionBuilder.AddCommonConventions();
            }
            init?.Invoke(convensionBuilder);

            convensionBuilder.Build(builder);
        }
    }
}
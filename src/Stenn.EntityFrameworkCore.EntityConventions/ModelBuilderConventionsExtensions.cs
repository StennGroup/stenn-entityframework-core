﻿#nullable enable

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Stenn.EntityConventions.Contacts;

namespace Stenn.EntityFrameworkCore.EntityConventions
{
    public static class ModelBuilderConventionsExtensions
    {
        public static void AddCommonConventions(this IEntityConventionsBuilder builder)
        {
            builder
                .AddCreateAudited()
                .AddUpdateAudited()
                .AddSoftDelete()
                .AddConcurrentAudited()
                .AddEntityWithSourceSystemId();
        }

        public static IEntityConventionsBuilder AddCreateAudited(this IEntityConventionsBuilder builder)
        {
            builder.AddInterfaceConventionProperty<ICreateAuditedEntity>(x => x.Created,
                (_, _, p) => p.IsRequired()
                    .ValueGeneratedOnAdd()
                    .HasAnnotation(ConventionsAnnotationNames.SqlDefault_CurrentDateTime, true)
                    .HasComment("Row creation datetime. Configured by convention 'ICreateAuditedEntity'"));
            return builder;
        }

        public static IEntityConventionsBuilder AddUpdateAudited(this IEntityConventionsBuilder builder)
        {
            builder.AddInterfaceConventionProperty<IUpdateAuditedEntity>(x => x.ModifiedAt,
                (_, _, p) => p.IsRequired()
                    .ValueGeneratedOnAddOrUpdate()
                    .HasAnnotation(ConventionsAnnotationNames.SqlDefault_CurrentDateTime, true)
                    .HasAnnotation(ConventionsAnnotationNames.ColumnTriggerUpdate_SqlDefault, true)
                    .HasComment("Row last modified datetime. Updated by trigger. Configured by convention 'IUpdateAuditedEntity'"));

            return builder;
        }

        public static IEntityConventionsBuilder AddSoftDelete(this IEntityConventionsBuilder builder)
        {
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

            return builder;
        }

        public static IEntityConventionsBuilder AddConcurrentAudited(this IEntityConventionsBuilder builder)
        {
            builder.AddInterfaceConventionProperty<IConcurrentAuditedEntity>(x => x.RowVersion,
                (_, _, p) => p.IsRequired()
                    .IsRowVersion()
                    .HasComment("Concurrent token(row version). Configured by convention 'IConcurrentAuditedEntity'"));

            return builder;
        }

        public static IEntityConventionsBuilder AddEntityWithSourceSystemId(this IEntityConventionsBuilder builder)
        {
            
            
            builder.AddInterfaceConventionProperty<IEntityWithSourceSystemId>(x => x.SourceSystemId, (e, i, p) =>
            {
                var options = e.Metadata.ClrType.GetCustomAttribute<SourceSystemIdOptions>() ?? builder.DefaultOptions.SourceSystemId;
                
                p.IsRequired()
                    .HasComment(
                        "Source system id. Row id for cross services' communication. Uses trigger on row insertion. Configured by convention 'IEntityWithSourceSystemId'");

                if (options.HasValueGenerator)
                {
                    p.HasValueGenerator(options.Generator ?? typeof(SourceSystemIdValueGenerator));
                }

#pragma warning disable EF1001
                if (p.Metadata.FindAnnotation(CoreAnnotationNames.MaxLength) is not { })
                {
                    p.HasMaxLength(options.MaxLength);
                }
                if (p.Metadata.FindAnnotation(CoreAnnotationNames.Unicode) is not { })
                {
                    p.IsUnicode(options.IsUnicode);
                }
#pragma warning restore EF1001
                if (options.HasIndex)
                {
                    e.HasIndex(i.Name).IsUnique();
                }
            });

            return builder;
        }
    }
}
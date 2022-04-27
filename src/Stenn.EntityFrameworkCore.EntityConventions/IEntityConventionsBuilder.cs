﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stenn.EntityConventions.Contacts;

namespace Stenn.EntityFrameworkCore.EntityConventions
{
    public interface IEntityConventionsBuilder
    {
        void Add(IEntityConvention convention);

        void AddInterfaceConvention<TConvention>(Action<EntityTypeBuilder> configure)
            where TConvention:IEntityConventionContract;
        
        void AddInterfaceConventionProperty<TConvention>(Expression<Func<TConvention, object?>> propertyExpression,
            Action<EntityTypeBuilder, PropertyInfo, PropertyBuilder> configure)
            where TConvention:IEntityConventionContract;
        
        EntityConventionsCommonDefaultsOptions DefaultOptions { get; }
    }
}
﻿using System;

namespace Stenn.EntityConventions.Contacts.TriggerBased
{
    /// <summary>
    /// Entity with creation audited property Modified
    /// </summary>
    public interface ISoftDeleteEntityConvention : IEntityConventionContract
    {
        bool IsDeleted => throw ExceptionHelper.ThrowRegistrationOnly();
        DateTime? Deleted => throw ExceptionHelper.ThrowRegistrationOnly();
    }
}
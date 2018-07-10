﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Catalog.Infrastructure.UnitOfWork
{
    public interface IUnitOfWork
    {
        IRepository<T> GetRepository<T>() where T : class;
    }
}

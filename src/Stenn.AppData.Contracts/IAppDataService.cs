using System.Linq;
using System;

namespace Stenn.AppData.Contracts
{
    [Obsolete("Please use IAppDataServiceClient and IAppDataServiceServer")]
    public interface IAppDataService<in TBaseEntity>
        where TBaseEntity : IAppDataEntity
    {
        IQueryable<T> Query<T>() where T : class, TBaseEntity;
    }
}
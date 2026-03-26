using Cards.Domain.Common;

namespace Cards.Infrastructure.Common.Interfaces;

public interface IRepository<T> where T : IEntity;
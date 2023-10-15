using database.Repository.Base;
using database.Models;

namespace database.Repository;

public interface IMetricRepository : IDataRepository<Metric>
{
}

public class MetricRepository : DataRepositoryBase<Metric>, IMetricRepository
{
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using System.Reflection;
using Tippser.Core.Entities;
using Tippser.Core.Interfaces;
using Tippser.Infrastructure.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Tippser.Infrastructure.Repositories
{
    public class BaseRepository : IBaseRepository
    {
        private readonly ILogger _logger;
        private readonly TippserDbContext _db;

        public BaseRepository(ILogger<BaseRepository> logger, TippserDbContext db)
        {
            _logger = logger;
            _db = db;   
        }

        public async Task<TEntity> Create<TEntity>(TEntity item) 
            where TEntity : BaseEntity
        {
            try
            {
                _db.Add(item);
                await _db.SaveChangesAsync();
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        public async Task<Person> Create(Person item)
        {
            try
            {
                _db.Add(item);
                await _db.SaveChangesAsync();
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        public async Task<IEnumerable<TEntity?>> Read<TEntity>(bool? active = null)
            where TEntity : BaseEntity
        {
            try
            {
                var dbSet = _db.Set<TEntity>();
                var query = dbSet.AsQueryable().Where(e => active == null || e.Active == active);
                query = IncludeNavProperties(query, typeof(TEntity));

                var items = await query.ToListAsync();
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        public async Task<IEnumerable<Person?>> Read(bool? active = null)
        {
            try
            {
                var dbSet = _db.Set<Person>();
                var query = dbSet.AsQueryable().Where(e => active == null || e.Active == active);
                var properties = typeof(Person)
                    .GetProperties()
                    .Where(p => p.Name == nameof(Person.CreatedBy) ||
                                p.Name == nameof(Person.ModifiedBy) ||
                                p.Name == nameof(Person.Bets));

                foreach (var property in properties)
                {
                    query = query.Include(property.Name);
                }

                var items = await query.ToListAsync();
                return items;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        public async Task<TEntity?> Read<TEntity>(string id, bool? active = null)
            where TEntity : BaseEntity
        {
            try
            {
                var dbSet = _db.Set<TEntity>();
                var item = await dbSet.Where(e => e.Id == id).Where(e => active == null || e.Active == active).FirstOrDefaultAsync();
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        public async Task<Person?> Read(string id, bool? active = null)
        {
            try
            {
                var dbSet = _db.Set<Person>();
                var item = await dbSet.Where(e => e.Id == id).Where(e => active == null || e.Active == active).FirstOrDefaultAsync();
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        public async Task<TEntity> Update<TEntity>(TEntity item)
            where TEntity : BaseEntity
        {
            try
            {
                _db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await _db.SaveChangesAsync();
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        public async Task<Person> Update(Person item)
        {
            try
            {
                _db.Entry(item).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await _db.SaveChangesAsync();
                return item;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        public async Task Delete<TEntity>(TEntity item)
            where TEntity : BaseEntity
        {
            try
            {
                _db.Remove(item);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        public async Task Delete(Person item)
        {
            try
            {
                _db.Remove(item);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        private static IEnumerable<string> ExcludedProperties()
        {
            return new List<string>
            {
                nameof(BaseEntity.Id),
                nameof(BaseEntity.CreatedUtc),
                nameof(BaseEntity.CreatedBy),
                nameof(BaseEntity.ModifiedUtc),
                nameof(BaseEntity.ModifiedBy),
            };
        }

        public async Task<IEnumerable<TEntity>?> Find<TEntity>(TEntity item)
            where TEntity : BaseEntity
        {
            try
            {
                var dbSet = _db.Set<TEntity>();
                var query = dbSet.Select(e => e);

                var itemProperties = GetItemProperties(item);
                var filterExpressions = BuildFilterExpressions<TEntity>(itemProperties);

                foreach (var filterExpression in filterExpressions)
                {
                    query = query.Where(filterExpression);
                }

                var result = await query.ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        private Dictionary<string, object?> GetItemProperties<TEntity>(TEntity item)
        {
            var properties = item!.GetType().GetProperties();
            return properties
                .Where(p =>
                    p.GetValue(item) != null &&
                    !ExcludedProperties().Any(x => p.Name.Contains(x)) &&
                    !IsCollectionType(p.PropertyType)
                )
                .ToDictionary(p => p.Name, p => p.GetValue(item));
        }

        private bool IsCollectionType(Type type)
        {
            return typeof(IEnumerable<dynamic>).IsAssignableFrom(type) ||
                   typeof(List<dynamic>).IsAssignableFrom(type) ||
                   typeof(ICollection<dynamic>).IsAssignableFrom(type);
        }

        private IEnumerable<Expression<Func<TEntity, bool>>> BuildFilterExpressions<TEntity>(Dictionary<string, object?> itemProperties)
        {
            foreach (var property in itemProperties)
            {
                var parameter = Expression.Parameter(typeof(TEntity), "entity");
                var propertyInfo = typeof(TEntity).GetProperty(property.Key);
                var propertyExpression = Expression.Property(parameter, propertyInfo!);
                var equalityExpression = Expression.Equal(propertyExpression, Expression.Constant(property.Value));

                var lambda = Expression.Lambda<Func<TEntity, bool>>(equalityExpression, parameter);
                yield return lambda;
            }
        }

        private IQueryable<TEntity> IncludeNavProperties<TEntity>(IQueryable<TEntity> query, Type type)
            where TEntity : BaseEntity
        {
            IEnumerable<PropertyInfo> properties = new List<PropertyInfo>();
            if (type == typeof(Person))
            {
                properties = type
                    .GetProperties()
                    .Where(p => p.Name == nameof(Person.Bets));
            }
            else
            {
                properties = type
                    .GetProperties()
                    .Where(p => p.GetGetMethod()!.IsVirtual);
            }

            foreach (var property in properties)
            {
                switch (typeof(TEntity).Name)
                {
                    case nameof(Competition):
                        var specificQuery = query as IQueryable<Competition>;
                        var includedQuery = specificQuery!
                            .Include(t => t.Teams)
                            .ThenInclude(r => r.Group);
                        query = (IQueryable<TEntity>)includedQuery;
                        break;
                    default:
                        query = query.Include(property.Name);
                        break;
                }
            }

            return query;
        }
    }
}
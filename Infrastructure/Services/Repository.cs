using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Services
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly SiteDataContext _db;
        private readonly DbSet<T> Entity = null;
        public Repository(SiteDataContext db)
        {
            _db = db;
            Entity = _db.Set<T>();
        }
        #region main operation
        public void Add(T entity)
        {
            Entity.Add(entity);
        }

        public void AddRange(List<T> entities)
        {
            Entity.AddRange(entities);
        }


        public void Update(T entity)
        {

            Entity.Attach(entity);
            _db.Entry(entity).State = EntityState.Modified;
        }
        public void Delete(T entity)
        {
            Entity.Remove(entity);
        }

        public void DeleteRange(params T[] entities)
        {
            //_repo.DeleteRange(itemDb.ToArray())
            //
            Entity.RemoveRange(entities);
        }
        public void DeletelistRange(List<T> entities)
        {
            Entity.RemoveRange(entities);
        }

        #endregion

        #region GetALL Async =>(IQueryable)
        public IQueryable<T> GetAll()
        {

            return Entity;
        }

        public IQueryable<T> GetAll(params Expression<Func<T, object>>[] includes)
        {
            var result = Entity.Where(i => true);

            foreach (var includeExpression in includes)
                result = result.Include(includeExpression);

            return result;
        }

        public IQueryable<T> GetAll(Expression<Func<T, bool>> whereCondition)
        {
            return Entity.Where(whereCondition);
        }

        public IQueryable<T> GetAll(Expression<Func<T, bool>> whereCondition, params Expression<Func<T, object>>[] includes)
        {
            var result = Entity.Where(whereCondition);


            foreach (var includeExpression in includes)
                result = result.Include(includeExpression);
            return result;
        }

        #endregion

        #region GetALL Async =>(IEnumerable)

        public async Task<IEnumerable<T>> GetAllAsync(bool NoTracking = false)
        {
            if (NoTracking)
                return await Entity.AsNoTracking().ToListAsync();
            else
                return await Entity.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(bool NoTracking = false, params Expression<Func<T, object>>[] includes)
        {
            var result = Entity.Where(i => true);
            foreach (var includeExpression in includes)
                result = result.Include(includeExpression);
            // return await result.ToListAsync();
            if (NoTracking)
                return await result.AsNoTracking().ToListAsync();
            else
                return await result.ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> whereCondition, bool NoTracking = false)
        {
            if (NoTracking)
                return await Entity.Where(whereCondition).AsNoTracking().ToListAsync();
            else
                return await Entity.Where(whereCondition).ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> whereCondition, bool NoTracking = false, params Expression<Func<T, object>>[] includes)
        {
            var result = Entity.Where(whereCondition);
            foreach (var includeExpression in includes)
                result = result.Include(includeExpression);
            //return await result.ToListAsync();
            if (NoTracking)
                return await result.AsNoTracking().ToListAsync();
            else
                return await result.ToListAsync();
        }



        #endregion

        #region GetById Async  

        public async Task<T> GetByIdAsync(Guid id)
        {
            return await Entity.FindAsync(id);

        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await Entity.FindAsync(id);

        }

        #endregion

        #region  SingleOrDefaultAsync
        public async Task<T> SingleOrDefaultAsync()
        {
            return await Entity.FirstOrDefaultAsync();
        }
        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> whereCondition)
        {
            return await Entity.Where(whereCondition).FirstOrDefaultAsync();
        }

        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> whereCondition, params Expression<Func<T, object>>[] includes)
        {
            var result = Entity.Where(whereCondition);
            foreach (var includeExpression in includes)
                result = result.Include(includeExpression);

            return await result.FirstOrDefaultAsync();
        }

        public async Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> whereCondition, bool NoTacking = false, params Expression<Func<T, object>>[] includes)
        {
            var result = Entity.Where(whereCondition);
            foreach (var includeExpression in includes)
                result = result.Include(includeExpression);

            if (NoTacking)
                return await result.AsNoTracking().FirstOrDefaultAsync();
            else
                return await result.FirstOrDefaultAsync();
        }

        #endregion


        #region OrderBy
        public IEnumerable<T> OrderBy(Expression<Func<T, bool>> whereCondition)
        {
            return Entity.OrderBy(whereCondition);
        }
        public IEnumerable<T> OrderByDescending(Expression<Func<T, bool>> whereCondition)
        {
            return Entity.OrderByDescending(whereCondition);
        }
        #endregion


        #region SaveAllAsync
        public async Task<bool> SaveAllAsync()
        {


            return await _db.SaveChangesAsync() > 0;

        }


        #endregion

        #region checkState
        public bool checkState(T entity, string state)
        {
            return (_db.Entry(entity).State.ToString().ToLower() == state.ToLower().Trim()) ? true : false;
        }

        public DbSet<T> GetContext()
        {
            return Entity;

        }

        public async Task<T> FirstAsync()
        {
            return await Entity.FirstOrDefaultAsync();
        }

        public void UpdateRange(List<T> entities)
        {
            Entity.UpdateRange(entities);

        }




        #endregion

        #region Get Current Identity




        public ICollection<TType> GetColmounAsync<TType>(Expression<Func<T, TType>> LamdaExpression) where TType : class
        {
            return Entity.Select(LamdaExpression).ToList();

        }


        public ICollection<TType> GetCoulmnAsync<TType>(Expression<Func<T, bool>> where, Expression<Func<T, TType>> select) where TType : class
        {
            return Entity.Where(where).Select(select).ToList();
        }

        #endregion


        #region Count

        public async Task<int> GetCountAsync()
        {
            return await Entity.CountAsync();
        }


        public async Task<int> GetCountAsync(Expression<Func<T, bool>> whereCondition)
        {
            return await Entity.Where(whereCondition).CountAsync();

        }

        public bool Any(Expression<Func<T, bool>> whereCondition)
        {
            return Entity.Any(whereCondition);
        }



        #endregion

        #region NextId
        public TResult ExecuteSqlQuery<TResult>(string sqlQuery)
        {
            // Ensure the database connection is open
            using (var connection = _db.Database.GetDbConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = sqlQuery;
                    command.CommandType = System.Data.CommandType.Text;

                    // Execute the command and convert the result to TResult
                    var result = command.ExecuteScalar();
                    return (TResult)Convert.ChangeType(result, typeof(TResult));
                }
            }
        }

        #endregion

    }
}
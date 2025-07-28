using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        #region main operation
        void Add(T entity);
        void AddRange(List<T> entities);
        void Update(T entity);
        void UpdateRange(List<T> entities);
        void Delete(T entity);
        void DeleteRange(params T[] entities);
        void DeletelistRange(List<T> entities);
        #endregion

        #region GetById Async
        Task<T> GetByIdAsync(Guid id);

        Task<T> GetByIdAsync(int id);


        #endregion

        #region GetALL Async =>(IEnumerable)
        Task<IEnumerable<T>> GetAllAsync(bool NoTracking = false);
        Task<IEnumerable<T>> GetAllAsync(bool NoTracking = false,params Expression<Func<T, object>>[] includes);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> whereCondition, bool NoTracking = false);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> whereCondition, bool NoTracking = false, params Expression<Func<T, object>>[] includes);

        #endregion

        #region  GetALL Async =>(IQueryable)
        IQueryable<T> GetAll();

        IQueryable<T> GetAll(params Expression<Func<T, object>>[] includes);

        IQueryable<T> GetAll(Expression<System.Func<T, bool>> whereCondition);

        IQueryable<T> GetAll(Expression<System.Func<T, bool>> whereCondition, params Expression<Func<T, object>>[] includes);

        #endregion

        #region SingleOrDefault Async
        Task<T> SingleOrDefaultAsync();
        Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> whereCondition);
        Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> whereCondition, params Expression<Func<T, object>>[] includes);
        Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> whereCondition, bool NoTacking = false, params Expression<Func<T, object>>[] includes);

        #endregion

        #region OrderBy
        IEnumerable<T> OrderBy(Expression<Func<T, bool>> whereCondition);
        IEnumerable<T> OrderByDescending(Expression<Func<T, bool>> whereCondition);
        #endregion

        #region SaveAllAsync
        Task<bool> SaveAllAsync();

        #endregion

        #region checkState
        public bool checkState(T entity, string state);
        public DbSet<T> GetContext();
        public bool Any(Expression<Func<T, bool>> whereCondition);
        #endregion
        #region Get first
        Task<T> FirstAsync();


        #endregion

        #region Get Current Identity
      
          ICollection<TType> GetColmounAsync<TType>(Expression<Func<T, TType>> LamdaExpression) where TType : class;

        ICollection<TType> GetCoulmnAsync<TType>(Expression<Func<T, bool>> where, Expression<Func<T, TType>> select) where TType : class;
        #endregion

        #region NextId

        public TResult ExecuteSqlQuery<TResult>(string sqlQuery);

        #endregion



    }
}
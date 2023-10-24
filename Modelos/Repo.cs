using System;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using DashBoard.Data;

namespace DashBoard.Modelos
{
	public class Repo<TEntity, TDataContext> : ApiFiltroGet<TEntity>
        where TEntity : class
        where TDataContext : DbContext
    {
        protected readonly TDataContext context;
        internal DbSet<TEntity> dbset;

        private readonly ApplicationDbContext _appDbContext;
        MyFunc myFunc = new();
        

        public Repo(TDataContext dataContext,
            ApplicationDbContext appDbContext)
        {
            context = dataContext;
            dbset = context.Set<TEntity>();
            _appDbContext = appDbContext;
        }

        public virtual async Task<bool> DeleteEntity(TEntity entityToDel)
        {
            if (context.Entry(entityToDel).State == EntityState.Detached)
            {
                dbset.Attach(entityToDel);
            }
            dbset.Remove(entityToDel);
            return await context.SaveChangesAsync() >= 1;
        }
        public virtual async Task<bool> DeleteEntity(object id)
        {
            try
            {
                TEntity entityToDel = await dbset.FindAsync(id);
                return await DeleteEntity(entityToDel);
            }
            catch (Exception ex)
            {
                string etxt = $"Error al intentar Borrar un registro REPO<> {ex}";
                var logTmp = myFunc.MakeLog("SistemaUserID", "SistemaOrgId",etxt ,
                    "SistemaCorp", "Sistema");
                await _appDbContext.LogsBitacora.AddAsync(logTmp);
                await _appDbContext.SaveChangesAsync();
                return false;
            }

        }

        public virtual async Task<IEnumerable<TEntity>>Get(Expression<Func<TEntity, bool>> filtro = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>
            orderby = null, string propiedades = "")
        {
            try
            {
                IQueryable<TEntity> querry = dbset;
                if (filtro != null)
                {
                    querry = querry.Where(filtro);
                }
                foreach (var propiedad in propiedades.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    querry = querry.Include(propiedad);
                }
                if (orderby != null)
                {
                    return orderby(querry).ToList();
                }
                else
                {
                    return await querry.ToListAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<IEnumerable<TEntity>> GetAll()
        {
            try
            {
                return await dbset.ToListAsync();
                //await Task.Delay(1);
                //return dbset;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public virtual async Task<TEntity> GetById(object id)
        {
            try
            {
                return await dbset.FindAsync(id);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public virtual async Task<TEntity> Insert(TEntity entity)
        {
            try
            {
                await dbset.AddAsync(entity);
                await context.SaveChangesAsync();
                return entity;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<IEnumerable<TEntity>> InsertPlus(IEnumerable<TEntity> entities)
        {
            try
            {
                await dbset.AddRangeAsync(entities);
                await context.SaveChangesAsync();
                return entities;
            }
            catch (Exception)
            {
                throw;
            }

        }
        public virtual async Task<TEntity> Update(TEntity entityToUpdate)
        {
            try
            {
                var dbSet = context.Set<TEntity>();
                dbSet.Attach(entityToUpdate);
                context.Entry(entityToUpdate).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return entityToUpdate;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Aqui agregue las funciones que yo hice

        public virtual async Task<IEnumerable<TEntity>> UpdatePlus(IEnumerable<TEntity> entitiesToUpdate)
        {
            try
            {
                var dbSet = context.Set<TEntity>();
                foreach (var entityToUpdate in entitiesToUpdate)
                {
                    dbSet.Attach(entityToUpdate);
                    context.Entry(entityToUpdate).State = EntityState.Modified;
                }
                await context.SaveChangesAsync();
                return entitiesToUpdate;
            }
            catch (Exception)
            {
                throw;
            }
        
        }

        public virtual async Task<bool> DeletePlus(IEnumerable<TEntity> entitiesToDelete)
        {
            try
            {
                foreach (var entityToDelete in entitiesToDelete)
                {
                    if (context.Entry(entityToDelete).State == EntityState.Detached)
                    {
                        dbset.Attach(entityToDelete);
                    }
                    dbset.Remove(entityToDelete);
                }

                return await context.SaveChangesAsync() >= entitiesToDelete.Count();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public virtual async Task<int> GetCount(Expression<Func<TEntity, bool>> filtro = null)
        {
            try
            {
                IQueryable<TEntity> querry = dbset;
                if (filtro != null)
                {
                    querry = querry.Where(filtro);
                }

                int count = await querry.CountAsync();
                return count;
            }
            catch (Exception)
            {
                throw;
            }
        }

        
    }
}


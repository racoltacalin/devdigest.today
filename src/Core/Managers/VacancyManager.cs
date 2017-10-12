﻿using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using System.Threading.Tasks;
using X.PagedList;

namespace Core.Managers
{
    public class VacancyManager : ManagerBase
    {
        public VacancyManager(string connectionString, IMemoryCache cache)
            : base(connectionString, cache)
        {
        }

        public async Task<IPagedList<DAL.Vacancy>> GetVacancies(int page = 1, int pageSize = 10)
        {
            var key = $"vacancy{page}_{pageSize}";

            var result = _cache.Get(key) as IPagedList<DAL.Vacancy>;

            if (result == null)
            {
                var skip = (page - 1) * pageSize;

                var items = _database
                    .Vacancy
                    .OrderByDescending(o => o.Id)
                    .Skip(skip)
                    .Take(pageSize).ToList();

                var totalItemsCount = await _database.Publication.CountAsync();

                result = new StaticPagedList<Vacancy>(items, page, pageSize, totalItemsCount);
                _cache.Set(key, result, GetMemoryCacheEntryOptions());
            }

            return result;
        }

        public async Task<DAL.Vacancy> Get(int id)
        {
            var key = $"vacancy_{id}";

            var result = _cache.Get(key) as Vacancy;

            if (result == null)
            {
                result = _database.Vacancy.SingleOrDefault(o => o.Id == id);
                _cache.Set(key, result, GetMemoryCacheEntryOptions());
            }

            return await Task.FromResult(result);
        }

        public async Task<Vacancy> Save(Vacancy vacancy)
        {
            _database.Add(vacancy);
            await _database.SaveChangesAsync();
            vacancy = _database.Vacancy.LastOrDefault();

            return vacancy;
        }
    }
}

using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public UserRepository(DataContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await this.context.Users.Where(x => x.UserName == username)
                .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            var query = this.context.Users.AsQueryable(); 

            query = query.Where(x=>x.UserName!=userParams.CurrentUsername);
            query = query.Where(x => x.Gender == userParams.Gender);

            var minDateOfBirth = DateTime.Today.AddYears(-userParams.MaxAge - 1);
            var maxDateOfBirth = DateTime.Today.AddYears(-userParams.MinAge);
            query = query.Where(x=>x.BirthDate>=minDateOfBirth && x.BirthDate<=maxDateOfBirth);

            query = userParams.OrderBy switch
            {
                "created" => query.OrderByDescending(x => x.Created),
                _ => query.OrderByDescending(x => x.LastActive),
            };

            return await PagedList<MemberDto>.CreateAsync(
                query.ProjectTo<MemberDto>(mapper.ConfigurationProvider).AsNoTracking(), 
                userParams.PageNumber, 
                userParams.PageSize);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await this.context.Users
                .Include(x=>x.Photos)
                .ToListAsync();
        }

        public async Task<AppUser> GetUsersByIdAsync(int id)
        {
            return await this.context.Users
                .Include(x => x.Photos)
                .FirstOrDefaultAsync(x=>x.Id==id);
        }

        public async Task<AppUser> GetUsersByUsernameAsync(string username)
        {
            return await this.context.Users
                .Include(x => x.Photos)
                .FirstOrDefaultAsync(x => x.UserName == username);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await this.context.SaveChangesAsync()>0;
        }

        public void Update(AppUser user)
        {
           this.context.Entry(user).State = EntityState.Modified;
        }
    }
}

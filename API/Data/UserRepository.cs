using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<MemberDto>> GetMembersAsync()
        {
            return await this.context.Users
                .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                .ToListAsync();
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

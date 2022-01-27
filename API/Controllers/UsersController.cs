using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private DataContext context;

        public UsersController(DataContext context)
        {
            this.context = context;
        }

        // GET: api/<UsersController>
        [HttpGet]
        public IEnumerable<AppUser> Get()
        {
            var users =  this.context.Users.ToList();
            return users;
        }

        // GET api/<UsersController>/5
        [HttpGet("{id}")]
        public AppUser Get(int id)
        {
            var user = this.context.Users.FirstOrDefault(x=>x.Id==id);
            return user;
        }

        // POST api/<UsersController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<UsersController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<UsersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}

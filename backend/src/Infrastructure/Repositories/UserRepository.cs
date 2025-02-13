using Domain.Models;  // Para a classe User
using Domain.Services; // Para IRepository<>
using Infrastructure.Data;  // Para o AppDbContext
using Microsoft.EntityFrameworkCore;

public class UserRepository : IRepository<User>
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User> Add(User entity)
    {
        await _context.Users.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

public async Task<User> Update(Guid id, User entity)
{
    _context.Users.Update(entity);
    await _context.SaveChangesAsync();
    return entity;
}

    public async Task<bool> Delete(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<User?> GetById(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<IEnumerable<User>> GetAll()
    {
        return await _context.Users.ToListAsync();
    }
}
